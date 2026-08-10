using System.IO;

namespace MizanFinance.App.Services;

public static class DbPathProvider
{
    public static string GetDatabasePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MizanFinance");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "mizanfinance.db");
    }

    public static string GetDocumentsFolder()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MizanFinance", "Documents");
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetBackupFolder()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MizanFinance", "Backups");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
