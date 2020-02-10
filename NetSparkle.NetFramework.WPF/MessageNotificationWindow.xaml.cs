using System.Windows;

namespace NetSparkle.UI.NetFramework.WPF
{
    /// <summary>
    /// Interaction logic for MessageNotificationWindow.xaml
    /// </summary>
    public partial class MessageNotificationWindow : Window
    {
        public MessageNotificationWindow()
        {
            InitializeComponent();
            Message.Text = "";
        }

        public string MessageToShow
        {
            set { Message.Text = value; }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
