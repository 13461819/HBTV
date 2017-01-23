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
using System.Threading;
using System.Security.Cryptography;

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

            if (regConfig.GetValue("last_email") != null)
            {
                textBoxEmail.Text = regConfig.GetValue("last_email") as string;
                textBoxName.Text = getNameFromReg(textBoxEmail.Text);
                passwordBoxPassword.Focus();
                if (regConfig.GetValue("auto_login") != null)
                {
                    passwordBoxPassword.Password = getPasswordFromReg(textBoxEmail.Text);
                    login();
                }
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

            login();
        }

        private void login()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            buttonLogin.Content = "로그인중";
            buttonLogin.IsEnabled = false;

            Dictionary<string, object> dataObject = new Dictionary<string, object>();
            dataObject.Add("email", textBoxEmail.Text);
            dataObject.Add("password", passwordBoxPassword.Password);
            dataObject.Add("p_id", getCpuId());
            dataObject.Add("name", textBoxName.Text);
            dataObject.Add("version", int.Parse(ApplicationState.appVersion.Replace(".", string.Empty)));

            var data = JsonConvert.SerializeObject(dataObject);
            var content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpResponseMessage response = client.PostAsync("tvapp/login", content).Result;
                //response.EnsureSuccessStatusCode();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // 이메일 데이터만 저장하고 바로 MainWindow 호출
                    ApplicationState.SetValue("email", textBoxEmail.Text);
                    //MainWindow mainWondow = new MainWindow();
                    //mainWondow.Show();

                    // MainWindow에서는 로드되고 난 후 나머지 데이터 기다림
                    //Mouse.OverrideCursor = null;
                    string responseText = response.Content.ReadAsStringAsync().Result;
                    dynamic jo = JObject.Parse(responseText);
                    user = JsonConvert.DeserializeObject<User>(responseText);
                    ApplicationState.SetValue("currentUser", user);
                    ApplicationState.SetValue("isDataStored", true);
                    RegistryKey regConfig = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\config", true);
                    ApplicationState.SetValue("emr_id", regConfig.GetValue("emr_id") as string);
                    regConfig.SetValue("last_email", textBoxEmail.Text);
                    setNameToReg(textBoxEmail.Text, textBoxName.Text);
                    setPasswordToReg(textBoxEmail.Text, passwordBoxPassword.Password);

                    MainWindow mainWondow = new MainWindow();
                    mainWondow.Show();

                    // logo.wvm 파일 다운로드
                    Thread downloadLogoVideoThread = new Thread(new ThreadStart(downloadLogoVideo));
                    downloadLogoVideoThread.SetApartmentState(ApartmentState.STA);
                    downloadLogoVideoThread.IsBackground = true;
                    downloadLogoVideoThread.Start();
                    Close();
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    Mouse.OverrideCursor = null;
                    string responseText = response.Content.ReadAsStringAsync().Result;
                    dynamic jo = JObject.Parse(responseText);
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
                }
            }
            catch (HttpRequestException he)
            {
                MessageBox.Show("로그인 에러\n" + he.Message, "로그인 실패");
                buttonLogin.Content = "로그인";
                buttonLogin.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n네트워크 연결을 확인해주세요.(01)", "로그인 실패");
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
                regEmail = regHealthBreeze.CreateSubKey(email);
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

        private void setPasswordToReg(string email, string password)
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
                regEmail = regHealthBreeze.CreateSubKey(email);
            }

            regEmail.SetValue("password", encryptPassword(email, password));
        }

        private string getPasswordFromReg(string email)
        {
            string password = string.Empty;

            RegistryKey regEmail = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\" + email, true);

            if (regEmail != null)
            {
                if (regEmail.GetValue("password") != null)
                {
                    password = regEmail.GetValue("password") as string;
                }
            }

            return decryptPassword(email, password);
        }

        private void textBoxEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            textBoxName.Text = getNameFromReg(textBoxEmail.Text);
        }

        private void downloadLogoVideo()
        {
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

        private string encryptPassword(string email, string password)
        {
            byte[] hash = SHA256Hash(email);
            byte[] key = new byte[16];
            Array.Copy(hash, 0, key, 0, 16);
            byte[] iv = new byte[16];
            Array.Copy(hash, 16, iv, 0, 16);
            return AESEncrypt128(password, key, iv);
        }

        private string decryptPassword(string email, string password)
        {
            byte[] hash = SHA256Hash(email);
            byte[] key = new byte[16];
            Array.Copy(hash, 0, key, 0, 16);
            byte[] iv = new byte[16];
            Array.Copy(hash, 16, iv, 0, 16);
            return AESDecrypt128(password, key, iv);
        }

        private byte[] SHA256Hash(string email)
        {
            SHA256 sha256 = new SHA256Managed();
            return sha256.ComputeHash(Encoding.ASCII.GetBytes(email));
        }

        public string AESEncrypt128(string password, byte[] key, byte[] iv)
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged();

            // 입력받은 문자열을 바이트 배열로 변환  
            byte[] plainText = Encoding.Unicode.GetBytes(password);

            // 딕셔너리 공격을 대비해서 키를 더 풀기 어렵게 만들기 위해서   
            // Salt를 사용한다.  
            byte[] salt = Encoding.ASCII.GetBytes(password.Length.ToString());

            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(password, salt);


            // Create a encryptor from the existing SecretKey bytes.  
            // encryptor 객체를 SecretKey로부터 만든다.  
            // Secret Key에는 16바이트  
            // Initialization Vector로 16바이트를 사용  
            ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(key, iv);

            MemoryStream memoryStream = new MemoryStream();

            // CryptoStream객체를 암호화된 데이터를 쓰기 위한 용도로 선언  
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(plainText, 0, plainText.Length);

            cryptoStream.FlushFinalBlock();

            byte[] CipherBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            string EncryptedData = Convert.ToBase64String(CipherBytes);

            return EncryptedData;
        }

        public string AESDecrypt128(string password, byte[] key, byte[] iv)
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged();

            byte[] encryptedData = Convert.FromBase64String(password);
            byte[] salt = Encoding.ASCII.GetBytes(password.Length.ToString());

            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(password, salt);

            // Decryptor 객체를 만든다.  
            ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(key, iv);

            MemoryStream memoryStream = new MemoryStream(encryptedData);

            // 데이터 읽기 용도의 cryptoStream객체  
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            // 복호화된 데이터를 담을 바이트 배열을 선언한다.  
            byte[] plainText = new byte[encryptedData.Length];

            int decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);

            memoryStream.Close();
            cryptoStream.Close();

            string DecryptedData = Encoding.Unicode.GetString(plainText, 0, decryptedCount);

            return DecryptedData;
        }
    }
}
