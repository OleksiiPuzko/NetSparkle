﻿using NetSparkle.Enums;
using NetSparkle.Events;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace NetSparkle.UI.NetFramework.WPF
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindow.xaml
    /// </summary>
    public partial class UpdateAvailableWindow : Window, IUpdateAvailable, INotifyPropertyChanged
    {
        private Sparkle _sparkle;
        private List<AppCastItem> _updates;
        private ReleaseNotesGrabber _releaseNotesGrabber;

        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        private bool _isOnMainThread;
        private bool _hasInitiatedShutdown;

        private UpdateAvailableResult _userResponse;
        private string releaseNotesHtml;

        public UpdateAvailableWindow()
        {
            InitializeComponent();
            Closing += UpdateAvailableWindow_Closing;
            _userResponse = UpdateAvailableResult.None;
        }

        private void UpdateAvailableWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= UpdateAvailableWindow_Closing;
            if (!_isOnMainThread && !_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
                Dispatcher.InvokeShutdown();
            }
        }

        public void Initialize(Sparkle sparkle, List<AppCastItem> items, bool isUpdateAlreadyDownloaded = false,
            string separatorTemplate = "", string headAddition = "")
        {
            _sparkle = sparkle;
            _updates = items;

            _releaseNotesGrabber = new ReleaseNotesGrabber(separatorTemplate, headAddition, sparkle);

            ReleaseNotesBrowser.AllowDrop = false;

            AppCastItem item = items.FirstOrDefault();

            // TODO: string translations
            TitleHeader.Text = string.Format("A new version of {0} is available.", item?.AppName ?? "the application");
            var downloadInstallText = isUpdateAlreadyDownloaded ? "install" : "download";
            if (item != null)
            {
                var versionString = "";
                try
                {
                    // Use try/catch since Version constructor can throw an exception and we don't want to
                    // die just because the user has a malformed version string
                    Version versionObj = new Version(item.AppVersionInstalled);
                    versionString = NetSparkle.Utilities.GetVersionString(versionObj);
                }
                catch
                {
                    versionString = "?";
                }
                InfoText.Text = string.Format("{0} is now available (you have {1}). Would you like to {2} it now?", item.AppName, versionString, downloadInstallText);
            }
            else
            {
                InfoText.Text = string.Format("Would you like to {0} it now?", downloadInstallText);
            }

            bool isUserMissingCriticalUpdate = items.Any(x => x.IsCriticalUpdate);
            RemindMeLaterButton.IsEnabled = isUserMissingCriticalUpdate == false;
            SkipButton.IsEnabled = isUserMissingCriticalUpdate == false;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            DispatcherHelper.BeginInvoke(
                        DispatcherPriority.Background,
                        () =>
                        {
                            ReleaseNotesHtml = _releaseNotesGrabber.GetLoadingText();
                            ReleaseNotesBrowser.Visibility = Visibility.Visible;
                        });

            LoadReleaseNotes(items);
        }

        private async void LoadReleaseNotes(List<AppCastItem> items)
        {
            AppCastItem latestVersion = items.OrderByDescending(p => p.Version).FirstOrDefault();
            string releaseNotes = await _releaseNotesGrabber.DownloadAllReleaseNotesAsHTML(items, latestVersion, _cancellationToken);

            DispatcherHelper.BeginInvoke(
                        DispatcherPriority.Background,
                        () =>
                        {
                            ReleaseNotesHtml = releaseNotes;
                            ReleaseNotesBrowser.Visibility = Visibility.Visible;
                        });
        }

        UpdateAvailableResult IUpdateAvailable.Result => _userResponse;

        AppCastItem IUpdateAvailable.CurrentItem => CurrentItem;

        public AppCastItem CurrentItem
        {
            get { return _updates.Count() > 0 ? _updates[0] : null; }
        }

        public string ReleaseNotesHtml
        {
            get => releaseNotesHtml; set
            {
                releaseNotesHtml = value;
                RaisePropertyChanged(nameof(this.ReleaseNotesHtml));
            }
        }

        public event UserRespondedToUpdate UserResponded;

        void IUpdateAvailable.BringToFront()
        {
            Topmost = true;
            Activate();
            Topmost = false;
        }

        void IUpdateAvailable.Close()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            // make sure to close the window on the thread it has been started on
            Dispatcher.InvokeAsync(() =>
            {
                Close();
                if (!_isOnMainThread && !_hasInitiatedShutdown)
                {
                    _hasInitiatedShutdown = true;
                    Dispatcher.InvokeShutdown();
                }
            });
        }

        void IUpdateAvailable.HideReleaseNotes()
        {
            Dispatcher.InvokeAsync(() =>
            {
                ReleaseNotesBrowser.Visibility = Visibility.Collapsed;
            });
            // TODO: resize window to account for no release notes being shown
        }

        void IUpdateAvailable.HideRemindMeLaterButton()
        {
            Dispatcher.InvokeAsync(() =>
            {
                RemindMeLaterButton.Visibility = Visibility.Collapsed; // TODO: Binding instead of direct property setting (#70)
            });
        }

        void IUpdateAvailable.HideSkipButton()
        {
            Dispatcher.InvokeAsync(() =>
            {
                SkipButton.Visibility = Visibility.Collapsed; // TODO: Binding instead of direct property setting (#70)
            });
        }

        void IUpdateAvailable.Show(bool IsOnMainThread)
        {
            try
            {
                Show();
                _isOnMainThread = IsOnMainThread;
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

        private void SendResponse(UpdateAvailableResult response)
        {
            _userResponse = response;
            UserResponded?.Invoke(this, new UpdateResponseArgs(_userResponse, CurrentItem));
            _cancellationTokenSource?.Cancel();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            SendResponse(UpdateAvailableResult.SkipUpdate);
        }

        private void RemindMeLaterButton_Click(object sender, RoutedEventArgs e)
        {
            SendResponse(UpdateAvailableResult.RemindMeLater);
        }

        private void DownloadInstallButton_Click(object sender, RoutedEventArgs e)
        {
            SendResponse(UpdateAvailableResult.InstallUpdate);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
