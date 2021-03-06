﻿using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;

namespace WpfHealthBreezeTV
{
    /// <summary>
    /// RegisterWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private string URL = string.Empty;
        private string URL2 = string.Empty;
        private bool isManagerRegister = true;
        private string smsId = string.Empty;
        private string msIsdn = string.Empty;
        private bool isPhoneVerify = false;
        private bool isPhoneCheck = true;

        public RegisterWindow(bool isManagerRegister)
        {
            InitializeComponent();
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
            this.isManagerRegister = isManagerRegister;
            if (isManagerRegister)
            {
                if (isEMRAleadyExist())
                {
                    if (isManagerAleadyExist())
                    {
                        MessageBox.Show("이미 등록이 완료되었습니다.");
                        Close();
                    }
                    else
                    {
                        tabControlRegister.SelectedIndex = 1;
                    }
                }
                else
                {
                    Width = 400;
                    Height = 250;
                }
            }
            else
            {
                tabControlRegister.SelectedIndex = 1;
                Title = "회원가입 (일반 사용자)";
                buttonSignUp.Content = "사용자 추가";
            }
        }

        private bool isEMRAleadyExist()
        {
            bool result = false;

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

            if ((regConfig.GetValue("emr_id") != null) && (regConfig.GetValue("api_key") != null))
            {
                ApplicationState.SetValue("emr_id", regConfig.GetValue("emr_id") as string);
                ApplicationState.SetValue("emr_api_key", regConfig.GetValue("api_key") as string);
                result = true;
            }

            return result;
        }

        private bool isManagerAleadyExist()
        {
            bool result = false;

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

            if (regConfig.GetValue("manager") != null)
            {
                result = true;
            }

            return result;
        }

