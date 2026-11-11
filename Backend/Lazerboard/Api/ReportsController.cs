using Lazerboard.Api.Dtos;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Api;

[ApiController]
[Route("api/[controller]")]
public class ReportsController: ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserReportsRepository _userReportsRepository;
    private readonly ILogger _logger;
    private readonly IUserCooldownRepository _userCooldownRepository;
    
    private readonly TimeSpan _reportsWindow;
    private readonly TimeSpan _spamReportCooldown;
    private readonly TimeSpan _falseReportCooldown;
    private readonly int _spamReportsMax;
    private readonly int _falseReportsMax;
    
    public ReportsController(IUserRepository userRepository,
        IUserReportsRepository userReportsRepository,
        ILogger<ReportsController> logger,
        IUserCooldownRepository userCooldownRepository,
        IConfiguration config)
    {
        _userRepository = userRepository;
        _userReportsRepository = userReportsRepository;
        _logger = logger;
        _userCooldownRepository = userCooldownRepository;

        var reportsConfig = config.GetSection("Reports");
        _reportsWindow = TimeSpan.FromMinutes(reportsConfig.GetValue<double>("ReportsWindow"));
        _spamReportCooldown = TimeSpan.FromMinutes(reportsConfig.GetValue<double>("SpamReportCooldown"));
        _falseReportCooldown = TimeSpan.FromMinutes(reportsConfig.GetValue<double>("FalseReportCooldown"));
        _spamReportsMax = reportsConfig.GetValue<int>("SpamReportsMax");
        _falseReportsMax = reportsConfig.GetValue<int>("FalseReportsMax");
    }

    private int UserId => int.Parse(HttpContext.User.Identity!.Name!);
    
    /// <summary>
    /// Report a <see cref="User"/>
    /// </summary>
    /// <param name="reportCommand">The <see cref="ReportCommand"/></param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> ReportUser([FromBody] ReportCommand reportCommand, CancellationToken ct = default)
    {
        var currentTime = DateTimeOffset.UtcNow;
        var currentCooldown = await _userCooldownRepository.GetByUserIdAsync(UserId, ct);

        if (currentCooldown != null && currentTime < currentCooldown.ExpiresOn)
        {
            return BadRequest(new ReportError
            {
                Code = ErrorState.OnCooldown,
                Message = "Your report ability is on a cooldown, please wait."
            });
        }
        
        if (reportCommand.UserId == UserId)
        {
            return BadRequest(new ReportError
            {
                Code = ErrorState.SelfReport,
                Message = "You can't report yourself."
            });
        }
        
        var user = await _userRepository.GetByIdAsync(reportCommand.UserId, ct);
        if (user is null)
        {
            return NotFound("User does not exist.");
        }

        if (user.IsRestricted)
        {
            return BadRequest(new ReportError
            {
                Code = ErrorState.UserIsRestricted,
                Message = "User is already restricted or deleted."
            });
        }
        var inProgressUserReport = await _userReportsRepository.GetInProgressUserReportAsync(reportCommand.UserId, ct);
        if (inProgressUserReport != null)
        {
            return Conflict(new ReportError
            {
                Code = ErrorState.UserIsAlreadyReported,
                Message = "User has already been reported recently."
            });
        }
        
        var lastReportsMadeByUser = await _userReportsRepository.GetRecentReportsDoneByUserAsync(UserId, 50, ct);
        var falseRecentReports = lastReportsMadeByUser.Count(r => r.Status == ReportStatus.IsNotRestricted);
        if (falseRecentReports >= _falseReportsMax)
        {
            currentTime = DateTimeOffset.UtcNow;
            var cooldown = new UserCooldown
            {
                UserId = UserId,
                StartedOn = currentTime,
                ExpiresOn = currentTime.Add(_falseReportCooldown),
                CooldownReason = CooldownReason.TooManyReports
            };
            _userCooldownRepository.Create(cooldown);
            await _userCooldownRepository.SaveChangesAsync(ct);
            
            return BadRequest(new ReportError
            {
                Code = ErrorState.TooManyFalseReports,
                Message = "You've made too many false reports recently, please wait."
            });
        }
        
        var recentReportsMadeByUser = await _userReportsRepository.GetRecentReportsDoneByUserAsync(UserId, _reportsWindow, ct);
        var recentReports = recentReportsMadeByUser.Count;
        if (recentReports >= _spamReportsMax)
        {
            currentTime = DateTimeOffset.UtcNow;
            var cooldown = new UserCooldown
            {
                UserId = UserId,
                StartedOn = currentTime,
                ExpiresOn = currentTime.Add(_spamReportCooldown),
                CooldownReason = CooldownReason.TooManyReports
            };
            _userCooldownRepository.Create(cooldown);
            await _userCooldownRepository.SaveChangesAsync(ct);
            
            return BadRequest(new ReportError
            {
                Code = ErrorState.TooManyReports,
                Message = "You've made too many reports recently, please wait."
            });
        }

        var report = new UserReport
        {
            ReportedOn = DateTimeOffset.UtcNow,
            ReportedUserId = reportCommand.UserId,
            ReportingUserId = UserId
        };

        try
        {
            _userReportsRepository.Create(report);
            await _userReportsRepository.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            _logger.Log(LogLevel.Warning, "Multiple people tried to report user {userId} at the same time!", reportCommand.UserId);
            return Conflict(new ReportError
            {
                Code = ErrorState.UserIsAlreadyReported,
                Message = "User has already been reported recently."
            });
        }

        return Created();
    }
}