using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace GlashartLibrary.IO
{
    public class CachedWebDownloader : IWebDownloader
    {
        private readonly IWebDownloader _webDownloader;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CachedWebDownloader));
        private const string Filename = "HttpCache.dat";

        private readonly string _file;
        private readonly Dictionary<string, string> _jsonCache = new Dictionary<string, string>();
        private readonly Dictionary<string, byte[]> _dataCache = new Dictionary<string, byte[]>();

        public CachedWebDownloader(string folder, IWebDownloader webDownloader)
        {
            _webDownloader = webDownloader;
            _file = Path.Combine(folder, Filename);
        }

        public byte[] DownloadBinary(string url)
        {
            var data = GetFromDataCache(url);
            if (data != null) return data;
            data = _webDownloader.DownloadBinary(url);
            if (data != null) AddToDataCache(url, data);
            return data;
        }

        public string DownloadString(string url)
        {
            var data = GetFromJsonCache(url);
            if (data != null) return data;
            data = _webDownloader.DownloadString(url);
            if (data != null) AddToStringCache(url, data);
            return data;
        }

        private string GetFromJsonCache(string url)
        {
            if (!_jsonCache.ContainsKey(url)) return null;
            Logger.DebugFormat("Load http string from cache for {0}", url);
            return _jsonCache[url];
        }

        private void AddToStringCache(string url, string data)
        {
            if (_dataCache.ContainsKey(url))
            {
                Logger.DebugFormat("Update json cache for {0}", url);
                _jsonCache[url] = data;
            }
            else
            {
                Logger.DebugFormat("Save http json to cache for {0}", url);
                _jsonCache.Add(url, data);
            }
        }

        private byte[] GetFromDataCache(string url)
        {
            if (!_dataCache.ContainsKey(url)) return null;
            Logger.DebugFormat("Load http data from cache for {0}", url);
            return _dataCache[url];
        }

        private void AddToDataCache(string url, byte[] data)
        {
            if (_dataCache.ContainsKey(url))
            {
                Logger.DebugFormat("Update data cache for {0}", url);
                _dataCache[url] = data;
            }
            else
            {
                Logger.DebugFormat("Save http data to cache for {0}", url);
                _dataCache.Add(url, data);
            }
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
                    Logger.DebugFormat("Read {0} json objects into cache", count);
                    for(long i = 0; i < count; i ++)
                    {
                        var key = reader.ReadString();
                        var value = reader.ReadString();
                        _jsonCache.Add(key, value);
                    }
                    count = reader.ReadInt32();
                    Logger.DebugFormat("Read {0} data objects into cache", count);
                    for (long i = 0; i < count; i++)
                    {
                        var key = reader.ReadString();
                        var bytes = reader.ReadInt32();
                        var value = reader.ReadBytes(bytes);
                        _dataCache.Add(key, value);
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
                Logger.DebugFormat("Save cache to {0}", _file);
                using (var writer = new BinaryWriter(File.OpenWrite(_file)))
                {
                    writer.Write(_jsonCache.Count);
                    Logger.DebugFormat("Write {0} json objects to cache", _jsonCache.Count);
                    foreach (var kv in _jsonCache)
                    {
                        writer.Write(kv.Key);
                        writer.Write(kv.Value);
                    }
                    writer.Write(_dataCache.Count);
                    Logger.DebugFormat("Write {0} data objects to cache", _dataCache.Count);
                    foreach (var kv in _dataCache)
                    {
                        writer.Write(kv.Key);
                        writer.Write(kv.Value.Length);
                        writer.Write(kv.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load http cache from {0}", _file);
            }
        }
    }
}
