using System;
using System.Collections.Generic;
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
        private int RefreshMs { get; }

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
                //PER DETECTARE FINESTRE NON PIU ESISTENTI CARICATE NELLA DATAGRID

                foreach (WindowInfo loadedWindow in _windowInfos.ToArray())
                {
                    loadedWindow.UpdateStatus();
                }

                //
                List<WindowInfo> newWindowInfos = WindowHostage.GetMainWindows(new[] {Process.GetCurrentProcess()});
                foreach (WindowInfo newWindowInfo in newWindowInfos.ToArray())
                {
                    WindowInfo currentWindowInfo = _windowInfos.FirstOrDefault(a => a.Handle == newWindowInfo.Handle);

                    if (currentWindowInfo != null)
                    {
                        if (currentWindowInfo.CurrentStatus != newWindowInfo.CurrentStatus)
                            currentWindowInfo.UpdateStatus();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke((Action) (() => _windowInfos.Add(newWindowInfo)));
                        newWindowInfo.WindowExited += WindowExited;
#if DEBUG
                        Debug.WriteLine("WINDOW ADDED (Handle: " + newWindowInfo.Handle + ")");
#endif
                    }
                }

                Thread.Sleep(RefreshMs);
            } while (AutoRefresh);
        }

        private
            void MenuItemShowHide_Click(object sender, RoutedEventArgs e)
        {
            WindowInfo selectedWindow = (WindowInfo) DataGridWindows.SelectedItem;
            selectedWindow.ShowWindow(selectedWindow.CurrentStatus == WindowInfo.Status.Visible
                ? WindowInfo.WindowShowStyle.Hide
                : WindowInfo.WindowShowStyle.Show);
        }
    }
}