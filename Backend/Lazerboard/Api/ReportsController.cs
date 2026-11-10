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
public class ReportsController(IUserRepository userRepository,
    IUserReportsRepository userReportsRepository,
    ILogger<ReportsController> logger): ControllerBase
{
    private readonly TimeSpan _reportsWindow = TimeSpan.FromHours(1);
    private const int SpamReportsMax = 25;
    private const int FalseReportsMax = 10;

    private int UserId => int.Parse(HttpContext.User.Identity!.Name!);
    
    [Authorize]
    [HttpPost("")]
    public async Task<IActionResult> ReportUser([FromBody] ReportCommand reportCommand, CancellationToken ct = default)
    {
        if (reportCommand.UserId == UserId)
        {
            return BadRequest(new ReportError
            {
                Code = ErrorState.SelfReport,
                Message = "You can't report yourself"
            });
        }
        
        var user = await userRepository.GetByIdAsync(reportCommand.UserId, ct);
        if (user is null)
        {
            return NotFound("User does not exist");
        }

        if (user.IsRestricted)
        {
            return BadRequest(new ReportError
            {
                Code = ErrorState.UserIsRestricted,
                Message = "User is already restricted or deleted"
            });
        }
        var inProgressUserReport = await userReportsRepository.GetInProgressUserReport(reportCommand.UserId, ct);
        if (inProgressUserReport != null)
        {
            return Conflict(new ReportError
            {
                Code = ErrorState.UserIsAlreadyReported,
                Message = "User has already been reported recently"
            });
        }

        var recentReportsMadeByUser = await userReportsRepository.GetRecentReportsDoneByUser(UserId, _reportsWindow, ct);
        var falseRecentReports = recentReportsMadeByUser.Count(r => r.Status == ReportStatus.IsNotRestricted);
        var recentReports = recentReportsMadeByUser.Count;
        
        if (falseRecentReports >= FalseReportsMax)
            return BadRequest(new ReportError
            {
                Code = ErrorState.TooManyFalseReports,
                Message = "You've made too many false reports recently"
            });
        
        if (recentReports >= SpamReportsMax)
        {
            return BadRequest(new ReportError
            {
                Code = ErrorState.TooManyFalseReports,
                Message = "You've made too many reports recently, please wait"
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
            userReportsRepository.Create(report);
            await userReportsRepository.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.Log(LogLevel.Warning, "Multiple people tried to report user {userId} at the same time!", reportCommand.UserId);
            return Conflict(new ReportError
            {
                Code = ErrorState.UserIsAlreadyReported,
                Message = "User has already been reported recently"
            });
        }

        return Created();
    }
}