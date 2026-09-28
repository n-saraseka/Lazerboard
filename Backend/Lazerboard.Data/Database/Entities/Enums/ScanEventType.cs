namespace Lazerboard.Data.Database.Entities.Enums;

public enum ScanEventType
{
    RescanStarted,
    RescanFinished,
    MainSeedingStarted,
    MainSeedingFinished,
    SecondarySeedingStarted,
    SecondarySeedingFinished,
    UserScanStarted,
    UserScanFinished,
    UserCheckStarted,
    UserCheckFinished,
    RestrictedCheckStarted,
    RestrictedCheckFinished,
    ScanStarted, // These and rescan variables are backwards. ScanStarted is used for the rescans service.
    ScanFinished
}