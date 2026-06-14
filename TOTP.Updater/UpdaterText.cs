using System.Globalization;

namespace TOTP.Updater;

public static class UpdaterText
{
    private static bool IsGerman => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase);

    public static string WindowTitle => IsGerman ? "TOTP Manager Updater" : "TOTP Manager Updater";
    public static string HeaderTitle => IsGerman ? "TOTP Manager Updates" : "TOTP Manager Updates";
    public static string HeaderDescription => IsGerman
        ? "Das signierte Update-Paket wird angewendet und die App neu gestartet."
        : "Applying the signed update package and relaunching the app.";
    public static string CurrentStep => IsGerman ? "Aktueller Schritt" : "Current step";
    public static string OverallProgress => IsGerman ? "Gesamtfortschritt" : "Overall progress";
    public static string Close => IsGerman ? "Schliessen" : "Close";

    public static string InstallingUpdate => IsGerman ? "Update wird installiert" : "Installing update";
    public static string PreparingUpdater => IsGerman ? "Updater wird vorbereitet..." : "Preparing updater...";
    public static string ClosingApp => IsGerman ? "TOTP Manager wird geschlossen..." : "Closing TOTP Manager...";
    public static string WaitingForAppClose => IsGerman ? "Warte, bis die App geschlossen ist..." : "Waiting for the app to close...";
    public static string StagingPackage => IsGerman ? "Update-Paket wird vorbereitet..." : "Staging update package...";
    public static string InstallingFiles => IsGerman ? "Dateien werden installiert..." : "Installing files...";
    public static string RelaunchingApp => IsGerman ? "App wird neu gestartet..." : "Relaunching app...";
    public static string FinalizingCopiedFiles => IsGerman ? "Kopierte Dateien werden abgeschlossen" : "Finalizing copied files";
    public static string Complete100 => IsGerman ? "100% abgeschlossen" : "100% complete";
    public static string UpdateFailedTitle => IsGerman ? "Update fehlgeschlagen" : "Update failed";
    public static string UpdateFailedStatus => IsGerman ? "Das Update konnte nicht installiert werden." : "The update could not be installed.";
    public static string StartupFailedTitle => WindowTitle;
    public static string StartupFailedMessageFormat => IsGerman
        ? "Der Update-Installer konnte nicht gestartet werden.\n\n{0}"
        : "The update installer could not start.\n\n{0}";

    public static string FilesQueued(int count) => IsGerman ? $"{count} Datei(en) in Warteschlange" : $"{count} file(s) queued";
    public static string ItemsQueued(int count) => IsGerman ? $"{count} Element(e) in Warteschlange" : $"{count} item(s) queued";
    public static string FileCountProgress(int copiedFiles, int totalFiles) => IsGerman ? $"{copiedFiles}/{totalFiles} Datei(en)" : $"{copiedFiles}/{totalFiles} file(s)";
    public static string PercentComplete(int percentage) => IsGerman ? $"{percentage}% abgeschlossen" : $"{percentage}% complete";
}
