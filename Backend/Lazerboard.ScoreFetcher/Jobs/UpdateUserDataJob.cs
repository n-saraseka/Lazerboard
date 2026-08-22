using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.OsuEntityToDtoService;
using Lazerboard.ScoreFetcher.Processing;
using Quartz;

namespace Lazerboard.ScoreFetcher.Jobs;

public class UpdateUserAndScoreDataJob : IJob
{
    private IApiFetcher _apiFetcher;
    private IUserRepository _userRepository;
    private IScoreRepository _scoreRepository;
    private IScorePendingDeletionRepository _scorePendingDeletionRepository;
    private IOsuEntityToDtoService _entityToDtoService;
    private ILogger<UpdateUserAndScoreDataJob> _logger;
    private readonly double _confirmDeletionThreshold;

    public UpdateUserAndScoreDataJob(IApiFetcher apiFetcher,
        IUserRepository userRepository,
        IScoreRepository scoreRepository,
        IScorePendingDeletionRepository scorePendingDeletionRepository,
        IOsuEntityToDtoService entityToDtoService,
        ILogger<UpdateUserAndScoreDataJob> logger,
        IConfiguration configuration)
    {
        _apiFetcher = apiFetcher;
        _userRepository = userRepository;
        _scoreRepository = scoreRepository;
        _scorePendingDeletionRepository = scorePendingDeletionRepository;
        _entityToDtoService = entityToDtoService;
        _logger = logger;
        
        _confirmDeletionThreshold = configuration.GetValue<double>("DeleteScoresAfter");
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await CheckPendingRemovedScores();
            await UpdateUserData();
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Critical, ex, "Quartz job failed!");
        }
    }

    private async Task UpdateUserData()
    {
        _logger.Log(LogLevel.Information, "Updating user data...");
        var users = await _userRepository.GetAll().ToListAsync();
        var userIds = users.Select(u => u.Id).ToList();

        var apiUsers = await _apiFetcher.GetUsersAsync(userIds);
        var existingUserIds = apiUsers.Select(u => u.Id).ToList();
        
        // We want to mark scores from users who may have been restricted at the moment for deletion
        var scoresToRemove = await _scoreRepository.GetAll()
            .Where(s => !existingUserIds.Contains(s.UserId))
            .ToListAsync();
        
        var markedAt = DateTime.UtcNow;
        var scoresMarkedForDeletion = scoresToRemove.Select(s => new ScorePendingDeletion
            {
                MarkedAt = markedAt,
                ScoreId = s.Id
            })
            .ToList();
        _scorePendingDeletionRepository.CreateBulk(scoresMarkedForDeletion);
        _logger.Log(LogLevel.Information, "Marked {markedScoresCount} for deletion", scoresMarkedForDeletion.Count);
        
        var userDtos = apiUsers.Select(_entityToDtoService.UserEntityToDto).ToList();
        _userRepository.UpdateBulk(userDtos);
        await _userRepository.SaveChangesAsync();
    }

    private async Task CheckPendingRemovedScores()
    {
        _logger.Log(LogLevel.Information, "Checking scores marked for deletion...");
        var pendingScores = await _scorePendingDeletionRepository.GetAllWithUserData();
        
        // If a score has a PP value this high, it's likely the user has been restricted for good, and we can check them ASAP.
        // Hopefully top players won't push the boundaries of PP to this extreme...
        var suspiciousScores = pendingScores
            .Where(s =>
                s.Score.PP >= 3000 
                && DateTime.UtcNow - s.MarkedAt >= TimeSpan.FromDays(_confirmDeletionThreshold / 4))
            .ToList();
        // Otherwise procedure is the same.
        var scoresPastTheThreshold = pendingScores
            .Where(s => DateTime.UtcNow - s.MarkedAt >= TimeSpan.FromDays(_confirmDeletionThreshold))
            .ToList();

        var scoresToCheck = suspiciousScores
            .Concat(scoresPastTheThreshold)
            .DistinctBy(s => s.Id)
            .ToList();
        
        var userIds = scoresToCheck.Select(s => s.Score.User.Id).Distinct().ToList();
        
        var apiUsers = await _apiFetcher.GetUsersAsync(userIds);
        var existingUserIds = apiUsers.Select(u => u.Id).ToList();
        
        // Remove scores from users who are still restricted.
        var scoresToRemove = scoresToCheck
            .Where(s => !existingUserIds.Contains(s.Score.UserId))
            .Select(s => s.Score)
            .ToList();

        // Leave scores from users who are not restricted anymore.
        var scoresToLeave = scoresToCheck
            .Where(s => existingUserIds.Contains(s.Score.UserId))
            .ToList();
        
        _scorePendingDeletionRepository.DeleteBulk(scoresToLeave);
        _scoreRepository.DeleteBulk(scoresToRemove);
        
        var usersIdsToRemove = scoresToRemove.Select(s => s.UserId).Distinct().ToList();
        var usersToRemove = await _userRepository.GetBulkAsync(usersIdsToRemove);
        _userRepository.DeleteBulk(usersToRemove);
        
        await _scoreRepository.SaveChangesAsync();
        _logger.Log(LogLevel.Information, "Removed {deletedScoresCount} scores from {deletedUsersCount} users", 
            scoresToRemove.Count, usersToRemove.Count);
    }
}