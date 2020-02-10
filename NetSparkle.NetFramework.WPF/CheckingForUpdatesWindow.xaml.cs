using NetSparkle.Interfaces;
using System;
using System.Windows;

namespace NetSparkle.UI.NetFramework.WPF
{
    /// <summary>
    /// Interaction logic for CheckingForUpdatesWindow.xaml
    /// </summary>
    public partial class CheckingForUpdatesWindow : Window, ICheckingForUpdates
    {
        public event EventHandler UpdatesUIClosing;

        public CheckingForUpdatesWindow()
        {
            InitializeComponent();
            Closing += CheckingForUpdatesWindow_Closing;
        }

        private void CheckingForUpdatesWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= CheckingForUpdatesWindow_Closing;
            UpdatesUIClosing?.Invoke(sender, new EventArgs());
        }

        void ICheckingForUpdates.Close()
        {
            Close();
        }

        void ICheckingForUpdates.Show()
        {
            Show();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
