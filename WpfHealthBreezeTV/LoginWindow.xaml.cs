using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.Management;
using Newtonsoft.Json.Linq;
using System.Windows.Input;
using System.Net;
using System.IO;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// LoginWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string URL = string.Empty;
        private User user;

        public LoginWindow()
        {
            InitializeComponent();
            getInitData();
        }
        private void getInitData()
        {
            switch (ApplicationState.runAt)
            {
                case "server":
                    URL = ApplicationState.serverUrl;
                    break;
                case "test":
                    URL = ApplicationState.testUrl;
                    break;
                case "local":
                    URL = ApplicationState.localUrl2;
                    break;
                default:
                    break;
            }
            RegistryKey regConfig = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\config", true);

            if (regConfig == null)
            {
                RegistryKey regHealthBreeze = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze", true);
                if (regHealthBreeze == null)
                {
                    RegistryKey regSoftware = Registry.CurrentUser.OpenSubKey("Software", true);
                    regHealthBreeze = regSoftware.CreateSubKey("HealthBreeze");
                }
                regConfig = regHealthBreeze.CreateSubKey("config");
            }

            if (regConfig.GetValue("email") != null)
            {
                textBoxEmail.Text = regConfig.GetValue("email") as string;
                textBoxName.Text = getNameFromReg(textBoxEmail.Text);
                passwordBoxPassword.Focus();
            }
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxEmail.Text.Length == 0)
            {
                MessageBox.Show("email은 비워두실 수 없습니다.", "로그인 실패");
                return;
            }
            if (passwordBoxPassword.Password.Length < 8)
            {
                MessageBox.Show("비밀번호는 최소 8자리입니다.", "로그인 실패");
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            buttonLogin.Content = "로그인중";
            buttonLogin.IsEnabled = false;
            

            Dictionary<string, string> dataObject = new Dictionary<string, string>();
            dataObject.Add("email", textBoxEmail.Text);
            dataObject.Add("password", passwordBoxPassword.Password);
            dataObject.Add("p_id", getCpuId());
            dataObject.Add("name", textBoxName.Text);

            var data = JsonConvert.SerializeObject(dataObject);
            var content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.PostAsync("tvapp/login", content).Result;
            try
            {
                //response.EnsureSuccessStatusCode();
                string responseText = response.Content.ReadAsStringAsync().Result;
                dynamic jo = JObject.Parse(responseText);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    user = JsonConvert.DeserializeObject<User>(responseText);
                    ApplicationState.SetValue("currentUser", user);
                    ApplicationState.SetValue("email", textBoxEmail.Text);
                    RegistryKey regConfig = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\config", true);
                    regConfig.SetValue("email", textBoxEmail.Text);
                    setNameToReg(textBoxEmail.Text, textBoxName.Text);
                    downloadLogoVideo();
                    MainWindow mainWondow = new MainWindow();
                    mainWondow.Show();
                    Mouse.OverrideCursor = null;
                    this.Close();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    switch ((string)jo.code)
                    {
                        case "2004":
                            MessageBox.Show("email과 비밀번호를 확인해주세요.");
                            break;
                        case "2011":
                            MessageBox.Show("유효하지 않은 요청입니다.");
                            break;
                        default:
                            MessageBox.Show("로그인 에러\n" + jo.code + ": " + jo.message);
                            break;
                    }
                    buttonLogin.Content = "로그인";
                    buttonLogin.IsEnabled = true;
                    Mouse.OverrideCursor = null;
                }
            }
            catch (HttpRequestException he)
            {
                MessageBox.Show("로그인 에러\n" + he.Message, "로그인 실패");
                buttonLogin.Content = "로그인";
                buttonLogin.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        private void textBlockSignUp_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow(true);
            try
            {
                registerWindow.ShowDialog();
            }
            catch
            {

            }
        }

        public string getCpuId()
        {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["processorID"].Value.ToString();
                break;
            }

            return cpuInfo;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonLogin_Click(sender, e);
            }
        }

        private void setNameToReg(string email, string name)
        {
            RegistryKey regEmail = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\" + email, true);

            if (regEmail == null)
            {
                RegistryKey regHealthBreeze = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze", true);
                if (regHealthBreeze == null)
                {
                    RegistryKey regSoftware = Registry.CurrentUser.OpenSubKey("Software", true);
                    regHealthBreeze = regSoftware.CreateSubKey("HealthBreeze");
                }
                regEmail = regHealthBreeze.CreateSubKey("config");
            }

            regEmail.SetValue("name", name);
        }

        private string getNameFromReg(string email)
        {
            string name = string.Empty;

            RegistryKey regEmail = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\" + email, true);

            if (regEmail != null)
            {
                if (regEmail.GetValue("name") != null)
                {
                    name = regEmail.GetValue("name") as string;
                }
            }

            return name;
        }

        private void textBoxEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            textBoxName.Text = getNameFromReg(textBoxEmail.Text);
        }

        private void downloadLogoVideo()
        {
            //if (File.Exists(ApplicationState.filePath + "logo.wvm")) return;
            
            using (WebClient webClient = new WebClient())
            {
                Uri from;
                try
                {
                    Stream stream = webClient.OpenRead(user.logos);
                    if (stream != null)
                    {
                        stream.Close();
                        from = new Uri(user.logos);
                        webClient.DownloadFileAsync(from, ApplicationState.filePath + "logo.wvm");
                    }
                    else
                    {
                        from = new Uri(user.logos.Substring(0, user.logos.LastIndexOf("/") + 1) + "logo.wvm");
                        webClient.DownloadFileAsync(from, ApplicationState.filePath + "logo.wvm");
                    }
                }
                catch
                {
                    from = new Uri(user.logos.Substring(0, user.logos.LastIndexOf("/") + 1) + "logo.wvm");
                    webClient.DownloadFileAsync(from, ApplicationState.filePath + "logo.wvm");
                }
            }
        }
    }
}
