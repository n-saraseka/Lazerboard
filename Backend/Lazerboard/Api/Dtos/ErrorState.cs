namespace Lazerboard.Api.Dtos;

public enum ErrorState
{
    SelfReport,
    UserIsRestricted,
    UserIsAlreadyReported,
    TooManyFalseReports,
    TooManyReports,
    OnCooldown
}