using System.Windows;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// AppInfoWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AppInfoWindow : Window
    {
        public AppInfoWindow(string appVersion)
        {
            InitializeComponent();
            textBlockAppVersion.Text = appVersion;
        }
    }
}
