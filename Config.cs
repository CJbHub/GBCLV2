﻿namespace GBCLV2
{
    using System.IO;
    using LitJson;
    using System;
    using System.Text;
    using System.Security.Cryptography;

    public class Config
    {
        const string encryptKey = "无可奉告";
        private static Config data;

        [JsonPropertyName("JavaPath")]              public string   _JavaPath;
        [JsonPropertyName("VersionIndex")]          public int      _VersionIndex;
        [JsonPropertyName("MaxMemory")]             public uint     _MaxMemory;
        [JsonPropertyName("Offline")]               public bool     _Offline;
        [JsonPropertyName("UserName")]              public string   _UserName;
        [JsonPropertyName("Email")]                 public string   _Email;
        [JsonPropertyName("PassWord")]              public string   _PassWord;
        [JsonPropertyName("RememberPassWord")]      public bool     _RememberPassWord;
        [JsonPropertyName("WinWidth")]              public ushort   _WinWidth;
        [JsonPropertyName("WinHeight")]             public ushort   _WinHeight;
        [JsonPropertyName("FullScreen")]            public bool     _FullScreen;
        [JsonPropertyName("ServerAddress")]         public string   _ServerAddress;
        [JsonPropertyName("AdvancedArgs")]          public string   _AdvancedArgs;
        [JsonPropertyName("ThemeColor")]            public string   _ThemeColor;
        [JsonPropertyName("UseSystemThemeColor")]   public bool     _UseSystemThemeColor;
        [JsonPropertyName("UseImageBackground")]    public bool     _UseImageBackground;
        [JsonPropertyName("ImagePath")]             public string   _ImagePath;

        #region 属性访问器

        public static string JavaPath
        {
            get => data._JavaPath; set => data._JavaPath = value;
        }

        public static int VersionIndex
        {
            get => data._VersionIndex; set => data._VersionIndex = value;
        }

        public static uint MaxMemory
        {
            get => data._MaxMemory; set => data._MaxMemory = value;
        }

        public static bool Offline
        {
            get => data._Offline; set => data._Offline = value;
        }

        public static string UserName
        {
            get => data._UserName; set => data._UserName = value;
        }

        public static string Email
        {
            get => data._Email; set => data._Email = value;
        }

        public static string PassWord
        {
            get => data._PassWord; set => data._PassWord = value;
        }

        public static bool RememberPassWord
        {
            get => data._RememberPassWord; set => data._RememberPassWord = value;
        }

        public static ushort WinWidth
        {
            get => data._WinWidth; set => data._WinWidth = value;
        }

        public static ushort WinHeight
        {
            get => data._WinHeight; set => data._WinHeight = value;
        }

        public static bool FullScreen
        {
            get => data._FullScreen; set => data._FullScreen = value;
        }

        public static string AdvancedArgs
        {
            get => data._AdvancedArgs; set => data._AdvancedArgs = value;
        }

        public static string ServerAddress
        {
            get => data._ServerAddress; set => data._ServerAddress = value;
        }

        public static string ThemeColor
        {
            get => data._ThemeColor; set => data._ThemeColor = value;
        }

        public static bool UseSystemThemeColor
        {
            get => data._UseSystemThemeColor; set => data._UseSystemThemeColor = value;
        }

        public static bool UseImageBackground
        {
            get => data._UseImageBackground; set => data._UseImageBackground = value;
        }

        public static string ImagePath
        {
            get => data._ImagePath; set => data._ImagePath = value;
        }

        #endregion

        static Config()
        {
            Load();
        }

        public static void Save()
        {
            if(data._RememberPassWord)
            {
                data._PassWord = Encrypt(data._PassWord);
            }
            else
            {
                data._PassWord = null;
            }
                
            File.WriteAllText("GBCL.json", JsonMapper.ToJson(data));
        }

        private static void Load()
        {
            try
            {
                data = JsonMapper.ToObject<Config>(File.ReadAllText("GBCL.json"));
                data._PassWord = Decrypt(data._PassWord);
            }
            catch
            {
                data = new Config()
                {
                    _MaxMemory = 2048,
                    _WinWidth = 854,
                    _WinHeight = 480,
                };
            }
        }

        private static string Encrypt(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();

            byte[] key = Encoding.Unicode.GetBytes(encryptKey);
            byte[] data = Encoding.Default.GetBytes(str);

            using (var ms = new MemoryStream())
            {
                CryptoStream cs = new CryptoStream(ms, descsp.CreateEncryptor(key, key), CryptoStreamMode.Write);
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private static string Decrypt(string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return null;
            }

            DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();

            byte[] key = Encoding.Unicode.GetBytes(encryptKey);
            byte[] data = Convert.FromBase64String(str);

            using (var ms = new MemoryStream())
            {
                CryptoStream cs = new CryptoStream(ms, descsp.CreateDecryptor(key, key), CryptoStreamMode.Write);
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return Encoding.Default.GetString(ms.ToArray());
            }
        }
    }
}
