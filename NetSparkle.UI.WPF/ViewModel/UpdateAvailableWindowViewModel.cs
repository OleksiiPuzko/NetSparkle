using NetSparkle.Enums;
using NetSparkle.Events;
using NetSparkle.UI.WPF.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace NetSparkle.UI.WPF.ViewModel
{
    public class UpdateAvailableWindowViewModel : ViewModelBase, IUpdateAvailableWindowViewModel
    {
        private Sparkle sparkle;
        private List<AppCastItem> updates;
        private bool isUpdateAlreadyDownloaded;
        private string titleHeader;
        private string infoText;
        private string skipButtonContent;
        private string remindMeLaterButtonContent;
        private string downloadInstallButtonContent;
        private string myHtml;
        private UpdateAvailableResult _userResponse;
        private ReleaseNotesGrabber releaseNotesGrabber;
        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;
        private string separatorTemplate = "<div style=\"border: #ccc 1px solid;\">" +
            "<div style=\"background: {3}; padding: 5px;\">" +
            "<span style=\"float: right; display:float;\">" +
                    "{1}</span>{0}</div><div style=\"padding: 5px;\">{2}</div></div><br>";
        private string htmlHeadAddition = "<style>body { background-color: grey;  overflow: auto;  scrollbar-base-color: grey; }</style>";
        public UpdateAvailableWindowViewModel()
        {
            DebugInit();
        }

        public UpdateAvailableWindowViewModel(Sparkle sparkle, List<AppCastItem> updates, bool isUpdateAlreadyDownloaded)
        {
            this.sparkle = sparkle;
            this.updates = updates;
            this.isUpdateAlreadyDownloaded = isUpdateAlreadyDownloaded;
            releaseNotesGrabber = new ReleaseNotesGrabber(separatorTemplate, htmlHeadAddition, sparkle);


            DebugInit();

            LoadReleaseNotes(updates);
        }

        public string TitleHeader
        {
            get => titleHeader;
            set
            {
                this.titleHeader = value;
                RaisePropertyChanged(nameof(this.TitleHeader));
            }
        }

        public string InfoText
        {
            get => infoText; set
            {
                this.infoText = value;
                RaisePropertyChanged(nameof(this.InfoText));
            }
        }

        public string SkipButtonContent
        {
            get => skipButtonContent;
            set
            {
                skipButtonContent = value;
                RaisePropertyChanged(nameof(this.SkipButtonContent));
            }
        }

        public string RemindMeLaterButtonContent
        {
            get => remindMeLaterButtonContent;
            set
            {
                remindMeLaterButtonContent = value;
                RaisePropertyChanged(nameof(this.RemindMeLaterButtonContent));
            }
        }

        public string MyHtml
        {
            get => myHtml;
            set
            {
                myHtml = value;
                RaisePropertyChanged(nameof(this.MyHtml));
            }
        }
        public UpdateAvailableResult Result { get; }
        public AppCastItem CurrentItem
        {
            get { return this.updates.Count() > 0 ? this.updates[0] : null; }
        }

        public string DownloadInstallButtonContent
        {
            get => downloadInstallButtonContent;
            set
            {
                downloadInstallButtonContent = value;
                RaisePropertyChanged(nameof(this.DownloadInstallButtonContent));
            }
        }

        private async void LoadReleaseNotes(List<AppCastItem> items)
        {
            AppCastItem latestVersion = items.OrderByDescending(p => p.Version).FirstOrDefault();

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            string releaseNotes = await releaseNotesGrabber.DownloadAllReleaseNotesAsHTML(items, latestVersion, cancellationToken);
            MyHtml = releaseNotes;
        }

        private void DebugInit()
        {
            TitleHeader = string.Format("A new version of {0} is available.", "the application");
            InfoText = string.Format("{0} is now available (you have {1}). Would you like to {2} it now?", "AppName", "versionString", "downloadInstallText");

            SkipButtonContent = "Skip this version";
            RemindMeLaterButtonContent = "Remind me later";
            DownloadInstallButtonContent = "Download/Install";

            MyHtml = releaseNotesGrabber.GetLoadingText();
        }

        private void SendResponse(UpdateAvailableResult response)
        {
            _userResponse = response;
            //UserResponded?.Invoke(this, new UpdateResponseArgs(_userResponse, CurrentItem));
            cancellationTokenSource?.Cancel();
        }
    }

    public class WebBrowserHelper
    {
        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.RegisterAttached("Body", typeof(string), typeof(WebBrowserHelper), new PropertyMetadata(OnBodyChanged));

        public static string GetBody(DependencyObject dependencyObject)
        {
            return (string)dependencyObject.GetValue(BodyProperty);
        }

        public static void SetBody(DependencyObject dependencyObject, string body)
        {
            dependencyObject.SetValue(BodyProperty, body);
        }

        private static void OnBodyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webBrowser = (WebBrowser)d;
            webBrowser.NavigateToString((string)e.NewValue);
        }
    }
}
