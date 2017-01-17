using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishAngler.CachedImageLoader.Interfaces
{
    public interface ICacheFileManager
    {
        byte[] GetFileBytes(String path);

        void WriteFileBytes(string fileName, byte[] stream);

        TModel Deserialize<TModel>(String fileName) where TModel : class;

        void Serialize<TModel>(string fileName, TModel model) where TModel : class;

        bool FileExists(string fileName);

        List<string> GetCachedFilesPriorTo(DateTime dateTime);

        void RemoveFile(string file);

        List<string> GetAllCachedFiles();
    }
}
