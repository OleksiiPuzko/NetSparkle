using NetSparkle.Interfaces;
using NetSparkle.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetSparkle.UI.NetFramework.WPF
{
    /// <summary>
    /// UI factory for default interface
    /// </summary>
    public class UIFactory : IUIFactory
    {
        private string separatorTemplate = "<div style=\"border: #ccc 1px solid;\">" +
            "<div style=\"background: {3}; padding: 5px; color: {4}; font-family: Helvetica, Arial, sans-serif;\">" +
            "<span style=\"float: right; display:float;\">" +
                    "{1}</span>{0}</div><div style=\"padding: 5px; color: white; \">{2}</div></div><br>";

        private string htmlHeadAddition = "<style>body { background-color: #24292F;  overflow: auto;  scrollbar-base-color: #F2F2F2; }</style>";

        /// <summary>
        /// Create sparkle form implementation
        /// </summary>
        /// <param name="sparkle">The <see cref="Sparkle"/> instance to use</param>
        /// <param name="updates">Sorted array of updates from latest to earliest</param>
        /// <param name="applicationIcon">The icon to display</param>
        /// <param name="isUpdateAlreadyDownloaded">If true, make sure UI text shows that the user is about to install the file instead of download it.</param>
        public virtual IUpdateAvailable CreateSparkleForm(Sparkle sparkle, List<AppCastItem> updates, Icon applicationIcon, bool isUpdateAlreadyDownloaded = false)
        {
            var window = new UpdateAvailableWindow()
            {
                Icon = ToImageSource(applicationIcon)
            };
            window.Initialize(sparkle, updates, isUpdateAlreadyDownloaded, separatorTemplate, htmlHeadAddition);
            return window;
        }

        /// <summary>
        /// Create download progress window
        /// </summary>
        /// <param name="item">Appcast item to download</param>
        /// <param name="applicationIcon">Application icon to use</param>
        public virtual IDownloadProgress CreateProgressWindow(AppCastItem item, Icon applicationIcon)
        {
            return new DownloadProgressWindow
            {
                ItemToDownload = item,
                Icon = ToImageSource(applicationIcon)
            };
        }

        /// <summary>
        /// Convert System.Drawing.Icon to System.Windows.Media.ImageSource.
        ///  From: https://stackoverflow.com/a/6580799/3938401
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        private static ImageSource ToImageSource(Icon icon)
        {
            if (icon == null)
            {
                return null;
            }

            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        /// <summary>
        /// Inform user in some way that NetSparkle is checking for updates
        /// </summary>
        /// <param name="applicationIcon">The icon to display</param>
        public virtual ICheckingForUpdates ShowCheckingForUpdates(Icon applicationIcon = null)
        {
            return new CheckingForUpdatesWindow { Icon = ToImageSource(applicationIcon) };
        }

        /// <summary>
        /// Initialize UI. Called when Sparkle is constructed and/or when the UIFactory is set.
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// Show user a message saying downloaded update format is unknown
        /// </summary>
        /// <param name="downloadFileName">The filename to be inserted into the message text</param>
        /// <param name="applicationIcon">The icon to display</param>
        public virtual void ShowUnknownInstallerFormatMessage(string downloadFileName, Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle,
                string.Format(Resources.DefaultUIFactory_ShowUnknownInstallerFormatMessageText, downloadFileName), applicationIcon);
        }

        /// <summary>
        /// Show user that current installed version is up-to-date
        /// </summary>
        public virtual void ShowVersionIsUpToDate(Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, Resources.DefaultUIFactory_ShowVersionIsUpToDateMessage, applicationIcon);
        }

        /// <summary>
        /// Show message that latest update was skipped by user
        /// </summary>
        public virtual void ShowVersionIsSkippedByUserRequest(Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultUIFactory_MessageTitle, Resources.DefaultUIFactory_ShowVersionIsSkippedByUserRequestMessage, applicationIcon);
        }

        /// <summary>
        /// Show message that appcast is not available
        /// </summary>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        /// <param name="applicationIcon">The icon to display</param>
        public virtual void ShowCannotDownloadAppcast(string appcastUrl, Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultUIFactory_ErrorTitle, Resources.DefaultUIFactory_ShowCannotDownloadAppcastMessage, applicationIcon);
        }

        /// <summary>
        /// Show 'toast' window to notify new version is available
        /// </summary>
        /// <param name="updates">Appcast updates</param>
        /// <param name="applicationIcon">Icon to use in window</param>
        /// <param name="clickHandler">handler for click</param>
        public virtual void ShowToast(List<AppCastItem> updates, Icon applicationIcon, Action<List<AppCastItem>> clickHandler)
        {
            Thread thread = new Thread(() =>
            {
                var toast = new ToastNotification()
                {
                    ClickAction = clickHandler,
                    Updates = updates,
                    Icon = ToImageSource(applicationIcon)
                };
                try
                {
                    toast.Show(Resources.DefaultUIFactory_ToastMessage, Resources.DefaultUIFactory_ToastCallToAction, 5);
                    System.Windows.Threading.Dispatcher.Run();
                }
                catch (ThreadAbortException)
                {
                    toast.Dispatcher.InvokeShutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Show message on download error
        /// </summary>
        /// <param name="message">Error message from exception</param>
        /// <param name="appcastUrl">the URL for the appcast file</param>
        /// <param name="applicationIcon">The icon to display</param>
        public virtual void ShowDownloadErrorMessage(string message, string appcastUrl, Icon applicationIcon = null)
        {
            ShowMessage(Resources.DefaultUIFactory_ErrorTitle, string.Format(Resources.DefaultUIFactory_ShowDownloadErrorMessage, message), applicationIcon);
        }

        private void ShowMessage(string title, string message, Icon applicationIcon = null)
        {
            var messageWindow = new MessageNotificationWindow
            {
                Title = title,
                MessageToShow = message,
                Icon = ToImageSource(applicationIcon)
            };
            messageWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            messageWindow.ShowDialog();
        }

        /// <summary>
        /// Shut down the UI so we can run an upate.
        /// If in WPF, System.Windows.Application.Current.Shutdown().
        /// If in WinForms, Application.Exit().
        /// </summary>
        public void Shutdown()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
