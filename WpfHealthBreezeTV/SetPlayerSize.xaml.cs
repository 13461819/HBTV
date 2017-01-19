using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// SetPlayerSize.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SetPlayerSize : Window
    {
        public SetPlayerSize()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            Width = 960;
            Height = 540;
            Left = (SystemParameters.PrimaryScreenWidth - 960D) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - 540D) / 2;
        }
    }
}
