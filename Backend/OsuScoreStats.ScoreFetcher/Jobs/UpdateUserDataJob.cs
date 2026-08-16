using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OsuScoreStats.Data.Database.Entities;
using OsuScoreStats.Data.Database.Repositories.Interfaces;
using OsuScoreStats.ScoreFetcher.OsuEntityToDtoService;
using OsuScoreStats.ScoreFetcher.Processing;
using Quartz;

namespace OsuScoreStats.ScoreFetcher.Jobs;

public class UpdateUserAndScoreDataJob : IJob
{
    private IApiFetcher _apiFetcher;
    private IUserRepository _userRepository;
    private IScoreRepository _scoreRepository;
    private IScorePendingDeletionRepository _scorePendingDeletionRepository;
    private IOsuEntityToDtoService _entityToDtoService;
    private readonly double _confirmDeletionThreshold;

    public UpdateUserAndScoreDataJob(IApiFetcher apiFetcher,
        IUserRepository userRepository,
        IScoreRepository scoreRepository,
        IScorePendingDeletionRepository scorePendingDeletionRepository,
        IOsuEntityToDtoService entityToDtoService,
        IConfiguration configuration)
    {
        _apiFetcher = apiFetcher;
        _userRepository = userRepository;
        _scoreRepository = scoreRepository;
        _scorePendingDeletionRepository = scorePendingDeletionRepository;
        _entityToDtoService = entityToDtoService;
        
        _confirmDeletionThreshold = configuration.GetValue<double>("DeleteScoresAfter");
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        await CheckPendingRemovedScores();
        await UpdateUserData();
    }

    private async Task UpdateUserData()
    {
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
        });
        _scorePendingDeletionRepository.CreateBulk(scoresMarkedForDeletion);
        
        var userDtos = apiUsers.Select(_entityToDtoService.UserEntityToDto).ToList();
        _userRepository.UpdateBulk(userDtos);
        await _userRepository.SaveChangesAsync();
    }

    private async Task CheckPendingRemovedScores()
    {
        var pendingScores = await _scorePendingDeletionRepository.GetAllWithUserData();
        // We want to check users with pending scores that are past the confirmation threshold
        pendingScores = pendingScores
            .Where(s => DateTime.UtcNow - s.MarkedAt >= TimeSpan.FromDays(_confirmDeletionThreshold))
            .ToList();
        var userIds = pendingScores.Select(s => s.Score.User.Id).Distinct().ToList();
        
        var apiUsers = await _apiFetcher.GetUsersAsync(userIds);
        var existingUserIds = apiUsers.Select(u => u.Id).ToList();
        
        var scoresToRemove = pendingScores
            .Where(s => !existingUserIds.Contains(s.Score.UserId))
            .Select(s => s.Score)
            .ToList();
        
        _scoreRepository.DeleteBulk(scoresToRemove);
        await _scoreRepository.SaveChangesAsync();
    }
}