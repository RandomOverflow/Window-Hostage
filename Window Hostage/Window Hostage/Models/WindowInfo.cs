﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Window_Hostage
{
    public class WindowInfo : INotifyPropertyChanged
    {
        public enum Status
        {
            None,
            Visible,
            Hidden
        }

        /// <summary>
        ///     Enumeration of the different ways of showing a window using
        ///     ShowWindow
        /// </summary>
        public enum WindowShowStyle : uint
        {
            /// <summary>Hides the window and activates another window.</summary>
            /// <remarks>See SW_HIDE</remarks>
            Hide = 0,

            /// <summary>
            ///     Activates and displays a window. If the window is minimized
            ///     or maximized, the system restores it to its original size and
            ///     position. An application should specify this flag when displaying
            ///     the window for the first time.
            /// </summary>
            /// <remarks>See SW_SHOWNORMAL</remarks>
            ShowNormal = 1,

            /// <summary>Activates the window and displays it as a minimized window.</summary>
            /// <remarks>See SW_SHOWMINIMIZED</remarks>
            ShowMinimized = 2,

            /// <summary>Activates the window and displays it as a maximized window.</summary>
            /// <remarks>See SW_SHOWMAXIMIZED</remarks>
            ShowMaximized = 3,

            /// <summary>Maximizes the specified window.</summary>
            /// <remarks>See SW_MAXIMIZE</remarks>
            Maximize = 3,

            /// <summary>
            ///     Displays a window in its most recent size and position.
            ///     This value is similar to "ShowNormal", except the window is not
            ///     actived.
            /// </summary>
            /// <remarks>See SW_SHOWNOACTIVATE</remarks>
            ShowNormalNoActivate = 4,

            /// <summary>
            ///     Activates the window and displays it in its current size
            ///     and position.
            /// </summary>
            /// <remarks>See SW_SHOW</remarks>
            Show = 5,

            /// <summary>
            ///     Minimizes the specified window and activates the next
            ///     top-level window in the Z order.
            /// </summary>
            /// <remarks>See SW_MINIMIZE</remarks>
            Minimize = 6,

            /// <summary>
            ///     Displays the window as a minimized window. This value is
            ///     similar to "ShowMinimized", except the window is not activated.
            /// </summary>
            /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
            ShowMinNoActivate = 7,

            /// <summary>
            ///     Displays the window in its current size and position. This
            ///     value is similar to "Show", except the window is not activated.
            /// </summary>
            /// <remarks>See SW_SHOWNA</remarks>
            ShowNoActivate = 8,

            /// <summary>
            ///     Activates and displays the window. If the window is
            ///     minimized or maximized, the system restores it to its original size
            ///     and position. An application should specify this flag when restoring
            ///     a minimized window.
            /// </summary>
            /// <remarks>See SW_RESTORE</remarks>
            Restore = 9,

            /// <summary>
            ///     Sets the show state based on the SW_ value specified in the
            ///     STARTUPINFO structure passed to the CreateProcess function by the
            ///     program that started the application.
            /// </summary>
            /// <remarks>See SW_SHOWDEFAULT</remarks>
            ShowDefault = 10,

            /// <summary>
            ///     Windows 2000/XP: Minimizes a window, even if the thread
            ///     that owns the window is hung. This flag should only be used when
            ///     minimizing windows from a different thread.
            /// </summary>
            /// <remarks>See SW_FORCEMINIMIZE</remarks>
            ForceMinimized = 11
        }

        private Status _currentStatus;
        private string _title;

        public WindowInfo(IntPtr handle)
        {
            Handle = handle;
            Title = GetWindowTitle();
            int pid;
            GetWindowThreadProcessId((int) handle, out pid);
            ProcessOwner = Process.GetProcessById(pid);
            CurrentStatus = IsWindowVisible(handle) ? Status.Visible : Status.Hidden;
        }

        public Process ProcessOwner { get; }
        public IntPtr Handle { get; }

        public string Title
        {
            get { return _title; }
            private set
            {
                if (_title == value) return;

                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public Status CurrentStatus
        {
            get { return _currentStatus; }
            private set
            {
                if (_currentStatus == value) return;
                _currentStatus = value;
                OnPropertyChanged("CurrentStatus");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [DllImport("user32")]
        private static extern uint GetWindowThreadProcessId(
            int hWnd,
            out int lpdwProcessId
            );

        public static int GetWindowProcessId(int hwnd)
        {
            int pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return pid;
        }

        public event EventHandler WindowExited;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        public void UpdateStatus()
        {
            if (!IsWindow(Handle))
            {
                //FINESTRA NON ESISTENTE PIU
                CurrentStatus = Status.None;
                WindowExited?.Invoke(this, null);
            }
            else
            {
                CurrentStatus = IsWindowVisible(Handle) ? Status.Visible : Status.Hidden;
                Title = GetWindowTitle();
            }
        }

        private string GetWindowTitle()
        {
            StringBuilder windowText = new StringBuilder(1024);
            GetWindowText(Handle, windowText, 1024);
            return windowText.ToString();
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

        public bool ShowWindow(WindowShowStyle nCmdShow)
        {
            return ShowWindow(Handle, nCmdShow);
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}