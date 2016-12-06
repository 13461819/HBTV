using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// DownloadWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DownloadWindow : Window
    {
        private Dictionary<string, Video> videos = new Dictionary<string, Video>();
        private List<string> dnld_paths = new List<string>();
        private int videoCount = 0;
        private int downloadCompleteVideoCount = 0;
        public bool isCancled = false;
        private string path = string.Empty;
        private WebClient webClient;

        public DownloadWindow(Dictionary<string, Video> videos, List<string> dnld_paths)
        {
            InitializeComponent();
            foreach (KeyValuePair<string, Video> v in videos)
            {
                this.videos.Add(v.Key, v.Value);
                addVideoToDownloadView(v.Value);
                videoCount++;
            }
            foreach(string s in dnld_paths)
            {
                this.dnld_paths.Add(s);
            }
            startDownload();
        }

        public void addVideoToDownloadView(Video v)
        {
            Grid grid = new Grid();
            grid.Height = 60;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            grid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions[1].Width = new GridLength(4, GridUnitType.Star);
            
            Image thumbnail = new Image();
            thumbnail.Source = new BitmapImage(new Uri(v.thumbnail));
            thumbnail.Stretch = Stretch.Fill;
            Grid.SetRowSpan(thumbnail, 2);

            Canvas canvas = new Canvas();
            canvas.Margin = new Thickness(10, 0, 0, 0);
            Grid.SetColumn(canvas, 1);

            TextBlock title = new TextBlock();
            title.Text = v.title;
            title.FontSize = 16;

            canvas.Children.Add(title);

            ProgressBar progressBar = new ProgressBar();
            progressBar.Name = "progressBar" + v.code;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            progressBar.Height = 25;
            progressBar.Width = 270;
            progressBar.Foreground = new SolidColorBrush(Color.FromRgb(128, 192, 254));
            Grid.SetColumn(progressBar, 1);
            Grid.SetRow(progressBar, 1);

            TextBlock percent = new TextBlock();
            percent.Name = "percent" + v.code;
            percent.VerticalAlignment = VerticalAlignment.Center;
            percent.HorizontalAlignment = HorizontalAlignment.Center;
            percent.FontSize = 13;
            Grid.SetColumn(percent, 1);
            Grid.SetRow(percent, 1);

            TextBlock size = new TextBlock();
            size.Name = "size" + v.code;
            size.VerticalAlignment = VerticalAlignment.Center;
            size.HorizontalAlignment = HorizontalAlignment.Right;
            size.Margin = new Thickness(0, 0, 15, 0);
            size.FontSize = 13;
            Grid.SetColumn(size, 1);
            Grid.SetRow(size, 1);

            ToolTip toolTip = new ToolTip();
            toolTip.Content = v.title;

            grid.Children.Add(thumbnail);
            grid.Children.Add(canvas);
            grid.Children.Add(progressBar);
            grid.Children.Add(percent);
            grid.Children.Add(size);

            grid.Name = "d" + v.id.ToString();
            grid.ToolTip = toolTip;

            listBoxDownload.RegisterName(progressBar.Name, progressBar);
            listBoxDownload.RegisterName(percent.Name, percent);
            listBoxDownload.RegisterName(size.Name, size);
            listBoxDownload.Items.Add(grid);
        }
        
        private void startDownload()
        {
            path = ApplicationState.filePath;
            /*Video v;
            foreach (KeyValuePair<string, Video> pair in videos)
            {
                v = pair.Value;
                if (!File.Exists(path + v.code + ".wvm"))
                {
                    using (webClient = new WebClient())
                    {
                        Uri from = new Uri("http://storage.googleapis.com/hbreeze4ani.appspot.com/videos/" + v.code.Substring(7) + "/" + v.code.Substring(0, 1) + "/" + v.code + ".wvm");
                        webClient.DownloadProgressChanged += downloadProgressChanged(v.code);
                        webClient.DownloadFileCompleted += downloadDataCompleted(v.code);
                        webClient.DownloadFileAsync(from, path + v.code + ".wvm");
                    }
                }
                else
                {
                    MessageBox.Show(v.title + "\n파일은 이미 다운로드 받으셨습니다.");
                }
            }*/
            string code;
            foreach (string s in dnld_paths)
            {
                code = s.Substring(s.LastIndexOf("/") + 1, s.LastIndexOf(".") - s.LastIndexOf("/") - 1);
                if (!File.Exists(path + code + ".wvm"))
                {
                    using (webClient = new WebClient())
                    {
                        Uri from = new Uri(s);
                        webClient.DownloadProgressChanged += downloadProgressChanged(code);
                        webClient.DownloadFileCompleted += downloadDataCompleted(code);
                        webClient.DownloadFileAsync(from, path + code + ".wvm");
                    }
                }
            }
        }

        private DownloadProgressChangedEventHandler downloadProgressChanged(string code)
        {
            Action<object, DownloadProgressChangedEventArgs> action = (sender, e) =>
            {
                setProgressChange(code, e.ProgressPercentage, (double) e.BytesReceived, (double) e.TotalBytesToReceive);
            };
            return new DownloadProgressChangedEventHandler(action);
        }

        private void setProgressChange(string code, int value, double bytesReceived, double totalBytesToReceive)
        {
            ProgressBar progressBar = ((ProgressBar)listBoxDownload.FindName("progressBar" + code));
            TextBlock percent = ((TextBlock)listBoxDownload.FindName("percent" + code));
            TextBlock size = ((TextBlock)listBoxDownload.FindName("size" + code));
            progressBar.Value = value;
            percent.Text = value.ToString() + "%";
            size.Text = string.Format("{0:f1}", Math.Round((bytesReceived / 1024.0 / 1024.0), 1)) + "MB/" 
                + string.Format("{0:f1}", Math.Round((totalBytesToReceive / 1024.0 / 1024.0), 1)) + "MB";
        }
        
        private System.ComponentModel.AsyncCompletedEventHandler downloadDataCompleted(string code)
        {
            Action<object, System.ComponentModel.AsyncCompletedEventArgs> action = (sender, e) =>
            {
                downloadedComplete(code);
            };
            return new System.ComponentModel.AsyncCompletedEventHandler(action);
        }
        
        private void downloadedComplete(string code)
        {
            ((ProgressBar)listBoxDownload.FindName("progressBar" + code)).Foreground = new SolidColorBrush(Color.FromRgb(171, 214, 121));
            downloadCompleteVideoCount++;
            if (downloadCompleteVideoCount == videoCount && !isCancled)
            {
                buttonCancel.Content = "완료";
                buttonCancel.FontSize = 18;
                buttonCancel.FontWeight = FontWeights.Bold;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (downloadCompleteVideoCount != videoCount)
            {
                isCancled = true;
                if (webClient.IsBusy)
                {
                    webClient.CancelAsync();
                }
            }
            Close();
        }
    }
}
