using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Window_Hostage
{
    public partial class MainWindow
    {
        private readonly ObservableCollectionEx<WindowInfo> _windowInfos;

        public MainWindow()
        {
            InitializeComponent();
            AutoRefresh = true;
            RefreshMs = 1000;
            _windowInfos = new ObservableCollectionEx<WindowInfo>();
        }

        private bool AutoRefresh { get; }
        private int RefreshMs { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridWindows.ItemsSource = _windowInfos;
            Thread thread = new Thread(ReloadWindows) {IsBackground = true};
            thread.Start();
        }

        public void WindowExited(object sender, EventArgs args)
        {
            Dispatcher.BeginInvoke((Action) (() => _windowInfos.Remove((WindowInfo) sender)));
#if DEBUG
            Debug.WriteLine("WINDOW CLOSED (Handle: " + ((WindowInfo) sender).Handle + ")");
#endif
        }

        private void ReloadWindows()
        {
            do
            {
                WindowInfo[] tempWindowInfos = new WindowInfo[_windowInfos.Count];
                Array.Copy(_windowInfos.ToArray(), tempWindowInfos, tempWindowInfos.Length);
                foreach (WindowInfo loadedWindow in tempWindowInfos)
                {
                    loadedWindow.UpdateStatus();
                }

                List<WindowInfo> newWindowInfos = WindowHostage.GetMainWindows(new[] {Process.GetCurrentProcess()});
                foreach (WindowInfo newWindowInfo in newWindowInfos)

                {
                    if (tempWindowInfos.FirstOrDefault(a => a.Handle == newWindowInfo.Handle) != null) continue;
                    Dispatcher.BeginInvoke((Action) (() => _windowInfos.Add(newWindowInfo)));
                    newWindowInfo.WindowExited += WindowExited;
#if DEBUG
                    Debug.WriteLine("WINDOW ADDED (Handle: " + newWindowInfo.Handle + ")");
#endif
                }

                Thread.Sleep(RefreshMs);
            } while (AutoRefresh);
        }

        private
            void MenuItemShowHide_Click(object sender, RoutedEventArgs e)
        {
            WindowInfo selectedWindow = (WindowInfo) DataGridWindows.SelectedItem;
            selectedWindow?.ShowWindow(selectedWindow.CurrentStatus == WindowInfo.Status.Visible
                ? WindowInfo.WindowShowStyle.Hide
                : WindowInfo.WindowShowStyle.Show);
        }

        private void SliderUpdateDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RefreshMs = (int) SliderUpdateDelay.Value;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            int nWindowsHidden = _windowInfos.Count(windowInfo => windowInfo.CurrentStatus == WindowInfo.Status.Hidden);
            if (nWindowsHidden <= 0) return;
            if (MessageBox.Show("By exit " + nWindowsHidden + "windows will return visible.", "Closing",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information) == MessageBoxResult.Cancel)
                e.Cancel = true;
            else
                foreach (WindowInfo windowInfo in _windowInfos)
                {
                    if (windowInfo.CurrentStatus == WindowInfo.Status.Hidden)
                        windowInfo.ShowWindow(WindowInfo.WindowShowStyle.Show);
                }
        }
    }
}