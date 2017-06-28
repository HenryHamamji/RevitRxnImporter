using System;
using System.IO;
using System.Reflection;

namespace RevitReactionImporter
{
    internal class PathUtils
    {
        internal static string GetDllPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        internal static string GetPath(string file)
        {
            return Path.Combine(GetDllPath(), file);
        }

        internal static string GetHistoryFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = string.Format(@"RevitRxnImporter\history-{0}.txt", projectId);
            return Path.Combine(folder, path);
        }

        internal static string GetHistoryModelFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = string.Format(@"RevitRxnImporter\history-model-{0}.txt", projectId);
            return Path.Combine(folder, path);
        }

        internal static Uri GetResourceUri(string resourceName)
        {
            return new Uri(string.Format("/RevitReactionImporterApp;component/Resources/{0}", resourceName), UriKind.RelativeOrAbsolute);
        }

        internal static string GetPreferencesFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = string.Format(@"RevitRxnImporter\preferences-{0}.txt", projectId);
            return Path.Combine(folder, path);
        }

        internal static void EnsurePreferencesDirectoryExists()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(folder, "RevitRxnImporter");

            if (Directory.Exists(dir))
                return;

            Directory.CreateDirectory(dir);
        }

        internal static string GetLogFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(folder, @"RevitRxnImporter\logs");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir, string.Format("Project-{0}.log", projectId));
        }

        internal static string GetUserFreandlyLogFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(folder, @"RevitRxnImporter\logs");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var path = Path.Combine(dir, string.Format("Project-{0}.csv", projectId));

            //if (!File.Exists(path))

            return path;
        }

        //internal static string GetChangesLogFile(string projectId)
        //{
        //    var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        //    var dir = Path.Combine(folder, @"RevitRxnImporter\logs");
        //    if (!Directory.Exists(dir))
        //        Directory.CreateDirectory(dir);

        //    var path = Path.Combine(dir, string.Format("Project-{0}-changes.csv", projectId));

        //    if (!File.Exists(path))
        //        File.WriteAllText(path, Properties.Resources.LOGGER_CHANGES_FILE_HEADER + Environment.NewLine);

        //    return path;
        //}
    }
}