        private void buttonRegisterHospital_Click(object sender, RoutedEventArgs e)
        {
            buttonRegisterHospital.IsEnabled = false;
            buttonRegisterHospital.Content = "정보 확인중";

            if (textBoxEMail.Text.Length == 0)
            {
                MessageBox.Show("이메일은 비워두실 수 없습니다.", "로그인 실패");
                buttonRegisterHospital.IsEnabled = true;
                buttonRegisterHospital.Content = "등록";
                return;
            }
            if (textBoxRegKey.Text.Length == 0)
            {
                MessageBox.Show("등록코드는 비워두실 수 없습니다.", "로그인 실패");
                buttonRegisterHospital.IsEnabled = true;
                buttonRegisterHospital.Content = "등록";
                return;
            }
            
            Dictionary<string, string> dataObject = new Dictionary<string, string>();
            dataObject.Add("email", textBoxEMail.Text);
            dataObject.Add("reg_key", textBoxRegKey.Text);

            var data = JsonConvert.SerializeObject(dataObject);
            var content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpResponseMessage response = client.PutAsync("emr/register", content).Result;
                //response.EnsureSuccessStatusCode();

                string responseText = response.Content.ReadAsStringAsync().Result;
                dynamic jo = JObject.Parse(responseText);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (saveHospitalAccount(responseText))
                    {
                        confirmHospital(responseText);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    switch ((string)jo.code)
                    {
                        case "2001":
                            MessageBox.Show("신청서에 기재된 email과 key를 확인해주세요.");
                            break;
                        case "2002":
                            MessageBox.Show("만료된 key입니다. 새로운 메일을 확인해주세요.");
                            break;
                        default:
                            MessageBox.Show("병원 등록 실패\n" + jo.code + ": " + jo.message);
                            break;
                    }
                    buttonRegisterHospital.IsEnabled = true;
                    buttonRegisterHospital.Content = "등록";
                }
            }
            catch (HttpRequestException he)
            {
                MessageBox.Show("등록 실패\n" + he.Message, "로그인 실패");
                buttonRegisterHospital.IsEnabled = true;
                buttonRegisterHospital.Content = "등록";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n네트워크 연결을 확인해주세요.(05)");
                buttonRegisterHospital.IsEnabled = true;
                buttonRegisterHospital.Content = "등록";
            }
        }
        
        private bool saveHospitalAccount(string json)
        {
            bool result = false;
            var data = (JObject)JsonConvert.DeserializeObject(json);
            try
            {
                ApplicationState.SetValue("emr_id", data["emr_id"].Value<string>());
                ApplicationState.SetValue("emr_api_key", data["api_key"].Value<string>());

                result = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("데이터 저장에 실패하였습니다.\n" + e.Message);
                buttonRegisterHospital.IsEnabled = true;
                buttonRegisterHospital.Content = "등록";
            }
            return result;
        }

        private bool confirmHospital(string json)
        {
            bool result = false;
            Dictionary<string, string> dataObject = new Dictionary<string, string>();
            string emr_id = ApplicationState.GetValue<string>("emr_id");
            string emr_api_key = ApplicationState.GetValue<string>("emr_api_key");

            var data = JsonConvert.SerializeObject(dataObject);
            var content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", emr_id, emr_api_key))));

            try
            {
                HttpResponseMessage response = client.PutAsync("emr/" + emr_id + "/confirm", content).Result;
                //response.EnsureSuccessStatusCode();

                string responseText = response.Content.ReadAsStringAsync().Result;
                dynamic jo = JObject.Parse(responseText);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
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

                    regConfig.SetValue("emr_id", emr_id);
                    regConfig.SetValue("api_key", emr_api_key);

                    this.Height = 440;
                    this.Width = 500;
                    tabControlRegister.SelectedIndex = 1;
                    buttonRegisterHospital.IsEnabled = true;
                    buttonRegisterHospital.Content = "등록";
                }

                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    switch ((string)jo.code)
                    {
                        case "2001":
                            MessageBox.Show("유효하지 않은 id입니다.");
                            break;
                        case "2003":
                            MessageBox.Show("register를 먼저 해주세요..");
                            break;
                        case "2005":
                            MessageBox.Show("유효하지 않은 요청입니다.");
                            break;
                        default:
                            MessageBox.Show("병원 등록 에러\n" + jo.code + ": " + jo.message);
                            break;
                    }
                    buttonRegisterHospital.IsEnabled = true;
                    buttonRegisterHospital.Content = "등록";
                }
            }
            catch (HttpRequestException he)
            {
                MessageBox.Show("등록 실패\n" + he.Message, "로그인 실패");
                buttonRegisterHospital.IsEnabled = true;
                buttonRegisterHospital.Content = "등록";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n네트워크 연결을 확인해주세요.(06)");
                buttonRegisterHospital.IsEnabled = true;
                buttonRegisterHospital.Content = "등록";
            }
            return result;
        }

        private void buttonSignUp_Click(object sender, RoutedEventArgs e)
        {
            if (isEnteredValidValue())
            {
                string user_id = string.Empty;
                string user_api_key = string.Empty;
                string data = "{\"country\":\"KR\",\"msIsdn\":\"" + msIsdn + "\",\"nickName\":\"" + textBoxUserName.Text + 
                    "\",\"profession\":901,\"specialty\":4021,\"smsId\":\"" + smsId + "\",\"email\":\"" + textBoxUserEmail.Text + 
                    "\",\"password\":\"" + passwordBoxPassword.Password + "\"}";
                var content = new StringContent(data, Encoding.UTF8, "application/json");

                string urlPath = string.Empty;
                string auth = string.Empty;
                if (isManagerRegister)
                {
                    urlPath = ApplicationState.GetValue<string>("emr_id") + "/register";
                    auth = string.Format("{0}:{1}", ApplicationState.GetValue<string>("emr_id"), ApplicationState.GetValue<string>("emr_api_key"));
                }
                else
                {
                    urlPath = ApplicationState.GetValue<string>("emr_id") + "/tv_accounts";
                    auth = string.Format("{0}-{1}:{2}",
                                            ApplicationState.GetValue<User>("currentUser").uid,
                                            ApplicationState.GetValue<User>("currentUser").did,
                                            ApplicationState.GetValue<User>("currentUser").sessionKey);
                }

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(auth)));
                try
                {
                    HttpResponseMessage response = client.PostAsync("emr/" + urlPath, content).Result;
                    //response.EnsureSuccessStatusCode();
                    
                    string responseText = response.Content.ReadAsStringAsync().Result;
                    dynamic jo = JObject.Parse(responseText);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var responseObject = (JObject)JsonConvert.DeserializeObject(responseText);
                        user_id = responseObject["user_id"].Value<string>();
                        user_api_key = responseObject["api_key"].Value<string>();

                        RegistryKey regUser = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\" + textBoxUserEmail.Text, true);
                        if (regUser == null)
                        {
                            RegistryKey regHealthBreeze = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze", true);
                            if (regHealthBreeze == null)
                            {
                                RegistryKey regSoftware = Registry.CurrentUser.OpenSubKey("Software", true);
                                regHealthBreeze = regSoftware.CreateSubKey("HealthBreeze");
                            }
                            regUser = regHealthBreeze.CreateSubKey(textBoxUserEmail.Text);
                        }
                        regUser.SetValue("user_id", user_id);
                        regUser.SetValue("api_key", user_api_key);

                        if (isManagerRegister)
                        {
                            RegistryKey regConfig = Registry.CurrentUser.OpenSubKey(@"Software\HealthBreeze\config", true);
                            regConfig.SetValue("manager", true);
                            if (regConfig.GetValue("api_key") != null)
                            {
                                regConfig.DeleteValue("api_key");
                            }
                        }

                        MessageBox.Show(textBoxUserEmail.Text + "계정이 가입되었습니다.\n\n메일함에서 인증한 후 사용하실 수 있습니다.");
                        Close();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        switch ((string)jo.code)
                        {
                            case "2003":
                                if (isManagerRegister)
                                {
                                    MessageBox.Show("이미 관리자가 등록되어있습니다.");
                                }
                                else
                                {
                                    MessageBox.Show("관리자를 먼저 등록해주세요.");
                                }
                                break;
                            case "2011":
                                MessageBox.Show("id 혹은 국가코드를 확인해주세요.");
                                break;
                            case "2012":
                                MessageBox.Show("라이센스를 확인해주세요.");
                                break;
                            case "2013":
                                MessageBox.Show("이미 존재하는 email입니다.");
                                break;
                            case "2014":
                                MessageBox.Show("비밀번호를 확인해주세요.");
                                break;
                            default:
                                MessageBox.Show("회원가입 실패\n" + jo.code + ": " + jo.message);
                                break;
                        }
                        buttonRequest.IsEnabled = true;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show("관리자가 아닙니다.");
                    }
                }
                catch (HttpRequestException he)
                {
                    buttonRequest.IsEnabled = true;
                    MessageBox.Show("회원가입 실패\n" + he.Message, "가입 실패");
                }
                catch (Exception ex)
                {
                    buttonRequest.IsEnabled = true;
                    MessageBox.Show(ex.Message + "\n네트워크 연결을 확인해주세요.(07)");
                }
            }
        }

        private bool isEnteredValidValue()
        {
            bool result = false;

            if (textBoxUserEmail.Text.Length == 0)
            {
                MessageBox.Show("이메일을 입력해주세요.");
            }
            else if (textBoxUserName.Text.Length == 0)
            {
                MessageBox.Show("이름을 입력해주세요.");
            }
            else if (!isPhoneVerify)
            {
                MessageBox.Show("전화번호를 인증해주세요.");
            }
            else if (passwordBoxPassword.Password.Length == 0 || passwordBoxPassword.Password.Length < 8)
            {
                MessageBox.Show("비밀번호는 8자 이상입니다.");
            }
            else if (passwordBoxConfirmPassword.Password.CompareTo(passwordBoxPassword.Password) != 0)
            {
                MessageBox.Show("비밀번호가 서로 다릅니다.");
            }
            else
            {
                result = true;
            }

            return result;
        }

        private void buttonRequest_Click(object sender, RoutedEventArgs e)
        {
            buttonRequest.IsEnabled = false;
            Dictionary<string, object> dataObject = new Dictionary<string, object>();
            dataObject.Add("type", "Pro");
            dataObject.Add("country", "82");
            dataObject.Add("lang", "ko");
            dataObject.Add("mdn", textBoxTelNum.Text);
            dataObject.Add("checkAccount", isPhoneCheck);
            var data = JsonConvert.SerializeObject(dataObject);            
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient();

            string urlPath = string.Empty;
            string auth = string.Empty;
            if (isManagerRegister)
            {
                urlPath = "emr";
                client.BaseAddress = new Uri(URL);
                auth = string.Format("{0}:{1}", ApplicationState.GetValue<string>("emr_id"), ApplicationState.GetValue<string>("emr_api_key"));
            }
            else
            {
                urlPath = "tvapp";
                client.BaseAddress = new Uri(URL2);
                auth = string.Format("{0}-{1}:{2}",
                                        ApplicationState.GetValue<User>("currentUser").uid,
                                        ApplicationState.GetValue<User>("currentUser").did,
                                        ApplicationState.GetValue<User>("currentUser").sessionKey);
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(
                            auth)));

            try
            {
                HttpResponseMessage response = client.PostAsync(urlPath + "/request", content).Result;
                //response.EnsureSuccessStatusCode();
                string responseText = response.Content.ReadAsStringAsync().Result;
                dynamic jo = JObject.Parse(responseText);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    smsId = jo.smsId;
                    msIsdn = jo.msIsdn;
                    textBoxTelToken.IsEnabled = true;
                    buttonVerify.IsEnabled = true;
                    MessageBox.Show("인증번호를 성공적으로 발송하였습니다.");
                    buttonRequest.IsEnabled = true;
                    if (ApplicationState.runAt.CompareTo("local") == 0)
                    {
                        textBoxTelToken.Text = jo.token;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    switch ((string)jo.code)
                    {
                        case "3001":
                            string caption = "전화번호 중복";
                            string message = "이미 존재하는 번화번호입니다.\n\n그래도 진행 하시겠습니까?";
                            MessageBoxButton button = MessageBoxButton.YesNo;
                            MessageBoxImage icon = MessageBoxImage.Warning;
                            MessageBoxResult result = MessageBox.Show(message, caption, button, icon);

                            switch (result)
                            {
                                case MessageBoxResult.Yes:
                                    isPhoneCheck = false;
                                    buttonRequest_Click(sender, e);
                                    break;
                                case MessageBoxResult.No:
                                    textBoxTelNum.Text = string.Empty;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            MessageBox.Show("전화번호 인증 실패\n" + jo.code + ": " + jo.message);
                            break;
                    }
                    buttonRequest.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show(response.StatusCode.ToString());
                }
            }
            catch (HttpRequestException he)
            {
                MessageBox.Show("전화번호 인증 실패\n" + he.Message, "가입 실패");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n네트워크 연결을 확인해주세요.(08)");
                buttonRequest.IsEnabled = true;
            }
        }

        private void buttonVerify_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> dataObject = new Dictionary<string, string>();
            dataObject.Add("smsId", smsId);
            dataObject.Add("msIsdn", msIsdn);
            dataObject.Add("token", textBoxTelToken.Text);
            var data = JsonConvert.SerializeObject(dataObject);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient();

            string urlPath = string.Empty;
            string auth = string.Empty;
            if (isManagerRegister)
            {
                urlPath = "emr";
                client.BaseAddress = new Uri(URL);
                auth = string.Format("{0}:{1}", ApplicationState.GetValue<string>("emr_id"), ApplicationState.GetValue<string>("emr_api_key"));
            }
            else
            {
                urlPath = "tvapp";
                client.BaseAddress = new Uri(URL2);
                auth = string.Format("{0}-{1}:{2}", 
                                        ApplicationState.GetValue<User>("currentUser").uid,
                                        ApplicationState.GetValue<User>("currentUser").did,
                                        ApplicationState.GetValue<User>("currentUser").sessionKey);
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(
                            auth)));

            try
            {
                HttpResponseMessage response = client.PutAsync(urlPath + "/request", content).Result;
                response.EnsureSuccessStatusCode();

                string responseText = response.Content.ReadAsStringAsync().Result;
                var responseObject = (JObject)JsonConvert.DeserializeObject(responseText);
                smsId = responseObject["smsId"].Value<string>();
                msIsdn = responseObject["msIsdn"].Value<string>();
                MessageBox.Show("전화번호 인증을 성공하였습니다.");
                isPhoneVerify = true;
                textBoxTelNum.IsEnabled = false;
                buttonRequest.Visibility = Visibility.Hidden;
                textBoxTelToken.IsEnabled = false;
                buttonVerify.Visibility = Visibility.Hidden;
            }
            catch (HttpRequestException he)
            {
                MessageBox.Show("buttonRequest_Click 실패\n" + he.Message, "가입 실패");
            }
            catch (Exception ex)
            {
                buttonVerify.IsEnabled = true;
                MessageBox.Show(ex.Message + "\n네트워크 연결을 확인해주세요.(09)");
            }
        }
    }
}
