using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishAngler.CachedImageLoader.Interfaces
{
    public interface IBasicFileManager
    {
        string GetTempDirectory(String directoryName);

        string CombinePaths(params string[] args);

        bool FileExists(string fullFileName);

        string ReadAllText(string fullFileName);

        byte[] ReadAllBytes(string fullFileName);

        void WriteAllText(string fullFileName, string text);

        void WriteAllBytes(string fullFileName, byte[] bytes);

        List<string> GetAllFileNames(string path);

        void RemoveFile(string fullFileName);
    }
}
