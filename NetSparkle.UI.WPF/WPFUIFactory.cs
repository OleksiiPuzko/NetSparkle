using NetSparkle.Interfaces;
using NetSparkle.UI.WPF.View;
using NetSparkle.UI.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetSparkle.UI.WPF
{
    public class WPFUIFactory : IUIFactory
    {
        public IDownloadProgress CreateProgressWindow(AppCastItem item, Icon applicationIcon)
        {
            throw new NotImplementedException();
        }

        public IUpdateAvailable CreateSparkleForm(Sparkle sparkle, List<AppCastItem> updates, Icon applicationIcon, bool isUpdateAlreadyDownloaded = false)
        {
            var viewModel = new UpdateAvailableWindowViewModel(sparkle, updates, isUpdateAlreadyDownloaded);
            var view = new UpdateAvailableWindowView(viewModel)
            {
                Icon = ToImageSource(applicationIcon)
            };

            return view;
        }

        public void Init()
        {

        }

        public void ShowCannotDownloadAppcast(string appcastUrl, Icon applicationIcon = null)
        {
            throw new NotImplementedException();
        }

        public ICheckingForUpdates ShowCheckingForUpdates(Icon applicationIcon = null)
        {
            throw new NotImplementedException();
        }

        public void ShowDownloadErrorMessage(string message, string appcastUrl, Icon applicationIcon = null)
        {
            throw new NotImplementedException();
        }

        public void ShowToast(List<AppCastItem> updates, Icon applicationIcon, Action<List<AppCastItem>> clickHandler)
        {
            throw new NotImplementedException();
        }

        public void ShowUnknownInstallerFormatMessage(string downloadFileName, Icon applicationIcon = null)
        {
            throw new NotImplementedException();
        }

        public void ShowVersionIsSkippedByUserRequest(Icon applicationIcon = null)
        {
            throw new NotImplementedException();
        }

        public void ShowVersionIsUpToDate(Icon applicationIcon = null)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

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
    }
}
