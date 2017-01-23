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
using System.ComponentModel;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
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
        int downloadedCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            email = ApplicationState.GetValue<string>("email");
            getChannelFromReg();
            initData();
            getVideos();
            refreshMyChannel();
            refreshSearchResult();
            loadImage();
            getPlayerSize();
            Mouse.OverrideCursor = null;
        }

        /*
        private void loadImage()
        {
            Dispatcher uiDispatcher = Dispatcher.CurrentDispatcher;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, e) =>
            {
                uiDispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (KeyValuePair<string, Video> v in videos)
                    {
                        //v.Value.img = new BitmapImage(new Uri(v.Value.thumbnail));
                        v.Value.img.BeginInit();
                        v.Value.img.UriSource = new Uri(v.Value.thumbnail);
                        v.Value.img.EndInit();
                        //v.Value.testStr = "이게 도대체 무다...";
                        Console.WriteLine(v.Value.img.UriSource.ToString());
                    }
                }));
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                //((TextBlock)(videos["5717424225648640"].searchGrid.Children[1])).Text = videos["5717800035287040"].id.ToString();
                //((TextBlock)(videos["5717424225648640"].searchGrid.Children[1])).Text = videos["5717800035287040"].testStr;
                ((TextBlock)(videos["5717424225648640"].searchGrid.Children[1])).Text = videos["5717800035287040"].img.UriSource.ToString();
                //((Image)(videos["5717424225648640"].searchGrid.Children[0])).Source = videos["5717800035287040"].img;
                foreach (KeyValuePair<string, Video> v in videos)
                {
                    setImage(v.Value);
                }
            };
            
            worker.RunWorkerAsync();
        }
        */
        /*
        private void loadImage()
        {
            Dispatcher uiDispatcher = Dispatcher.CurrentDispatcher;
            Dictionary<string, BackgroundWorker> workers = new Dictionary<string, BackgroundWorker>();
            int i = 0;
            foreach (KeyValuePair<string, Video> v in videos)
            {
                workers[v.Key] = new BackgroundWorker();

                workers[v.Key].DoWork += (sender, e) =>
                {
                    //v.Value.img = new BitmapImage(new Uri(v.Value.thumbnail));
                    //e.Result = new System.Drawing.Bitmap()
                    //v.Value.uri = new Uri(v.Value.thumbnail);
                    //Image image = new Image();
                    //image.Source = new BitmapImage(new Uri(v.Value.thumbnail));
                    e.Result = new Uri(v.Value.thumbnail);
                    //Image img = new Image();
                    //img.Source = new BitmapImage(new Uri(v.Value.thumbnail));
                    //e.Result = img;
                };

                workers[v.Key].RunWorkerCompleted += (sender, e) =>
                {
                    //v.Value.img.Freeze();
                    //setImage(v.Value, (string)e.Result);
                    //setImage(v.Value, (BitmapImage)e.Result);
                    //setImage(v.Value);
                    setImage(v.Value, (Uri)e.Result);
                    //setImage(v.Value, (Image)e.Result);
                };

                workers[v.Key].RunWorkerAsync();
            }
        }
        */
        /*
        private void loadImage()
        {
            Dictionary<string, Thread> threads = new Dictionary<string, Thread>();
            int i = 0;
            foreach (KeyValuePair<string, Video> v in videos)
            {
                threads[v.Key] = new Thread(() =>
                {
                    Image image = new Image();
                    image.Source = new BitmapImage(new Uri(v.Value.thumbnail));
                    v.Value.searchGrid.Children.RemoveAt(0);
                    v.Value.searchGrid.Children.Insert(0, image);
                    if (isVideoChanneled(v.Value.id.ToString()))
                    {
                        v.Value.channelGrid.Children.RemoveAt(0);
                        v.Value.channelGrid.Children.Insert(0, image);
                    }
                });

                threads[v.Key].SetApartmentState(ApartmentState.STA);
                threads[v.Key].Start();
            }
        }
        */
        
        private void loadImage()
        {
            BackgroundWorker worker = new BackgroundWorker();
            System.Windows.Threading.Dispatcher uiDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            worker.DoWork += (sender, e) =>
            {
                foreach (KeyValuePair<string, Video> v in videos)
                {
                    //v.Value.uri = new Uri(v.Value.thumbnail);
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.DownloadDataAsync(new Uri(v.Value.thumbnail));
                        webClient.DownloadDataCompleted += downloadDataCompleted(v.Value, uiDispatcher);
                        /*
                        webClient.DownloadDataCompleted += (webClient_sender, webClient_e) =>
                        {
                            uiDispatcher.BeginInvoke(new Action(delegate
                            {
                                byte[] imageBytes = webClient_e.Result;
                                using (MemoryStream stream = new MemoryStream(imageBytes))
                                {
                                    v.Value.img = new BitmapImage();
                                    v.Value.img.BeginInit();
                                    v.Value.img.StreamSource = stream;
                                    v.Value.img.CacheOption = BitmapCacheOption.OnLoad;
                                    v.Value.img.EndInit();
                                    v.Value.img.Freeze();
                                    ((Image)(v.Value.searchGrid.Children[0])).Source = v.Value.img;
                                    if (isVideoChanneled(v.Value.id.ToString()))
                                    {
                                        ((Image)(v.Value.channelGrid.Children[0])).Source = v.Value.img;
                                    }
                                }
                            }));
                        };*/
                    }
                }
            };
            /*
            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Cancelled == true)
                {
                    MessageBox.Show("취소됨");
                }
                else if (e.Error != null)
                {
                    MessageBox.Show("이미지 불러오기 실패");
                }
                else
                {
                    setImage();
                }
            };
            */
            worker.RunWorkerAsync();
        }

        private DownloadDataCompletedEventHandler downloadDataCompleted(Video v, System.Windows.Threading.Dispatcher uiDispatcher)
        {
            Action<object, DownloadDataCompletedEventArgs> action = (sender, e) =>
            {
                //byte[] imageBytes = e.Result;
                v.imageBytes = e.Result;
                downloadedCount++;
                if (videos.Count == downloadedCount)
                {
                    uiDispatcher.BeginInvoke(new Action(delegate
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        foreach (KeyValuePair<string, Video> video in videos)
                        {
                            video.Value.img = new BitmapImage();
                            video.Value.img.BeginInit();
                            video.Value.img.StreamSource = new MemoryStream(video.Value.imageBytes);
                            video.Value.img.CacheOption = BitmapCacheOption.OnLoad;
                            video.Value.img.EndInit();
                            video.Value.img.Freeze();
                            ((Image)(video.Value.searchGrid.Children[0])).Source = video.Value.img;
                            if (isVideoChanneled(video.Value.id.ToString()))
                            {
                                ((Image)(video.Value.channelGrid.Children[0])).Source = video.Value.img;
                            }
                        }
                        Mouse.OverrideCursor = null;
                    }));
                }
                /*
                uiDispatcher.BeginInvoke(new Action(delegate
                {
                    using (MemoryStream stream = new MemoryStream(v.imageBytes))
                    {
                        //if (videoCount == downloadedCount)
                        {
                            v.img = new BitmapImage();
                            v.img.BeginInit();
                            v.img.StreamSource = stream;
                            v.img.CacheOption = BitmapCacheOption.OnLoad;
                            v.img.EndInit();
                            v.img.Freeze();
                            ((Image)(v.searchGrid.Children[0])).Source = v.img;
                            if (isVideoChanneled(v.id.ToString()))
                            {
                                ((Image)(v.channelGrid.Children[0])).Source = v.img;
                            }
                            //setImage();
                        }
                    }
                }));*/
                //Console.WriteLine(downloadedCount);
            };
            return new DownloadDataCompletedEventHandler(action);
        }
        
        private void setImage(Video v, Image img)
        {
            v.searchGrid.Children.RemoveAt(0);
            v.searchGrid.Children.Insert(0, img);
            if (isVideoChanneled(v.id.ToString()))
            {
                v.channelGrid.Children.RemoveAt(0);
                v.channelGrid.Children.Insert(0, img);
            }
        }

        private void setImage(Video v, Uri uri)
        {
            ((Image)(v.searchGrid.Children[0])).Source = new BitmapImage(uri);
            if (isVideoChanneled(v.id.ToString()))
            {
                ((Image)(v.channelGrid.Children[0])).Source = new BitmapImage(uri);
            }
        }

        private void setImage(Video v, string str)
        {
            ((TextBlock)(v.searchGrid.Children[1])).Text = str;
            if (isVideoChanneled(v.id.ToString()))
            {
                ((TextBlock)(v.channelGrid.Children[1])).Text = str;
            }
        }

        private void setImage(Video v, BitmapImage img)
        {
            ((Image)(v.searchGrid.Children[0])).Source = img;
            
            if (isVideoChanneled(v.id.ToString()))
            {
                ((Image)(v.channelGrid.Children[0])).Source = img;
            }
        }

        private void setImage(Video v)
        {
            ((Image)(v.searchGrid.Children[0])).Source = v.img;
            if (isVideoChanneled(v.id.ToString()))
            {
                ((Image)(v.channelGrid.Children[0])).Source = v.img;
            }
        }

        private void setImage(string id)
        {
            ((Image)(videos[id].searchGrid.Children[0])).Source = videos[id].img;
            if (isVideoChanneled(videos[id].id.ToString()))
            {
                ((Image)(videos[id].channelGrid.Children[0])).Source = videos[id].img;
            }
        }

        private void setImage()
        {
            foreach (KeyValuePair<string, Video> v in videos)
            {
                ((Image)(v.Value.searchGrid.Children[0])).Source = v.Value.img;
                if (isVideoChanneled(v.Value.id.ToString()))
                {
                    ((Image)(v.Value.channelGrid.Children[0])).Source = v.Value.img;
                }
            }
        }
        
        /*
        private void setImage(Video v, BitmapImage img)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                ((Image)(v.searchGrid.Children[0])).Source = img;
                if (isVideoChanneled(v.id.ToString()))
                {
                    ((Image)(v.channelGrid.Children[0])).Source = img;
                }
            }));
        }
        */

        private void initData()
        {
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
            }

            RegistryKey regConfig = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\config", true);
            if (regConfig.GetValue("auto_login") != null)
            {
                checkBoxAutoLogin.IsChecked = true;
            }
        }

        private void getPlayerSize()
        {
            RegistryKey regConfig = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\config", true);
            if (regConfig.GetValue("width") != null)
            {
                textBoxWidth.Text = regConfig.GetValue("width") as string;
            }
            else
            {
                textBoxWidth.Text = "960";
            }
            if (regConfig.GetValue("height") != null)
            {
                textBoxHeight.Text = regConfig.GetValue("height") as string;
            }
            else
            {
                textBoxHeight.Text = "540";
            }
            if (regConfig.GetValue("x") != null)
            {
                textBoxX.Text = regConfig.GetValue("x") as string;
            }
            else
            {
                textBoxX.Text = ((SystemParameters.PrimaryScreenWidth - 960D) / 2).ToString();
            }
            if (regConfig.GetValue("y") != null)
            {
                textBoxY.Text = regConfig.GetValue("y") as string;
            }
            else
            {
                textBoxY.Text = ((SystemParameters.PrimaryScreenHeight - 540D) / 2).ToString();
            }
        }

        private void getVideos()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL2);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(
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
                    v.img = new BitmapImage(new Uri("img/thumbnail.png", UriKind.Relative));
                    v.searchGrid = null;
                    v.channelGrid = null;
                    videos.Add(v.id.ToString(), v);
                    searchResult.Add(v.id.ToString());
                }
                ApplicationState.SetValue("videos", videos);
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
            /*
            if (textBoxSearch.Text == string.Empty)
            {
                MessageBox.Show("검색어를 입력하세요\n전체 비디오를 보시려면 * 을 검색하세요.", "검색");
                return;
            }
            */
            searchResult.Clear();
            listBoxSearch.Items.Clear();
            buttonAddChannel.Content = "추가";
            listBoxSearch.SelectedIndex = -1;
            string selectedTag = ((ComboBoxItem)comboBoxCategory.SelectedItem).Tag.ToString();

            if (textBoxSearch.Text == "")
            {
                foreach (KeyValuePair<string, Video> v in videos)
                {
                    if (selectedTag == "all")
                    {
                        searchResult.Add(v.Value.id.ToString());
                    }
                    else if (v.Value.code.Substring(0,1) == selectedTag)
                    { 
                        searchResult.Add(v.Value.id.ToString());
                        //addVideoToListView(v.Value, listViewSearch);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, Video> v in videos)
                {
                    if (v.Value.title.IndexOf(textBoxSearch.Text) > -1)
                    {
                        if (selectedTag == "all")
                        {
                            searchResult.Add(v.Value.id.ToString());
                        }
                        else if (v.Value.code.Substring(0, 1) == selectedTag)
                        {
                            searchResult.Add(v.Value.id.ToString());
                            //addVideoToListView(v.Value, listViewSearch);
                        }
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

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
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
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
            Grid.SetRow(border, 1);
            Grid.SetColumnSpan(border, 4);

            ContextMenu contextMenu = new ContextMenu();

            MenuItem menuItemAdd = new MenuItem();
            menuItemAdd.Header = "채널에 추가";
            menuItemAdd.Click += MenuItemAdd_Click;

            MenuItem menuItemRemove = new MenuItem();
            menuItemRemove.Header = "채널에서 삭제";
            menuItemRemove.Click += MenuItemRemove_Click;

            MenuItem menuItemPreview = new MenuItem();
            menuItemPreview.Header = "[" + v.title + "]\n미리보기";
            menuItemPreview.Click += MenuItemPreview_Click(v.id);

            MenuItem menuItemCancel = new MenuItem();
            menuItemCancel.Header = "취소";
            menuItemCancel.Click += MenuItemCancel_Click(contextMenu);

            Separator separator = new Separator();

            //Image thumbnail = new Image();
            /*
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(v.thumbnail, UriKind.Relative);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            //thumbnail.Source = new BitmapImage(new Uri(v.thumbnail));
            */
            Image thumbnail = new Image();
            //thumbnail.Source = new BitmapImage(v.uri);
            //thumbnail.Source = (v.img == null) ? new BitmapImage(new Uri("img/thumbnail.png", UriKind.Relative)) : v.img;
            thumbnail.Source = v.img;
            thumbnail.Stretch = Stretch.Fill;
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

            if (lb.Name == "listBoxSearch")
            {
                //contextMenu.Items.Add(menuItemAdd);
                contextMenu.Items.Add(menuItemPreview);
                //contextMenu.Items.Add(separator);
                //contextMenu.Items.Add(menuItemCancel);
                grid.ContextMenu = contextMenu;
            }
            if (lb.Name == "listBoxChannel")
            {
                //contextMenu.Items.Add(menuItemRemove);
            }
            //contextMenu.Items.Add(separator);
            //contextMenu.Items.Add(menuItemDownload);

            grid.Name = (lb.Name == "listBoxSearch" ? "s" : "c") + v.id.ToString();
            //grid.MouseLeftButtonDown += Grid_MouseLeftButtonDown;
            grid.ToolTip = toolTip;
            lb.Items.Add(grid);
            if (lb.FindName(grid.Name) == null)
            {
                lb.RegisterName(grid.Name, grid);
            }
            else
            {
                lb.UnregisterName(grid.Name);
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
                    Image channeled = new Image();
                    channeled.Source = new BitmapImage(new Uri("img/channel.png", UriKind.Relative));
                    Grid.SetRowSpan(channeled, 2);
                    channeled.Width = 18;
                    channeled.Margin = new Thickness(0, 0, 4, 4);
                    channeled.HorizontalAlignment = HorizontalAlignment.Right;
                    channeled.VerticalAlignment = VerticalAlignment.Bottom;
                    grid.Children.Add(channeled);
                }
                v.searchGrid = grid;
            }
            else
            {
                if (!isVideoDownloaded(v.code))
                {
                    Image saved = new Image();
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
                    Image play = new Image();
                    play.Source = new BitmapImage(new Uri("img/Icon-Closed.png", UriKind.Relative));
                    play.Width = 28;
                    play.Cursor = Cursors.Help;
                    play.ToolTip = "컨텐츠의 재생권한이 필요합니다.";
                    Grid.SetColumn(play, 3);
                    Grid.SetRow(play, 1);
                    grid.Children.Add(play);
                }
                v.channelGrid = grid;
            }
        }
        private RoutedEventHandler MenuItemCancel_Click(ContextMenu contextMenu)
        {
            Action<object, RoutedEventArgs> action = (sender, e) =>
            {
                contextMenu.IsOpen = false;
            };
            return new RoutedEventHandler(action);
        }

        private RoutedEventHandler MenuItemPreview_Click(long videoId)
        {
            Action<object, RoutedEventArgs> action = (sender, e) =>
            {
                previewVideo(videoId);
            };
            return new RoutedEventHandler(action);
        }

        private void previewVideo(long videoId)
        {
            string url = URL2 + "tvapp/" + user.uid.ToString() + "/" + user.did.ToString() + "/play";
            
            PreviewPlayWindow previewPlayWindow = new PreviewPlayWindow(url, user, videoId);
            previewPlayWindow.Top = double.Parse(textBoxY.Text);
            previewPlayWindow.Left = double.Parse(textBoxX.Text);
            previewPlayWindow.ShowDialog();
            previewPlayWindow.webBrowser.Navigate("about:blank");
            /*
            previewPlayWindow.Height = double.Parse(textBoxHeight.Text);
            previewPlayWindow.Width = double.Parse(textBoxWidth.Text);
            previewPlayWindow.Top = double.Parse(textBoxY.Text);
            previewPlayWindow.Left = double.Parse(textBoxX.Text);
            previewPlayWindow.ShowDialog();

            textBoxHeight.Text = previewPlayWindow.Height.ToString();
            textBoxWidth.Text = previewPlayWindow.Width.ToString();
            textBoxX.Text = previewPlayWindow.Left.ToString();
            textBoxY.Text = previewPlayWindow.Top.ToString();

            savePlayerSize();
            */
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
                    var selected = listBoxChannel.SelectedItems.Cast<object>().ToArray();
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
                            Encoding.ASCII.GetBytes(
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
                                            Encoding.ASCII.GetBytes(
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
                //selectedVideosStr = "%5B" + string.Join(",", selectedVideosList) + "%5D";
                // selectedVideosStr = [id,id]
                /*
                ProcessStartInfo startInfo = new ProcessStartInfo("IEXPLORE.EXE");
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.Arguments = URL2 + "tvapp/" + user.uid.ToString() + "/" + user.did.ToString() + "/play?videos=" + selectedVideosStr + "&localPath=/" + Uri.EscapeDataString(filePath.Replace("\\", "/").Substring(3, filePath.Length - 4));
                Process.Start(startInfo);
                */

                string url = URL2 + "tvapp/" + user.uid.ToString() + "/" + user.did.ToString() + "/play";
                string videos = "[" + string.Join(",", selectedVideosList) + "]";
                string localPath  = "/" + filePath.Replace("\\", "/").Substring(3, filePath.Length - 4);
                PlayWindow playWindow = new PlayWindow(url, user, videos, localPath);
                try
                {
                    playWindow.Height = double.Parse(textBoxHeight.Text);
                    playWindow.Width = double.Parse(textBoxWidth.Text);
                    playWindow.Top = double.Parse(textBoxY.Text);
                    playWindow.Left = double.Parse(textBoxX.Text);
                }
                catch (Exception ex)
                {
                    playWindow.Height = 540D;
                    playWindow.Width = 960D;
                    playWindow.Top = (SystemParameters.PrimaryScreenHeight - 540D) / 2;
                    playWindow.Left = (SystemParameters.PrimaryScreenWidth - 960D) / 2;
                }

                playWindow.ShowDialog();
                playWindow.webBrowser.Navigate("about:blank");
                textBoxHeight.Text = playWindow.Height.ToString();
                textBoxWidth.Text = playWindow.Width.ToString();
                textBoxX.Text = playWindow.Left.ToString();
                textBoxY.Text = playWindow.Top.ToString();
                
                savePlayerSize();
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
            string message = "정말로 비디오를 삭제하시겠습니까?";
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
            foreach (string id in myChannel)
            {
                addVideoToListView(videos[id], listBoxChannel);
            }
            updateChannelReg();
        }

        private void refreshSearchResult()
        {
            listBoxSearch.Items.Clear();

            foreach (string id in searchResult)
            {
                addVideoToListView(videos[id], listBoxSearch);
            }
        }

        private void changeChanneledThumb(string changedVideo, bool isAdd)
        {
            Grid grid = (Grid)(listBoxSearch.FindName("s" + changedVideo));
            if (grid != null)
            {
                if (isAdd)
                {
                    Image channeled = new Image();
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
                    if (!myChannel.Contains(changedVideo))
                    {
                        if (grid.Children.Count > 4)
                        {
                            grid.Children.RemoveAt(4);
                        }
                    }
                }
            }
        }

        private void buttonNewUser_Click(object sender, RoutedEventArgs e)
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
            string message = "선택된 컨텐츠를 채널에 추가하시겠습니까?";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(message, caption, button, icon);
            string channelExistVideo = string.Empty;

            bool isDuplicated = false;

            switch (result)
            {
                case MessageBoxResult.Yes:
                    var selected = listBoxSearch.SelectedItems.Cast<Object>().ToArray();
                    foreach (var item in selected)
                    {
                        if (myChannel.Contains(((Grid)item).Name.Substring(1)))
                        {
                            //channelExistVideo += videos[((Grid)item).Name.Substring(1)].title + "\n";
                            //continue;
                            isDuplicated = true;
                            caption = "중복된 비디오";
                            message = videos[((Grid)item).Name.Substring(1)].title + "\n이 컨텐츠는 이미 등록된 컨텐츠입니다.\n그래도 등록 하시겠습니까?";
                            button = MessageBoxButton.YesNo;
                            icon = MessageBoxImage.Warning;
                            MessageBoxResult result2 = MessageBox.Show(message, caption, button, icon);
                            switch (result2)
                            {
                                case MessageBoxResult.Yes:
                                    myChannel.Add(((Grid)item).Name.Substring(1));
                                    continue;
                                default:
                                    continue;
                            }
                        }
                        if (!isDuplicated)
                        {
                            myChannel.Add(((Grid)item).Name.Substring(1));
                            changeChanneledThumb(((Grid)item).Name.Substring(1), true);
                        }
                        else
                        {
                            isDuplicated = false;
                        }
                        //addVideoToListView(videos[((Grid)item).Name.Substring(1)], listViewChannel);
                    }
                    buttonAddChannel.Content = "추가";
                    listBoxSearch.SelectedIndex = -1;
                    break;
                default:
                    break;
            }
            /*
            if (channelExistVideo != string.Empty)
            {
                MessageBox.Show(channelExistVideo + "\n해당 비디오는 이미 채널에 등록되어있습니다.");
            }*/
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

        private void buttonChannelImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "데이터 파일 (*.dat)|*.dat";
            if (openFileDialog.ShowDialog() == true)
            {
                string channeledVideos = File.ReadAllText(openFileDialog.FileName);

                if (myChannel.Count != 0)
                {
                    string caption = "채널 가져오기";
                    string message = "이미 채널이 존재합니다.\n기존 채널을 삭제 후 추가하시겠습니까?\n\n아니오를 누르면 기존 채널에 추가됩니다.";
                    MessageBoxButton button = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBoxResult result = MessageBox.Show(message, caption, button, icon);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                                myChannel.Clear();
                            break;
                        default:
                            break;
                    }
                }

                foreach (string videoID in channeledVideos.Split(','))
                {
                    myChannel.Add(videoID);
                }

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
                refreshMyChannel();
            }
        }

        private void buttonChannelExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.Filter = "데이터 파일 (*.dat)|*.dat";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, string.Join(",", myChannel));
            }          
        }

        private void buttonSetWindowPosition_Click(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double width = double.Parse(textBoxWidth.Text);
            double height = double.Parse(textBoxHeight.Text);
            double top = double.Parse(textBoxY.Text);
            double left = double.Parse(textBoxX.Text);

            SettingWindow settingWindow = new SettingWindow();
            settingWindow.Top = (SystemParameters.PrimaryScreenHeight - settingWindow.Height) / 2;
            settingWindow.Left = (SystemParameters.PrimaryScreenWidth - settingWindow.Width) / 2;
            settingWindow.ShowDialog();
            
            switch (settingWindow.location)
            {
                case "buttonLT":
                    break;
                case "buttonLC":
                    top = (screenHeight - height) / 2;
                    break;
                case "buttonLB":
                    top = (screenHeight - height);
                    break;
                case "buttonCT":
                    left = (screenWidth - width) / 2;
                    break;
                case "buttonCC":
                    left = (screenWidth - width) / 2;
                    top = (screenHeight - height) / 2;
                    break;
                case "buttonCB":
                    left = (screenWidth - width) / 2;
                    top = (screenHeight - height);
                    break;
                case "buttonRT":
                    left = (screenWidth - width);
                    break;
                case "buttonRC":
                    left = (screenWidth - width);
                    top = (screenHeight - height) / 2;
                    break;
                case "buttonRB":
                    left = (screenWidth - width);
                    top = (screenHeight - height);
                    break;
                default:
                    break;
            }

            textBoxX.Text = left.ToString();
            textBoxY.Text = top.ToString();

            savePlayerSize();
        }

        private void buttonSetWindowSize_Click(object sender, RoutedEventArgs e)
        {
            SetPlayerSize setplayerSize = new SetPlayerSize();
            setplayerSize.Height = double.Parse(textBoxHeight.Text);
            setplayerSize.Width = double.Parse(textBoxWidth.Text);
            setplayerSize.Top = double.Parse(textBoxY.Text);
            setplayerSize.Left = double.Parse(textBoxX.Text);
            setplayerSize.ShowDialog();

            textBoxHeight.Text = setplayerSize.Height.ToString();
            textBoxWidth.Text = setplayerSize.Width.ToString();
            textBoxX.Text = setplayerSize.Left.ToString();
            textBoxY.Text = setplayerSize.Top.ToString();

            savePlayerSize();
        }

        private void buttonListUp_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxChannel.SelectedItems.Count == 0)
            {
                MessageBox.Show("컨텐츠를 선택해주세요.");
                return;
            }

            List<int> targetIndex = new List<int>();
            List<string> selectedList = new List<string>();
            string temp = default(string);
            var selected = listBoxChannel.SelectedItems.Cast<object>().ToArray();
            foreach (var item in selected)
            {
                selectedList.Add(((Grid)item).Name.Substring(1));
            }
            for (int ii = myChannel.Count; Convert.ToBoolean(ii--); )
            {
                if (selectedList.Contains(myChannel[ii]))
                {
                    if (ii == 0)
                    {
                        break;
                    }
                    targetIndex.Add(ii);
                    if (!selectedList.Contains(myChannel[(ii - 1)]))
                    {
                        temp = myChannel[(ii - 1)];
                        for (int jj = targetIndex.Count; Convert.ToBoolean(jj--); )
                        {
                            myChannel[targetIndex[jj] - 1] = myChannel[targetIndex[jj]];
                        }
                        myChannel[targetIndex[0]] = temp;
                        ii--;
                        targetIndex.Clear();
                    }
                }
            }
            refreshMyChannel();
        }

        private void buttonListDown_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxChannel.SelectedItems.Count == 0)
            {
                MessageBox.Show("컨텐츠를 선택해주세요.");
                return;
            }
        }

        private void textBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int result = default(int);

            if (!(int.TryParse(e.Text, out result) || e.Text == "."))
            {
                e.Handled = true;
            }
        }

        private void savePlayerSize()
        {
            RegistryKey regConfig = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\config", true);
            regConfig.SetValue("width", textBoxWidth.Text);
            regConfig.SetValue("height", textBoxHeight.Text);
            regConfig.SetValue("x", textBoxX.Text);
            regConfig.SetValue("y", textBoxY.Text);
        }

        private void textBoxSearch_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            searchResult.Clear();
            listBoxSearch.Items.Clear();
            buttonAddChannel.Content = "추가";
            listBoxSearch.SelectedIndex = -1;
            string selectedTag = ((ComboBoxItem)comboBoxCategory.SelectedItem).Tag.ToString();

            if (textBoxSearch.Text == "")
            {
                foreach (KeyValuePair<string, Video> v in videos)
                {
                    if (selectedTag == "all")
                    {
                        searchResult.Add(v.Value.id.ToString());
                    }
                    else if (v.Value.code.Substring(0, 1) == selectedTag)
                    {
                        searchResult.Add(v.Value.id.ToString());
                        //addVideoToListView(v.Value, listViewSearch);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, Video> v in videos)
                {
                    if (v.Value.title.IndexOf(textBoxSearch.Text) > -1)
                    {
                        if (selectedTag == "all")
                        {
                            searchResult.Add(v.Value.id.ToString());
                        }
                        else if (v.Value.code.Substring(0, 1) == selectedTag)
                        {
                            searchResult.Add(v.Value.id.ToString());
                            //addVideoToListView(v.Value, listViewSearch);
                        }
                    }
                }
            }
            refreshSearchResult();
        }

        private void comboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            searchResult.Clear();
            if (listBoxSearch != null)
            {
                listBoxSearch.Items.Clear();
                buttonAddChannel.Content = "추가";
                listBoxSearch.SelectedIndex = -1;
            }
            string selectedTag = ((ComboBoxItem)comboBoxCategory.SelectedItem).Tag.ToString();

            if (textBoxSearch != null)
            {
                if (textBoxSearch.Text == "")
                {
                    foreach (KeyValuePair<string, Video> v in videos)
                    {
                        if (selectedTag == "all")
                        {
                            searchResult.Add(v.Value.id.ToString());
                        }
                        else if (v.Value.code.Substring(0, 1) == selectedTag)
                        {
                            searchResult.Add(v.Value.id.ToString());
                            //addVideoToListView(v.Value, listViewSearch);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, Video> v in videos)
                    {
                        if (v.Value.title.IndexOf(textBoxSearch.Text) > -1)
                        {
                            if (selectedTag == "all")
                            {
                                searchResult.Add(v.Value.id.ToString());
                            }
                            else if (v.Value.code.Substring(0, 1) == selectedTag)
                            {
                                searchResult.Add(v.Value.id.ToString());
                                //addVideoToListView(v.Value, listViewSearch);
                            }
                        }
                    }
                }
                refreshSearchResult();
            }
        }

        private void listBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void buttonAppInfo_Click(object sender, RoutedEventArgs e)
        {
            AppInfoWindow appInfoWindow = new AppInfoWindow(ApplicationState.GetValue<string>("appVersion"));
            appInfoWindow.Left = (SystemParameters.PrimaryScreenWidth - appInfoWindow.Width) / 2;
            appInfoWindow.Top = (SystemParameters.PrimaryScreenHeight - appInfoWindow.Height) / 2;
            appInfoWindow.ShowDialog();
        }

        private void checkBoxAutoLogin_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey regConfig = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\config", true);

            if (((CheckBox)sender).IsChecked == true)
            {
                regConfig.SetValue("auto_login", true);
            }
            else
            {
                if (regConfig.GetValue("auto_login") != null)
                    regConfig.DeleteValue("auto_login");
            }
        }
    }
}
