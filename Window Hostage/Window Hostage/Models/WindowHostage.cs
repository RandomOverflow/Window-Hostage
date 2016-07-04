using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Window_Hostage
{
    public class WindowHostage
    {
        public static List<WindowInfo> GetMainWindows(Process[] exclusions)
        {
            List<WindowInfo> listWindows = new List<WindowInfo>();
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    if (process.MainWindowTitle == string.Empty || process.HasExited) continue;
                }
                catch (Exception)
                {
#if DEBUG
                    {
                        Debug.WriteLine("ERROR WHILE GETTING PROCESS (NAME: " + process.ProcessName + ")");
                    }
#endif
                    continue;
                }

                if (exclusions.Any(s => s.Id == process.Id)) continue;

                listWindows.Add(new WindowInfo(process.MainWindowHandle));
            }
            return listWindows;
        }
    }
}