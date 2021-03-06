﻿using BILIBILI_UWP.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace bilibili2
{
    class ApiHelper
    {
        //九幽反馈
        public const string JyAppkey = @"1afd8ae4b933daa51a39573a5719bba5";
        public const string JySecret = @"d9e7262e70801e795c18dc20e0972df6";

        public const string _appSecret_Wp = "ba3a4e554e9a6e15dc4d1d70c2b154e3";//Wp
        public const string _appSecret_IOS = "8cb98205e9b2ad3669aad0fce12a4c13";//Ios
        public const string _appSecret_Android = "ea85624dfcf12d7cc7b2b3a94fac1f2c";//Android
        public const string _appSecret_DONTNOT = "2ad42749773c441109bdc0191257a664";//Android

        public const string _appKey = "422fd9d7289a1dd9";//Wp
        public const string _appKey_IOS = "4ebafd7c4951b366";
        public const string _appKey_Android = "c1b107428d337928";
        public const string _appkey_DONTNOT = "85eb6835b0a1034e";//e5b8ba95cab6104100be35739304c23a
        //85eb6835b0a1034e,2ad42749773c441109bdc0191257a664
        public static string access_key = string.Empty;

        public static string GetSign(string url)
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(_appSecret_Wp);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }
        public static string GetSign_Android(string url)
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(_appSecret_Android);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }
        public static string GetSign_Ios(string url)
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(_appSecret_IOS);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }
        public static string GetSign_DN(string url)
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(_appSecret_DONTNOT);
            result = MD5.GetMd5String(stringBuilder.ToString()).ToLower();
            return result;
        }
        public static long GetTimeSpen
        {
            get { return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds); }
        }

        public static async Task<string> GetEncryptedPassword(string passWord)
        {
            string base64String;
            try
            {
                //https://secure.bilibili.com/login?act=getkey&rnd=4928
                //https://passport.bilibili.com/login?act=getkey&rnd=4928
                HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient(httpBaseProtocolFilter);
                //WebClientClass wc = new WebClientClass();
                string stringAsync = await httpClient.GetStringAsync((new Uri("https://secure.bilibili.com/login?act=getkey&rnd=" + new Random().Next(1000, 9999), UriKind.Absolute)));
                JObject jObjects = JObject.Parse(stringAsync);
                string str = jObjects["hash"].ToString();
                string str1 = jObjects["key"].ToString();
                string str2 = string.Concat(str, passWord);
                string str3 = Regex.Match(str1, "BEGIN PUBLIC KEY-----(?<key>[\\s\\S]+)-----END PUBLIC KEY").Groups["key"].Value.Trim();
                byte[] numArray = Convert.FromBase64String(str3);
                AsymmetricKeyAlgorithmProvider asymmetricKeyAlgorithmProvider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
                CryptographicKey cryptographicKey = asymmetricKeyAlgorithmProvider.ImportPublicKey(WindowsRuntimeBufferExtensions.AsBuffer(numArray), 0);
                IBuffer buffer = CryptographicEngine.Encrypt(cryptographicKey, WindowsRuntimeBufferExtensions.AsBuffer(Encoding.UTF8.GetBytes(str2)), null);
                base64String = Convert.ToBase64String(WindowsRuntimeBufferExtensions.ToArray(buffer));
            }
            catch (Exception)
            {
                //throw;
                base64String = passWord;
            }
            return base64String;
        }

        public static async Task<string> LoginBilibili(string UserName, string Password)
        {
            try
            {
                //发送第一次请求，得到access_key
                WebClientClass wc = new WebClientClass();
                string url = "https://api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=" + WebUtility.UrlEncode(Password) + "&type=json&userid=" + WebUtility.UrlEncode(UserName);


                string results = await wc.GetResults(new Uri(url));
                //Json解析及数据判断
                LoginModel model = new LoginModel();
                model = JsonConvert.DeserializeObject<LoginModel>(results);
                if (model.code == -627)
                {
                    return "登录失败，密码错误！";
                }
                if (model.code == -626)
                {
                    return "登录失败，账号不存在！";
                }
                if (model.code == -625)
                {
                    return "密码错误多次";
                }
                if (model.code == -628)
                {
                    return "未知错误";
                }
                if (model.code == -1)
                {
                    return "登录失败，程序注册失败！请联系作者！";
                }
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                if (model.code == 0)
                {
                    access_key = model.access_key;
                    Windows.Web.Http.HttpResponseMessage hr2 = await hc.GetAsync(new Uri("http://api.bilibili.com/login/sso?&access_key=" + model.access_key + "&appkey=422fd9d7289a1dd9&platform=wp"));
                    hr2.EnsureSuccessStatusCode();
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    StorageFile file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
                    await FileIO.WriteTextAsync(file, model.access_key);
                }
                //看看存不存在Cookie
                HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
                HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));

                List<string> ls = new List<string>();
                foreach (HttpCookie item in cookieCollection)
                {
                    ls.Add(item.Name);
                }
                if (ls.Contains("DedeUserID"))
                {
                    return "登录成功";
                }
                else
                {
                    return "登录失败";
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867)
                {
                    return "登录失败，检查你的网络连接！";
                }
                else
                {
                    return "登录发生错误";
                }
                
            }

        }

        public static string GetUserId()
        {
            HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
            HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            Dictionary<string, string> ls = new Dictionary<string, string>();
            string a = string.Empty;
            foreach (HttpCookie item in cookieCollection)
            {
                ls.Add(item.Name, item.Value);
                if (item.Name == "DedeUserID")
                {
                    a = item.Value;
                }
            }
            return a;

        }


    }
}
