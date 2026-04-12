using System.Diagnostics;

namespace MauiApp1.Scripts
{
    static class ProcessHelpers
    {
        public static bool IsProcessRunning(string exeName)
        {
            // usuwamy .exe jeśli ktoś poda
            if (OperatingSystem.IsWindows())
            {
                string processName = Path.GetFileNameWithoutExtension(exeName);

                return Process.GetProcessesByName(processName)?.Any() ?? false;
            }
            else
            {
                return false;
            }
        }
        public static List<string> GetUserProcesses()
        {
            var result = new List<string>();

            if (!OperatingSystem.IsWindows())
            {
                return result;
            }

            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    if (process.MainModule == null)
                        continue;
                    string exePath = process.MainModule.FileName;

                    // filtr 1: katalog Windows → systemowy
                    if (exePath.StartsWith(
                            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                            StringComparison.OrdinalIgnoreCase))
                        continue;

                    // filtr 2: znane procesy systemowe
                    if (IsSystemProcess(process.ProcessName))
                        continue;

                    result.Add(Path.GetFileName(exePath));
                }
                catch
                {
                    // brak dostępu do procesu
                }
            }

            return result.Distinct().OrderBy(x => x).ToList();
        }

        private static bool IsSystemProcess(string name)
        {
            string[] systemProcesses =
            {
                "svchost",
                "explorer",
                "services",
                "lsass",
                "winlogon",
                "csrss",
                "smss",
                "dwm",
                "fontdrvhost",
                "spoolsv",
                "taskhostw"
            };

            return systemProcesses.Contains(name, StringComparer.OrdinalIgnoreCase);
        }
    }
}
