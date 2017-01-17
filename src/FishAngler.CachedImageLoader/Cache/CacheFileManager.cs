using FishAngler.CachedImageLoader.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishAngler.CachedImageLoader.Cache
{
    public class CacheFileManager : ICacheFileManager
    {
        IBasicFileManager _basicFileManager;

        public CacheFileManager(IBasicFileManager basicFileManager)
        {
            _basicFileManager = basicFileManager;
        }

        private String GetCacheDirectory()
        {
            return _basicFileManager.GetTempDirectory("imageCache");
        }

        private String MapTempFileName(string fileName)
        {
            return _basicFileManager.CombinePaths(GetCacheDirectory(), fileName);
        }

        public TModel Deserialize<TModel>(string fileName) where TModel : class
        {
            if (!_basicFileManager.FileExists(MapTempFileName(fileName)))
            {
                return null;
            }

            var json = _basicFileManager.ReadAllText(MapTempFileName(fileName));
            if (String.IsNullOrEmpty(json))
                return null;

            var model = JsonConvert.DeserializeObject<TModel>(json);
            return model;
        }

        public bool FileExists(string fileName)
        {
            return _basicFileManager.FileExists(MapTempFileName(fileName));
        }

        public byte[] GetFileBytes(string fileName)
        {
            return _basicFileManager.ReadAllBytes(MapTempFileName(fileName));
        }

        public void Serialize<TModel>(string fileName, TModel model) where TModel : class
        {
            lock (this)
            {
                var json = JsonConvert.SerializeObject(model);
                var outputFileName = MapTempFileName(fileName);
                _basicFileManager.WriteAllText(outputFileName, json);
            }
        }

        public void WriteFileBytes(string fileName, byte[] bytes)
        {
            _basicFileManager.WriteAllBytes(MapTempFileName(fileName), bytes);
        }

        public List<string> GetCachedFilesPriorTo(DateTime dateTime)
        {
            var files = _basicFileManager.GetAllFileNames(GetCacheDirectory());

            var dateMarker = String.Format("cache_{0:0000}{1:00}{2:00}", dateTime.Year, dateTime.Month, dateTime.Day);
            return files.Where(fileName => fileName.Length > dateMarker.Length && fileName.Substring(0, dateMarker.Length).CompareTo(dateMarker) == -1).ToList();
        }

        public List<string> GetAllCachedFiles()
        {
            return _basicFileManager.GetAllFileNames(GetCacheDirectory()).Where(file => file.EndsWith("img")).ToList();
        }

        public void RemoveFile(string fileName)
        {
            _basicFileManager.RemoveFile(fileName);
        }
    }
}
