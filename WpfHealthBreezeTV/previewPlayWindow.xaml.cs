using System;
using System.Text;
using System.Windows;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// previewPlayWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PreviewPlayWindow : Window
    {
        public PreviewPlayWindow(string url, User user, long videoId)
        {
            InitializeComponent();

            string strData = "{\"videos\":\"[" + videoId + "]\"}";
            byte[] postData = Encoding.Default.GetBytes(strData);
            string header = "Authorization: Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}-{1}:{2}", user.uid, user.did, user.sessionKey)));
            header += "\r\nContent-Type: application/json";
            webBrowser.Navigate(url, null, postData, header);
        }
    }
}
