using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
        public string location = string.Empty;
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            location = ((Button)sender).Name;
            switch (location)
            {
                case "buttonLT":
                    break;
                case "buttonLC":
                    break;
                case "buttonLB":
                    break;
                case "buttonCT":
                    break;
                case "buttonCC":
                    break;
                case "buttonCB":
                    break;
                case "buttonRT":
                    break;
                case "buttonRC":
                    break;
                case "buttonRB":
                    break;
                default:
                    break;
            }
            Close();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
