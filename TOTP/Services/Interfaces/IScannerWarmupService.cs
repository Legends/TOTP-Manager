namespace TOTP.Services.Interfaces;

public interface IScannerWarmupService
{
    void StartWarmupInBackground(string trigger);
}

