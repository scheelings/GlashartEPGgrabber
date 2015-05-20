using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlashartLibrary.Settings;
using log4net;

namespace GlashartLibrary.IO
{
    public class CachedWebDownloader : IWebDownloader
    {
        private readonly IWebDownloader _webDownloader;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CachedWebDownloader));
        private const string Filename = "HttpCache.dat";

        private readonly string _file;
        private readonly int _daysToCache;
        private readonly List<CacheObject> _cache = new List<CacheObject>(); 

        public CachedWebDownloader(ISettings settings, IWebDownloader webDownloader)
        {
            _webDownloader = webDownloader;
            _file = Path.Combine(settings.DataFolder, Filename);
            _daysToCache = settings.EpgArchiving;
        }

        public byte[] DownloadBinary(string url)
        {
            var data = GetFromCache(url);
            if (data != null) return data.ByteData;
            var webData = _webDownloader.DownloadBinary(url);
            if (webData != null) _cache.Add(new CacheObject(url, webData));
            return webData;
        }

        public string DownloadString(string url)
        {
            var data = GetFromCache(url);
            if (data != null) return data.StringData;
            var webData = _webDownloader.DownloadString(url);
            if (webData != null) AddToCache(new CacheObject(url, webData));
            return webData;
        }

        private void AddToCache(CacheObject obj)
        {
            Logger.DebugFormat("Add {0} data to cache for {1}", obj.DataType, obj.Url);
            _cache.Add(obj);
        }

        private CacheObject GetFromCache(string url)
        {
            var cacheObj = _cache.FirstOrDefault(c => c.Url == url);
            if (cacheObj != null)
            {
                Logger.DebugFormat("Load from cache for {0}", url);
            }
            return cacheObj;
        }

        public void LoadCache()
        {
            try
            {
                if (!File.Exists(_file))
                {
                    Logger.WarnFormat("HTTP Cache file {0} doesn't exist", _file);
                    return;
                }
                Logger.DebugFormat("Load {0} cache into memory", _file);
                using (var reader = new BinaryReader(File.OpenRead(_file)))
                {
                    var count = reader.ReadInt32();
                    Logger.DebugFormat("Read {0} objects into cache", count);
                    for(var i = 0; i < count; i ++)
                    {
                        var obj = new CacheObject();
                        obj.Deserialize(reader);
                        _cache.Add(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load http cache from {0}", _file);
            }
        }

        public void SaveCache()
        {
            try
            {
                var data = _cache;
                if (_daysToCache > 0)
                {
                    Logger.DebugFormat("Filter the current cache to {0} days", _daysToCache);
                    data = _cache.Where(c => c.Date.AddDays(_daysToCache) >= DateTime.Now).ToList();
                }
                Logger.DebugFormat("Save cache to {0}", _file);
                using (var writer = new BinaryWriter(File.OpenWrite(_file)))
                {
                    Logger.DebugFormat("Write {0} objects to cache file", data.Count);
                    writer.Write(data.Count);
                    data.ForEach(d => d.Serialize(writer));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save http cache to {0}", _file);
            }
        }
    }
}
