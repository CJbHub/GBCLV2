﻿using KMCCC.Launcher;
using LitJson;
using System.Collections.Generic;
using System.IO;

namespace GBCLV2.Modules
{
    interface IDownloadBaseUrl
    {
        string VersionListUrl { get; }
        string VersionBaseUrl { get; }
        string LibraryBaseUrl { get; }
        string MavenBaseUrl { get; }
        string JsonBaseUrl { get; }
        string AssetsBaseUrl { get; }
        string ForgeBaseUrl { get; }
    }

    class BMCLAPIBaseUrl : IDownloadBaseUrl
    {
        public string VersionListUrl { get; } = "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json";
        public string VersionBaseUrl { get; } = "https://bmclapi2.bangbang93.com/";
        public string LibraryBaseUrl { get; } = "https://bmclapi2.bangbang93.com/libraries/";
        public string MavenBaseUrl { get; } = "https://bmclapi2.bangbang93.com/maven/";
        public string JsonBaseUrl { get; } = "https://bmclapi2.bangbang93.com/";
        public string AssetsBaseUrl { get; } = "https://bmclapi2.bangbang93.com/assets/";
        public string ForgeBaseUrl { get; } = "https://bmclapi2.bangbang93.com/maven/net/minecraftforge/forge/";
    }

    class OfficialBaseUrl : IDownloadBaseUrl
    {
        public string VersionListUrl { get; } = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public string VersionBaseUrl { get; } = "https://launcher.mojang.com/";
        public string LibraryBaseUrl { get; } = "https://libraries.minecraft.net/";
        public string MavenBaseUrl { get; } = "https://files.minecraftforge.net/maven/";
        public string JsonBaseUrl { get; } = "https://launchermeta.mojang.com/";
        public string AssetsBaseUrl { get; } = "https://resources.download.minecraft.net/";
        public string ForgeBaseUrl { get; } = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/";
    }

    public class DownloadInfo
    {
        public string Path { get; set; }
        public string Url { get; set; }
    }

    static class DownloadHelper
    {
        public static IDownloadBaseUrl BaseUrl;

        public static void SetDownloadSource(int DownloadSource)
        {
            switch (DownloadSource)
            {
                case 0:
                    BaseUrl = new OfficialBaseUrl();
                    break;

                case 1:
                    BaseUrl = new BMCLAPIBaseUrl();
                    break;
            }
        }

        public static List<DownloadInfo> GetLostEssentials(LauncherCore core, Version version)
        {
            var lostEssentials = new List<DownloadInfo>();

            var JarPath = $"{core.GameRootPath}\\versions\\{version.JarID}\\{version.JarID}.jar";
            if (!File.Exists(JarPath))
            {
                lostEssentials.Add(new DownloadInfo
                {
                    Path = JarPath,
                    Url = BaseUrl.VersionBaseUrl + version.JarUrl
                });
            }

            foreach (var lib in version.Libraries)
            {
                var absolutePath = $"{core.GameRootPath}\\libraries\\{lib.Path}";
                if (!File.Exists(absolutePath))
                {
                    lostEssentials.Add(new DownloadInfo
                    {
                        Path = absolutePath,
                        Url = (lib.IsForgeLib) ? BaseUrl.MavenBaseUrl + lib.Path : BaseUrl.LibraryBaseUrl + lib.Path
                    });
                }
            }

            foreach (var native in version.Natives)
            {
                var absolutePath = $"{core.GameRootPath}\\libraries\\{native.Path}";
                if (!File.Exists(absolutePath))
                {
                    lostEssentials.Add(new DownloadInfo
                    {
                        Path = absolutePath,
                        Url = BaseUrl.LibraryBaseUrl + native.Path,
                    });
                }
            }
            return lostEssentials;
        }

        public static List<DownloadInfo> GetLostAssets(LauncherCore core, Version version)
        {
            var lostAssets = new List<DownloadInfo>();

            var indexPath = $"{core.GameRootPath}\\assets\\indexes\\{version.Assets}.json";
            string indexJson;

            if (!File.Exists(indexPath))
            {
                try
                {
                    string indexUrl;
                    if (version.AssetsIndexUrl != null)
                    {
                        indexUrl = BaseUrl.JsonBaseUrl + version.AssetsIndexUrl;
                    }
                    else
                    {
                        indexUrl = $"{BaseUrl.JsonBaseUrl}indexs/{version.Assets}.json";
                    }

                    var client = new System.Net.Http.HttpClient() { Timeout = new System.TimeSpan(0, 0, 5) };
                    indexJson = client.GetStringAsync(indexUrl).Result;
                    client.Dispose();
                }
                catch
                {
                    System.Windows.MessageBox.Show("获取资源列表失败!");
                    return lostAssets;
                }

                if (!Directory.Exists(Path.GetDirectoryName(indexPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(indexPath));
                }
                File.WriteAllText(indexPath, indexJson);
            }
            else
            {
                indexJson = File.ReadAllText(indexPath);
            }

            var assets = JsonMapper.ToObject(indexJson)["objects"];

            for (int i = 0; i < assets.Count; i++)
            {
                var hash = assets[i][0].ToString();
                var relativePath = $"{hash.Substring(0, 2)}\\{hash}";
                var absolutePath = $"{core.GameRootPath}\\assets\\objects\\{relativePath}";

                if (!File.Exists(absolutePath))
                {
                    lostAssets.Add(new DownloadInfo
                    {
                        Path = absolutePath,
                        Url = BaseUrl.AssetsBaseUrl + relativePath
                    });
                }
            }
            return lostAssets;
        }
    }
}
