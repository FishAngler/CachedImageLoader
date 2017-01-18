using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FishAngler.CachedImageLoader.Interfaces;

namespace FishAngler.CachedImageLoader.Droid
{
    public class BasicFileManager : IBasicFileManager
    {
        public string CombinePaths(params string[] args)
        {
            return Path.Combine(args);
        }

        public bool FileExists(string fullFileName)
        {
            return File.Exists(fullFileName);
        }

        public List<string> GetAllFileNames(string path)
        {
            return System.IO.Directory.GetFiles(path).ToList();
        }

        public string GetTempDirectory(String directoryName)
        {
            var cacheDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), directoryName);

            var created = false;
            var attempts = 0;
            while (attempts < 3 && !created)
            {
                if (!System.IO.Directory.Exists(cacheDirectory))
                {
                    System.IO.Directory.CreateDirectory(cacheDirectory);
                }

                created = System.IO.Directory.Exists(cacheDirectory);
            }

            if (!created)
            {
                /* Should never happen but well if it does throw an exception to track the core problem */
                throw new Exception("Could not create temporary directory at: " + cacheDirectory);
            }

            return cacheDirectory;
        }

        public byte[] ReadAllBytes(string fullFileName)
        {
            return File.ReadAllBytes(fullFileName);
        }

        public string ReadAllText(string fullFileName)
        {
            return File.ReadAllText(fullFileName);
        }

        public void RemoveFile(string fullFileName)
        {
            File.Delete(fullFileName);
        }

        public void WriteAllBytes(string fullFileName, byte[] bytes)
        {
            File.WriteAllBytes(fullFileName, bytes);
        }

        public void WriteAllText(string fullFileName, string text)
        {
            File.WriteAllText(fullFileName, text);
        }
    }
}