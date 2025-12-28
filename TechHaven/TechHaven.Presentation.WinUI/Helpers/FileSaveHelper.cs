using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class FileSaveHelper
    {
        public static string GetDownloadsPath()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var downloads = Path.Combine(userProfile ?? string.Empty, "Downloads");
            return downloads;
        }

        public static async Task<string> SavePdfToDownloadsAsync(byte[] bytes, string fileName)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var downloads = GetDownloadsPath();
            if (!Directory.Exists(downloads)) Directory.CreateDirectory(downloads);

            var safeName = string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));
            var fullPath = Path.Combine(downloads, safeName);

            await File.WriteAllBytesAsync(fullPath, bytes).ConfigureAwait(false);
            return fullPath;
        }

        public static void OpenFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
