using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishAngler.CachedImageLoader.Models
{
    public class RequestBytesResult
    {
        public RequestBytesResult(ResponseStatus status, bool fromCache, byte[] buffer = null, string errorMessage = null)
        {
            Buffer = buffer;
            Status = status;
            FromCache = fromCache;
            ErrorMessage = errorMessage;
        }

        public enum ResponseStatus
        {
            Failed,
            Cancelled,
            Success
        }

        public bool FromCache { get; private set; }

        public ResponseStatus Status { get; private set; }

        public Byte[] Buffer {get; private set;}

        public String ErrorMessage { get; set; }
    }
}
