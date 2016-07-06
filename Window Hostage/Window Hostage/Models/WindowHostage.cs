using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Window_Hostage
{
    public class WindowHostage
    {
        public static List<WindowInfo> GetMainWindows(Process[] exclusions)
        {
            return
                Process.GetProcesses()
                    .Where(process => exclusions.All(s => s.Id != process.Id))
                    .Select(process => new WindowInfo(process.MainWindowHandle))
                    .Where(windowInfo => windowInfo.Title.Length > 0)
                    .ToList();
        }
    }
}