using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// ChangePasswordWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        public bool isCanceled = true;
        private string email = string.Empty;
        private string URL = ApplicationState.serverUrl;

        public ChangePasswordWindow()
        {
            InitializeComponent();
            email = ApplicationState.GetValue<string>("email");
        }

        private void buttonCurrentPassword_Click(object sender, RoutedEventArgs e)
        {
            if (passwordBoxCurrentPassword.Password == getPasswordFromReg(email))
            {
                passwordBoxNewPassword.IsEnabled = true;
                passwordBoxConfirmNewPassword.IsEnabled = true;
                passwordBoxNewPassword.Focus();
            }
            else
            {
                MessageBox.Show("입력하신 현재 비밀번호가 잘못되었습니다.");
                passwordBoxCurrentPassword.Password = string.Empty;
                passwordBoxCurrentPassword.Focus();
            }
        }

        private void buttonNewPassword_Click(object sender, RoutedEventArgs e)
        {
            if (isEnteredValidValue())
            {
                string user_id = string.Empty;
                string user_api_key = string.Empty;
                Dictionary<string, object> dataObject = new Dictionary<string, object>();
                dataObject.Add("email", email);
                dataObject.Add("password", passwordBoxConfirmNewPassword.Password);
                var data = JsonConvert.SerializeObject(dataObject);
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}-{1}:{2}",
                                            ApplicationState.GetValue<User>("currentUser").uid,
                                            ApplicationState.GetValue<User>("currentUser").did,
                                            ApplicationState.GetValue<User>("currentUser").sessionKey))));
                try
                {
                    HttpResponseMessage response = client.PutAsync("tvapp/login", content).Result;
                    isCanceled = false;
                    Close();
                }
                catch (HttpRequestException he)
                {
                    MessageBox.Show("비밀번호 변경 실패\n" + he.Message, "변경 실패");
                    clearText();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace + "\n네트워크 연결을 확인해주세요.");
                    clearText();
                }
            }
        }

        private bool isEnteredValidValue()
        {
            bool result = true;

            if (passwordBoxCurrentPassword.Password != getPasswordFromReg(email))
            {
                MessageBox.Show("입력하신 현재 비밀번호가 잘못되었습니다.");
                clearText();
                return false;
            }

            if (passwordBoxNewPassword.Password != passwordBoxConfirmNewPassword.Password)
            {
                MessageBox.Show("입력하신 비밀번호가 서로 다릅니다.");
                clearText();
                return false;
            }

            if (passwordBoxConfirmNewPassword.Password.Length < 8)
            {
                MessageBox.Show("비밀번호는 최소 8자리 이상입니다.");
                clearText();
                return false;
            }
            return result;
        }

        private void clearText()
        {

            passwordBoxCurrentPassword.Password = string.Empty;
            passwordBoxNewPassword.Password = string.Empty;
            passwordBoxConfirmNewPassword.Password = string.Empty;
            passwordBoxCurrentPassword.Focus();
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

        private string getNickNameFromReg(string email)
        {
            string nickName = string.Empty;

            RegistryKey regEmail = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\" + email, true);

            if (regEmail != null)
            {
                if (regEmail.GetValue("nickName") != null)
                {
                    nickName = regEmail.GetValue("nickName") as string;
                }
            }

            return nickName;
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
