using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using log4net;

namespace GlashartLibrary.Helpers
{
    public class CachedHttpDownloader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CachedHttpDownloader));

        private readonly string _file;
        private Dictionary<string, HttpCacheObject> _cache;

        public CachedHttpDownloader(string file)
        {
            _file = file;
        }

        public byte[] DownloadBinary(string url)
        {
            var data = GetFromCache<byte[]>(url);
            if (data != null) return data;
            var webClient = new WebClient();
            data = webClient.DownloadData(url);
            AddToCache(url, data);

        }

        private void AddToCache<T>(string url, T data)
        {
            var obj = new HttpCacheObject {Url = url, Data = data};
            _cache.Add(url, obj);
        }

        private T GetFromCache<T>(string url) where T : class
        {
            if (!_cache.ContainsKey(url)) return null;
            var data = _cache[url].Data as T;
            if (data == null) return null;
            Logger.DebugFormat("Resolved data for url {0} from cache", url);
            return data;
        }

        public void LoadCache()
        {
            try
            {
                using (var reader = new BinaryReader(File.OpenRead(_file)))
                {

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load http cache from {0}", _file);
                _cache = new Dictionary<string, HttpCacheObject>();
            }
        }

        public void SaveCache()
        {
            try
            {
                using (var writer = new BinaryWriter(File.OpenWrite(_file)))
                {
                    foreach (var kv in _cache)
                    {
                        writer.Write(kv.Key);
                        writer.Write(kv.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save http cache to {0}", _file);
            }
        }
    }

    [Serializable]
    public class HttpCacheBytes : HttpCacheObject
    {
        public string Url { get; set; }
        public object Data { get; set; }
    }

    [Serializable]
    public class HttpCacheObject
    {
        public string Url { get; set; }
        public object Data { get; set; }
    }
}
