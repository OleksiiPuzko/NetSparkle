using NetSparkle.Enums;
using NetSparkle.Interfaces;
using NetSparkle.UI.WPF.Interfaces;
using System.Threading;
using System.Windows;

namespace NetSparkle.UI.WPF.View
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindowView.xaml
    /// </summary>
    public partial class UpdateAvailableWindowView : Window, IUpdateAvailable
    {
        private bool isOnMainThread;

        public UpdateAvailableWindowView(IUpdateAvailableWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            ViewModel = viewModel;
        }

        public IUpdateAvailableWindowViewModel ViewModel { get; }
        public UpdateAvailableResult Result { get { return ViewModel.Result; } }
        public AppCastItem CurrentItem { get { return ViewModel.CurrentItem; } }

        public event UserRespondedToUpdate UserResponded;

        public void BringToFront()
        {
        }

        public void HideReleaseNotes()
        {
        }

        public void HideRemindMeLaterButton()
        {
        }

        public void HideSkipButton()
        {
        }

        public void Show(bool IsOnMainThread)
        {
            try
            {
                Show();
                isOnMainThread = IsOnMainThread;
                if (!IsOnMainThread)
                {
                    // https://stackoverflow.com/questions/1111369/how-do-i-create-and-show-wpf-windows-on-separate-threads
                    System.Windows.Threading.Dispatcher.Run();
                }
            }
            catch (ThreadAbortException)
            {
                Close();
                if (!IsOnMainThread)
                {
                    Dispatcher.InvokeShutdown();
                }
            }
        }
    }
}
