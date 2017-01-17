using FishAngler.CachedImageLoader.Models;
using FishAngler.Shared.Models.Imaging;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace FishAngler.CachedImageLoader.Interfaces
{
    public interface ICacheManager
    {
        void Init(String path, Usage.CacheSettings settings, ICacheFileManager fileManager);

        void AddCachedFile(RemoteMedia media, byte[] imageBytes);

        bool HasCachedFile(RemoteMedia media);

        byte[] GetCachedFile(RemoteMedia media);

        double EstimatedCacheSizeMB { get; }
        int CachedFileCount { get; }

        void Clear();

        void Prune(int? days = null);

        ObservableCollection<CacheEventTraceMessage> TraceMessages { get; }

        void AddTraceMessage(CacheEventTraceMessage message);

        bool TraceEnabled { get; set; }
    }

    public class CacheEventTraceMessage
    {
        public enum EventTypes
        {
            Trace,
            Warning,
            Error
        }

        public EventTypes EventType { get; private set; }
        
        public DateTime DateStamp { get; private set; }
        public String Message { get; set; }

        public TimeSpan Duration { get; set; }

        public static CacheEventTraceMessage Create(EventTypes eventType, string message, TimeSpan duration)
        {
            var traceMessage = new CacheEventTraceMessage()
            {
                EventType = eventType,
                DateStamp = DateTime.Now,
                Message = message,
                Duration = duration
            };

            return traceMessage;
        }

        public static CacheEventTraceMessage Create(string message, TimeSpan? timeSpan = null)
        {
            return CacheEventTraceMessage.Create(EventTypes.Trace, message, timeSpan.HasValue ? timeSpan.Value : TimeSpan.Zero);
        }

        public static CacheEventTraceMessage CreateError(string message, TimeSpan? timeSpan = null)
        {
            return CacheEventTraceMessage.Create(EventTypes.Error, message, timeSpan.HasValue ? timeSpan.Value : TimeSpan.Zero);
        }

        public static CacheEventTraceMessage CreateWarning(string message, TimeSpan? timeSpan = null)
        {
            return CacheEventTraceMessage.Create(EventTypes.Warning, message, timeSpan.HasValue ? timeSpan.Value : TimeSpan.Zero);
        }

        public static CacheEventTraceMessage CreateError(Exception ex, String message, TimeSpan? timeSpan = null)
        {
            return CacheEventTraceMessage.Create(EventTypes.Warning, message + " " + ex.Message, timeSpan.HasValue ? timeSpan.Value : TimeSpan.Zero);
        }
    }
}
