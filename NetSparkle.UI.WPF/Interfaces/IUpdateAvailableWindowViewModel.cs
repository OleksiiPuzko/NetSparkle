using NetSparkle.Enums;

namespace NetSparkle.UI.WPF.Interfaces
{
    public interface IUpdateAvailableWindowViewModel
    {
        UpdateAvailableResult Result { get; }
        AppCastItem CurrentItem { get; }
    }
}
