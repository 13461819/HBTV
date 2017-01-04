using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Linq;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Windows.Media;
using Microsoft.Win32;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        delegate void Work();
        private BackgroundWorker workerInit;
        private BackgroundWorker workerChannel;
        private BackgroundWorker workerSearch;

        private string URL = string.Empty;
        private string URL2 = string.Empty;
        private Dictionary<string, Video> videos = new Dictionary<string, Video>();
        static HttpClient client = new HttpClient();
        private string filePath = ApplicationState.filePath;
        User user = null;
        string email = string.Empty;
        List<Channel> channels = new List<Channel>();
        List<string> myChannel = new List<string>();
        List<string> searchResult = new List<string>();
        
        public MainWindow()
        {
            InitializeComponent();
            Mouse.OverrideCursor = Cursors.Wait;
            email = ApplicationState.GetValue<string>("email");
            getChannelFromReg();
            //new Work(backgroundWork).BeginInvoke(null, null);
            textBlockNewUser.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Work(initData));
            //initExecute();
            //Mouse.OverrideCursor = null;
        }
        
        private void initExecute()
        {
            workerInit = new BackgroundWorker();
            workerInit.DoWork += new DoWorkEventHandler(initData);
            workerInit.RunWorkerAsync();
        }

        private void initData(object sender, DoWorkEventArgs e)
        {
            initData();
        }

        private void backgroundWork()
        {
            if (ApplicationState.GetValue<bool>("isDataStored") == false)
            {
                textBlockNewUser.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Work(backgroundWork));
            }
            textBlockNewUser.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Work(initData));
        }

        private void initData()
        {
            
            if (ApplicationState.GetValue<bool>("isDataStored") == false)
            {
                textBlockNewUser.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Work(initData));
            }
            
            user = ApplicationState.GetValue<User>("currentUser");

            switch (ApplicationState.runAt)
            {
                case "server":
                    URL = ApplicationState.serverUrl;
                    URL2 = ApplicationState.serverUrl;
                    break;
                case "test":
                    URL = ApplicationState.testUrl;
                    URL2 = ApplicationState.testUrl;
                    break;
                case "local":
                    URL = ApplicationState.localUrl1;
                    URL2 = ApplicationState.localUrl2;
                    break;
                default:
                    break;
            }

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            if (user.authorization != 10)
            {
                buttonNewUser.Visibility = Visibility.Hidden;
                textBlockNewUser.Visibility = Visibility.Hidden;
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL2);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}-{1}:{2}", user.uid, user.did, user.sessionKey))));

            // cache-control 과정
            RegistryKey regConfig = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\config", true);
            if (File.Exists(ApplicationState.filePath + "videos.dat"))
            {
                if (regConfig.GetValue("last-modified") != null)
                {
                    client.DefaultRequestHeaders.Add("If-Modified-Since", regConfig.GetValue("last-modified") as string);
                }
            }

            try
            {
                HttpResponseMessage response = client.GetAsync("tvapp/videos").Result;
                string jsonStringVideos = string.Empty;
                
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jsonStringVideos = response.Content.ReadAsStringAsync().Result;

                    // 파일로 저장 후 레지스트리에 last-modified값 저장
                    File.WriteAllText(ApplicationState.filePath + "videos.dat", jsonStringVideos);
                    regConfig.SetValue("last-modified", ((DateTimeOffset)response.Content.Headers.LastModified).ToString("r"));

                }
                else if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    jsonStringVideos = File.ReadAllText(ApplicationState.filePath + "videos.dat");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    MessageBox.Show("비디오 목록을 받아오지 못하였습니다.\n프로그램을 다시 실행시켜주세요.");
                    Close();
                }

                List<Video> tempVideos = JsonConvert.DeserializeObject<List<Video>>(jsonStringVideos);
                tempVideos = tempVideos.OrderBy(v => v.code.Substring(0, 1)).ThenBy(v => v.title).ToList();
                foreach (Video v in tempVideos)
                {
                    videos.Add(v.id.ToString(), v);
                    searchResult.Add(v.id.ToString());
                }
                ApplicationState.SetValue("videos", videos);
                refreshMyChannel();
                refreshSearchResult();
            }
            catch (HttpRequestException he)
            {
                MessageBox.Show("비디오 목록을 받아오지 못하였습니다.\n" + he.Message);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n네트워크 연결을 확인해주세요.(02)");
                Close();
            }
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxSearch.Text == string.Empty)
            {
                MessageBox.Show("검색어를 입력하세요\n전체 비디오를 보시려면 * 을 검색하세요.", "검색");
                return;
            }

            searchResult.Clear();
            listBoxSearch.Items.Clear();

            if (textBoxSearch.Text == "*")
            {
                foreach (KeyValuePair<string, Video> v in videos)
                {
                    searchResult.Add(v.Value.id.ToString());
                    //addVideoToListView(v.Value, listViewSearch);
                }
            }
            else
            {
                foreach (KeyValuePair<string, Video> v in videos)
                {
                    if (v.Value.title.IndexOf(textBoxSearch.Text) > -1)
                    {
                        searchResult.Add(v.Value.id.ToString());
                        //addVideoToListView(v.Value, listViewSearch);
                    }
                }
            }
            refreshSearchResult();
            textBoxSearch.Text = string.Empty;
        }
       
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                string grid = ((Grid)sender).Name.Substring(0, 1);
                string videoId = ((Grid)sender).Name.Substring(1);
                int index = myChannel.IndexOf(videoId);
                if (grid == "s")
                {
                    if (index >= 0)
                    {
                        myChannel.RemoveAt(index);
                    }
                    else
                    {
                        myChannel.Add(videoId);
                    }
                }
                else
                {
                    myChannel.RemoveAt(index);
                }
                refreshMyChannel();
                refreshSearchResult();
                updateChannelReg();
                buttonAddChannel.Content = "추가";
            }
        }

        private void textBoxSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonSearch_Click(sender, e);
            }
        }
        public string converPlaytime(string playtime)
        {
            return playtime + "분";
        }
                
        private void addVideoToListView(Video v, ListBox lb)
        {
            Grid grid = new Grid();
            grid.Height = 72;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            grid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions[0].Width = new GridLength(96);
            grid.ColumnDefinitions[1].Width = new GridLength(30, GridUnitType.Star);
            grid.ColumnDefinitions[2].Width = new GridLength(4, GridUnitType.Star);
            grid.ColumnDefinitions[3].Width = new GridLength(4, GridUnitType.Star);

            Border border = new Border();
            border.BorderThickness = new Thickness(0, 0, 0, 1);
            border.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
            Grid.SetRow(border, 1);
            Grid.SetColumnSpan(border, 4);

            ContextMenu contextMenu = new ContextMenu();

            MenuItem menuItemAdd = new MenuItem();
            menuItemAdd.Header = "채널에 추가";
            menuItemAdd.Click += MenuItemAdd_Click;

            MenuItem menuItemRemove = new MenuItem();
            menuItemRemove.Header = "채널에서 삭제";
            menuItemRemove.Click += MenuItemRemove_Click;

            Separator separator = new Separator();

            if (lb.Name == "listBoxSearch")
            {
                contextMenu.Items.Add(menuItemAdd);
            }
            if (lb.Name == "listBoxChannel")
            {
                contextMenu.Items.Add(menuItemRemove);
            }
            //contextMenu.Items.Add(separator);
            //contextMenu.Items.Add(menuItemDownload);

            System.Windows.Controls.Image thumbnail = new System.Windows.Controls.Image();
            thumbnail.Source = new BitmapImage(new Uri(v.thumbnail));
            thumbnail.Stretch = System.Windows.Media.Stretch.Fill;
            Grid.SetRowSpan(thumbnail, 2);

            Canvas canvas = new Canvas();
            Grid.SetColumn(canvas, 1);
            canvas.HorizontalAlignment = HorizontalAlignment.Stretch;

            TextBlock title = new TextBlock();
            title.Text = v.title;
            //title.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(title, 1);
            Grid.SetRowSpan(title, 2);
            title.FontSize = 16;
            title.Margin = new Thickness(10, 0, 0, 0);
            title.TextTrimming = TextTrimming.CharacterEllipsis;
            title.TextWrapping = TextWrapping.Wrap;

            //canvas.Children.Add(title);

            TextBlock playtime = new TextBlock();
            playtime.Text = convertPlayTime(v.playtime);
            playtime.FontSize = 14;
            //playtime.Margin = new Thickness(10, 0, 0, 0);
            playtime.VerticalAlignment = VerticalAlignment.Center;
            playtime.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetColumn(playtime, 2);
            Grid.SetColumnSpan(playtime, 2);

            ToolTip toolTip = new ToolTip();
            toolTip.Content = v.title;

            grid.Children.Add(thumbnail);
            //grid.Children.Add(canvas);
            grid.Children.Add(title);
            grid.Children.Add(playtime);
            grid.Children.Add(border);
            grid.ContextMenu = contextMenu;

            grid.Name = (lb.Name == "listBoxSearch" ? "s" : "c") + v.id.ToString();
            //grid.MouseLeftButtonDown += Grid_MouseLeftButtonDown;
            grid.ToolTip = toolTip;
            lb.Items.Add(grid);
            if (lb.FindName(grid.Name) == null)
            {
                lb.RegisterName(grid.Name, grid);
            }
            //grid.MouseEnter += Grid_MouseEnter;
            //grid.MouseLeave += Grid_MouseLeave;

            if (lb.Name == "listBoxSearch")
            {
                /*
                CheckBox checkBox = new CheckBox();
                checkBox.Name = "check" + v.id.ToString();
                checkBox.HorizontalAlignment = HorizontalAlignment.Center;
                checkBox.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRowSpan(checkBox, 2);
                Grid.SetColumnSpan(checkBox, 2);
                Grid.SetColumn(checkBox, 2);

                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = 2;
                scaleTransform.ScaleY = 2;
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(scaleTransform);
                checkBox.RenderTransform = transformGroup;

                grid.Children.Add(checkBox);
                */
                if (isVideoChanneled(v.id.ToString()))
                {
                    //grid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xCC, 0xF1, 0xFF));
                    System.Windows.Controls.Image channeled = new System.Windows.Controls.Image();
                    channeled.Source = new BitmapImage(new Uri("img/channel.png", UriKind.Relative));
                    Grid.SetRowSpan(channeled, 2);
                    channeled.Width = 18;
                    channeled.Margin = new Thickness(0, 0, 4, 4);
                    channeled.HorizontalAlignment = HorizontalAlignment.Right;
                    channeled.VerticalAlignment = VerticalAlignment.Bottom;
                    grid.Children.Add(channeled);
                }
            }
            else
            {
                if (!isVideoDownloaded(v.code))
                {
                    System.Windows.Controls.Image saved = new System.Windows.Controls.Image();
                    saved.Source = new BitmapImage(new Uri("img/Icon-download.png", UriKind.Relative));
                    saved.Width = 28;
                    saved.HorizontalAlignment = HorizontalAlignment.Right;
                    saved.Cursor = Cursors.Help;
                    saved.ToolTip = "파일이 없습니다.";
                    Grid.SetColumn(saved, 2);
                    Grid.SetRow(saved, 1);
                    grid.Children.Add(saved);
                }

                if (!isPlayable(v.id))
                {
                    System.Windows.Controls.Image play = new System.Windows.Controls.Image();
                    play.Source = new BitmapImage(new Uri("img/Icon-Closed.png", UriKind.Relative));
                    play.Width = 28;
                    play.Cursor = Cursors.Help;
                    play.ToolTip = "컨텐츠의 재생권한이 필요합니다.";
                    Grid.SetColumn(play, 3);
                    Grid.SetRow(play, 1);
                    grid.Children.Add(play);
                }
            }
        }

        private bool isVideoChanneled(string id)
        {
            bool result = false;
            
            if (myChannel.Contains(id))
            {
                result = true;
            }

            return result;
        }

        private bool isVideoDownloaded(string code)
        {
            bool result = false;
            if (File.Exists(filePath + code + ".wvm"))
            {
                result = true;
            }
            return result;
        }

        private bool isPlayable(long id)
        {
            bool result = false;
            foreach (long downloadedId in user.downloads)
            {
                if (id == downloadedId)
                {
                    result = true;
                }
            }
            return result;
        }
        
        private void MenuItemRemove_Click(object sender, RoutedEventArgs e)
        {
            string caption = "비디오 삭제";
            string message = "선택된 비디오를 채널에서 삭제하시겠습니까?";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(message, caption, button, icon);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    var selected = listBoxChannel.SelectedItems.Cast<Object>().ToArray();
                    foreach (var item in selected)
                    {
                        myChannel.Remove(((Grid)item).Name.Substring(1));
                        //listViewChannel.Items.Remove(item);
                    }
                    break;
                default:
                    break;
            }
            refreshMyChannel();
            refreshSearchResult();
            //updateChannelFile();
            updateChannelReg();
        }

        private void MenuItemAdd_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxSearch.SelectedItems.Count == 0)
            {
                MessageBox.Show("선택 된 컨텐츠가 없습니다.");
                return;
            }
            string caption = "비디오 추가";
            string message = "선택된 비디오를 채널에 추가하시겠습니까?";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(message, caption, button, icon);
            string channelExistVideo = string.Empty;

            switch (result)
            {
                case MessageBoxResult.Yes:
                    var selected = listBoxSearch.SelectedItems.Cast<Object>().ToArray();
                    foreach (var item in selected)
                    {
                        if (myChannel.Contains(((Grid)item).Name.Substring(1)))
                        {
                            channelExistVideo += videos[((Grid)item).Name.Substring(1)].title + "\n";
                            continue;
                        }
                        myChannel.Add(((Grid)item).Name.Substring(1));
                        buttonAddChannel.Content = "추가";
                        //addVideoToListView(videos[((Grid)item).Name.Substring(1)], listViewChannel);
                    }
                    break;
                default:
                    break;
            }

            if (channelExistVideo != string.Empty)
            {
                MessageBox.Show(channelExistVideo + "\n해당 비디오는 이미 채널에 등록되어있습니다.");
            }
            refreshMyChannel();
            refreshSearchResult();
            //updateChannelFile();
            updateChannelReg();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            textBoxSearch.Text = "Grid_MouseLeave";
            Grid grid = (Grid)sender;
            Canvas canvas = (Canvas)grid.Children[1];
            TextBlock textBlock = (TextBlock)canvas.Children[0];
            
            textBlock.BeginAnimation(Canvas.LeftProperty, null);
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            textBoxSearch.Text = "Grid_MouseEnter";
            Grid grid = (Grid)sender;
            Canvas canvas = (Canvas)grid.Children[1];
            TextBlock textBlock = (TextBlock)canvas.Children[0];
            Panel.SetZIndex(textBlock, -1);

            textBlock.BeginAnimation(Canvas.LeftProperty, null);

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = grid.ActualWidth;
            doubleAnimation.To = -textBlock.ActualWidth;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(10));
            textBlock.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
        }

        private string convertPlayTime(int playtime)
        {
            TimeSpan t = TimeSpan.FromSeconds(playtime);

            string result = string.Format("({0:D2}:{1:D2})",
                            t.Minutes,
                            t.Seconds);
            return result;
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxChannel.Items.Count == 0)
            {
                MessageBox.Show("채널에 등록된 비디오가 없습니다.");
                return;
            }

            List<string> selectedVideosList = new List<string>(); 
            string selectedVideosStr = string.Empty; 
            Dictionary<string, Video> notExistVideos = new Dictionary<string, Video>(); //다운로드 받지 않은 비디오
            Dictionary<string, Video> notAuthVideos = new Dictionary<string, Video>(); //재생 권한이 없는 비디오
            int charge = 0; //재생 가격

            foreach (var item in listBoxChannel.Items)
            {
                Video video = videos[((Grid)item).Name.Substring(1)];
                if (!isVideoDownloaded(video.code))
                {
                    notExistVideos.Add(video.id.ToString(), video);
                }
                if (!isPlayable(video.id))
                {
                    notAuthVideos.Add(video.id.ToString(), video);
                }
                selectedVideosList.Add(((Grid)item).Name.Substring(1));
            }

            if (notExistVideos.Count != 0) //비디오부터 없음
            {
                string caption = "컨텐츠 없음";
                string notExistVideo = string.Empty;
                foreach (KeyValuePair<string, Video> v in notExistVideos)
                {
                    notExistVideo += v.Value.title + "\n";
                }
                string message = notExistVideo + "\n이 컨텐츠를 다운로드 해야 시청하실 수 있습니다.\n모두 다운로드 하시겠습니까?";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result = MessageBox.Show(message, caption, button, icon);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        List<string> temp = new List<string>();
                        foreach (Video v in notExistVideos.Values)
                        {
                            temp.Add(v.id.ToString());
                        }
                        string data = "{\"action\":\"get\",\"contents\":\"[" + string.Join(",", temp) + "]\"}";
                        var content = new StringContent(data, Encoding.UTF8, "application/json");
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri(URL2);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue(
                                "Basic",
                                Convert.ToBase64String(
                                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                                        string.Format("{0}-{1}:{2}", user.uid, user.did, user.sessionKey))));
                        try
                        {
                            HttpResponseMessage response = client.PostAsync("api/v1/webdrm/downloads", content).Result;
                            //response.EnsureSuccessStatusCode();
                            string responseText = response.Content.ReadAsStringAsync().Result;
                            dynamic jo = JObject.Parse(responseText);

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                //charge = jo.charge;
                                List<string> dnld_paths = new List<string>();
                                foreach (var dnld_path in jo.dnld_paths)
                                {
                                    dnld_paths.Add(dnld_path.ToString());
                                }
                                DownloadWindow downloadWindow = new DownloadWindow(notExistVideos, dnld_paths);
                                downloadWindow.ShowDialog();
                                if (downloadWindow.isCancled)
                                {
                                    MessageBox.Show("다운로드를 취소하고 다운로드 중이던 컨텐츠를 삭제합니다.");
                                    Video v;
                                    foreach (KeyValuePair<string, Video> pair in notExistVideos)
                                    {
                                        v = pair.Value;
                                        if (File.Exists(filePath + v.code + ".wvm"))
                                        {
                                            File.Delete(filePath + v.code + ".wvm");
                                        }
                                    }
                                    //refreshSearchResult();
                                    refreshMyChannel();
                                    return;
                                }
                                //refreshSearchResult();
                                refreshMyChannel();
                            }
                            else if (response.StatusCode != HttpStatusCode.BadRequest)
                            {
                                MessageBox.Show("실패\n" + jo.code + ": " + jo.message);
                            }
                        }
                        catch (HttpRequestException he)
                        {
                            MessageBox.Show("api/v1/webdrm/downloads 실패\n" + he.Message, "실패");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "\n네트워크 연결을 확인해주세요.(03)");
                        }
                        break;
                    default:
                        return;
                }
            }
            
            if (notAuthVideos.Count != 0) //재생권한이 없음
            {
                List<string> temp = new List<string>();
                foreach (Video v in notAuthVideos.Values)
                {
                    temp.Add(v.id.ToString());
                }
                string data = "{\"action\":\"get\",\"contents\":\"[" + string.Join(",", temp) + "]\"}";
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(URL2);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}-{1}:{2}", user.uid, user.did, user.sessionKey))));
                try
                {
                    HttpResponseMessage response = client.PostAsync("api/v1/webdrm/downloads", content).Result;
                    //response.EnsureSuccessStatusCode();
                    string responseText = response.Content.ReadAsStringAsync().Result;
                    dynamic jo = JObject.Parse(responseText);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        charge = jo.charge;
                        List<string> dnld_paths = new List<string>();
                        foreach (var dnld_path in jo.dnld_paths)
                        {
                            dnld_paths.Add(dnld_path.ToString());
                        }
                        string caption = "재생 권한 없음";
                        string message = "컨텐츠를 바로 시청하시겠습니까?\n" + charge.ToString() + "티켓이 사용됩니다.";
                        MessageBoxButton button = MessageBoxButton.YesNo;
                        MessageBoxImage icon = MessageBoxImage.Warning;
                        MessageBoxResult result = MessageBox.Show(message, caption, button, icon);

                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                /*List<string>*/ temp = new List<string>();
                                foreach (Video v in notAuthVideos.Values)
                                {
                                    temp.Add(v.id.ToString());
                                }
                                /*string*/ data = "{\"action\":\"post\",\"contents\":\"[" + string.Join(",", temp) + "]\"}";
                                /*var*/ content = new StringContent(data, Encoding.UTF8, "application/json");
                                client = new HttpClient();
                                client.BaseAddress = new Uri(URL2);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(
                                    new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization =
                                    new AuthenticationHeaderValue(
                                        "Basic",
                                        Convert.ToBase64String(
                                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                                string.Format("{0}-{1}:{2}", user.uid, user.did, user.sessionKey))));
                                /*HttpResponseMessage*/ response = client.PostAsync("api/v1/webdrm/downloads", content).Result;
                                try
                                {
                            //response.EnsureSuccessStatusCode();
                                    /*string*/ responseText = response.Content.ReadAsStringAsync().Result;
                                    /*dynamic*/ jo = JObject.Parse(responseText);
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        foreach (Video v in notAuthVideos.Values)
                                        {
                                            user.downloads.Add(v.id);
                                        }
                                        //refreshSearchResult();
                                        refreshMyChannel();
                                        selectedVideosStr = "%5B" + string.Join(",", selectedVideosList) + "%5D"; // selectedVideosStr = [id,id]
                                        ProcessStartInfo startInfo = new ProcessStartInfo("IEXPLORE.EXE");
                                        startInfo.WindowStyle = ProcessWindowStyle.Normal;
                                        startInfo.Arguments = URL2 + "tvapp/" + user.uid.ToString() + "/" + user.did.ToString() + "/play?videos=" + selectedVideosStr + "&localPath=/" + Uri.EscapeDataString(filePath.Replace("\\", "/").Substring(3, filePath.Length - 4));
                                        Process.Start(startInfo);
                                    }
                                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                                    {
                                        switch ((string)jo.code)
                                        {
                                            case "2001":
                                                MessageBox.Show("티켓이 부족합니다.");
                                                break;
                                            default:
                                                MessageBox.Show("실패\n" + jo.code + ": " + jo.message);
                                                break;
                                        }
                                    }
                                }
                                catch (HttpRequestException he)
                                {
                                    MessageBox.Show("api/v1/webdrm/downloads 실패\n" + he.Message, "실패");
                                }
                                break;
                            default:
                                return;
                        }
                    }
                    else if (response.StatusCode != HttpStatusCode.BadRequest)
                    {
                        MessageBox.Show("실패\n" + jo.code + ": " + jo.message);
                    }
                }
                catch (HttpRequestException he)
                {
                    MessageBox.Show("api/v1/webdrm/downloads 실패\n" + he.Message, "실패");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n네트워크 연결을 확인해주세요.(04)");
                }
            }
            else //비디오도 있고 재생권한도 있음
            {
                selectedVideosStr = "%5B" + string.Join(",", selectedVideosList) + "%5D"; // selectedVideosStr = [id,id]
                ProcessStartInfo startInfo = new ProcessStartInfo("IEXPLORE.EXE");
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.Arguments = URL2 + "tvapp/" + user.uid.ToString() + "/" + user.did.ToString() + "/play?videos=" + selectedVideosStr + "&localPath=/" + Uri.EscapeDataString(filePath.Replace("\\", "/").Substring(3, filePath.Length - 4));
                Process.Start(startInfo);
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxChannel.Items.Count == 0)
            {
                MessageBox.Show("채널에 등록된 비디오가 없습니다.", "비디오 삭제");
                return;
            }
            
            if (listBoxChannel.SelectedItems.Count == 0)
            {
                MessageBox.Show("선택된 비디오가 없습니다.\n비디오를 선택해주세요.", "비디오 삭제");
                return;
            }

            string caption = "비디오 삭제";
            string message = "정말로 채널을 비우시겠습니까?";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(message, caption, button, icon);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    
                    var selected = listBoxChannel.SelectedItems.Cast<Object>().ToArray();
                    foreach (var item in selected)
                    {
                        myChannel.Remove(((Grid)item).Name.Substring(1));
                        changeChanneledThumb(((Grid)item).Name.Substring(1), false);
                        //listViewChannel.Items.Remove(item);
                    }
                    //myChannel.Clear();
                    buttonDelete.Content = "삭제";
                    break;
                default:
                    break;
            }
            //refreshSearchResult();
            refreshMyChannel();
            //updateChannelFile();
            updateChannelReg();
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshMyChannel();
            refreshSearchResult();
            MessageBox.Show("새로고침 완료");
        }
        
        private void updateChannelReg()
        {
            RegistryKey regUser = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\" + email, true);
            if (myChannel.Count != 0)
            {
                regUser.SetValue("channel", string.Join(",", myChannel));
            }
            else
            {
                if (regUser.GetValue("channel") != null)
                {
                    regUser.DeleteValue("channel");
                }
            }
        }
        
        private void getChannelFromReg()
        {
            RegistryKey regUser = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\" + email, true);

            if (regUser == null)
            {
                RegistryKey regHealthBreeze = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze", true);
                if (regHealthBreeze == null)
                {
                    RegistryKey regSoftware = Registry.CurrentUser.OpenSubKey("Software", true);
                    regHealthBreeze = regSoftware.CreateSubKey("HealthBreeze");
                }
                regUser = regHealthBreeze.CreateSubKey(email);
            }

            if (regUser.GetValue("channel") != null)
            {
                tabControlMain.SelectedIndex = 1;
                string channeledVideos = (string)(regUser.GetValue("channel"));
                foreach (string videoID in channeledVideos.Split(','))
                {
                    myChannel.Add(videoID);
                }
            }
        }

        private void refreshMyChannel()
        {
            listBoxChannel.Items.Clear();

            //new Work(setChannel).BeginInvoke(null, null);

            
            Thread myChannelThread = new Thread(new ThreadStart(channelThread));
            myChannelThread.SetApartmentState(ApartmentState.STA);
            myChannelThread.IsBackground = true;
            myChannelThread.Start();
            

            /*
            Thread thread = new Thread(() => channelThread());
            thread.IsBackground = true;
            
            thread.Start();
            */
        }

        private void setChannel()
        {
            listBoxChannel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Work(displayChannel));
        }

        private void displayChannel()
        {
            foreach (string id in myChannel)
            {
                addVideoToListView(videos[id], listBoxChannel);
            }
        }

        void channelThread()
        {
            listBoxChannel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                foreach (string id in myChannel)
                {
                    addVideoToListView(videos[id], listBoxChannel);
                }
            }));
        }

        private void refreshSearchResult()
        {
            listBoxSearch.Items.Clear();

            //new Work(setSearch).BeginInvoke(null, null);

            
            Thread searchResultThread = new Thread(new ThreadStart(searchThread));
            searchResultThread.SetApartmentState(ApartmentState.STA);
            searchResultThread.IsBackground = true;
            searchResultThread.Start();
            

            /*
            Thread thread = new Thread(() => searchThread());
            thread.IsBackground = true;
            
            thread.Start();
            */
        }

        private void setSearch()
        {
            listBoxChannel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Work(displaySearch));
        }

        private void displaySearch()
        {
            foreach (string id in searchResult)
            {
                addVideoToListView(videos[id], listBoxSearch);
            }
        }

        void searchThread()
        {
            listBoxChannel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                foreach (string id in searchResult)
                {
                    addVideoToListView(videos[id], listBoxSearch);
                }
            }));
        }

        private void changeChanneledThumb(string changedVideo, bool isAdd)
        {
            Grid grid = (Grid)(listBoxSearch.FindName("s" + changedVideo));
            if (grid != null)
            {
                if (isAdd)
                {
                    System.Windows.Controls.Image channeled = new System.Windows.Controls.Image();
                    channeled.Source = new BitmapImage(new Uri("img/channel.png", UriKind.Relative));
                    Grid.SetRowSpan(channeled, 2);
                    channeled.Width = 18;
                    channeled.Margin = new Thickness(0, 0, 4, 4);
                    channeled.HorizontalAlignment = HorizontalAlignment.Right;
                    channeled.VerticalAlignment = VerticalAlignment.Bottom;
                    grid.Children.Add(channeled);
                }
                else
                {
                    grid.Children.RemoveAt(4);
                }
            }
        }

        private void textBlockNewUser_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow(false);
            registerWindow.ShowDialog();
        }

        private void buttonAddChannel_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxSearch.SelectedItems.Count == 0)
            {
                MessageBox.Show("선택 된 컨텐츠가 없습니다.");
                return;
            }
            string caption = "비디오 추가";
            string message = "선택된 비디오를 채널에 추가하시겠습니까?";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(message, caption, button, icon);
            string channelExistVideo = string.Empty;

            switch (result)
            {
                case MessageBoxResult.Yes:
                    var selected = listBoxSearch.SelectedItems.Cast<Object>().ToArray();
                    foreach (var item in selected)
                    {
                        if (myChannel.Contains(((Grid)item).Name.Substring(1)))
                        {
                            channelExistVideo += videos[((Grid)item).Name.Substring(1)].title + "\n";
                            continue;
                        }
                        myChannel.Add(((Grid)item).Name.Substring(1));
                        changeChanneledThumb(((Grid)item).Name.Substring(1), true);
                        //addVideoToListView(videos[((Grid)item).Name.Substring(1)], listViewChannel);
                    }
                    buttonAddChannel.Content = "추가";
                    listBoxSearch.SelectedIndex = -1;
                    break;
                default:
                    break;
            }

            if (channelExistVideo != string.Empty)
            {
                MessageBox.Show(channelExistVideo + "\n해당 비디오는 이미 채널에 등록되어있습니다.");
            }
            //refreshSearchResult();
            refreshMyChannel();
            //updateChannelFile();
            updateChannelReg();
        }
        
        private void listBoxSearch_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int count = listBoxSearch.SelectedItems.Count;
            if (count == 0)
            {
                buttonAddChannel.Content = "추가";
            }
            else
            {
                buttonAddChannel.Content = "추가 (" + count.ToString() + ")";
            }
        }

        private void listBoxChannel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int count = listBoxChannel.SelectedItems.Count;
            if (count == 0)
            {
                buttonDelete.Content = "삭제";
            }
            else
            {
                buttonDelete.Content = "삭제 (" + count.ToString() + ")";
            }
        }

        private void TabItem_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                refreshMyChannel();
                refreshSearchResult();
                MessageBox.Show("새로고침 완료");
            }
        }
    }
}
