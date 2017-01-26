using System.Collections.Generic;
using System.IO;

namespace WpfHealthBreezeTV
{
    public static class ApplicationState
    {
        private static Dictionary<string, object> _values = new Dictionary<string, object>();
        public static string filePath = Directory.GetCurrentDirectory() + @"\download\";
        public static string serverUrl = "https://hbreeze4ani.appspot.com/";
        public static string testUrl = "https://test-dot-hbreeze4ani.appspot.com/";
        public static string localUrl1 = "http://10.11.12.100:8081/"; //로컬서버
        public static string localUrl2 = "http://10.11.12.100:8082/"; //로컬서버
        public static string runAt = "server"; //server, test, local
        public static string appVersion = "1.1.3";

        public static void SetValue(string key, object value)
        {
            if (_values.ContainsKey(key))
            {
                _values.Remove(key);
            }
            _values.Add(key, value);
        }

        public static T GetValue<T>(string key)
        {
            if (_values.ContainsKey(key))
            {
                return (T)_values[key];
            }
            else
            {
                return default(T);
            }
        }
    }
}
