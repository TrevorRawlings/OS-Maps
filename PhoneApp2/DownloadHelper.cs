using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

// From: http://msdn.microsoft.com/en-us/library/bb259689.aspx

namespace PhoneApp2
{
    static class DownloadHelper
    {
        public static Task<Stream> CreateStream(Uri url)
        {
            var tcs = new TaskCompletionSource<Stream>();
            var wc = new WebClient();

            Debug.WriteLine("Downloading: " + url + "...");
            wc.OpenReadCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };
            wc.OpenReadAsync(url);
            return tcs.Task;
        }


        public static async Task SaveStream(Stream mystr, IsolatedStorageFileStream file, CancellationToken cancelDownloadToken)
        {

            const int BUFFER_SIZE = 1024;
            byte[] buf = new byte[BUFFER_SIZE];

            int bytesread = 0;
            while ((bytesread = await mystr.ReadAsync(buf, 0, BUFFER_SIZE)) > 0)
            {
                cancelDownloadToken.ThrowIfCancellationRequested();
                file.Write(buf, 0, bytesread);
            }
        }
     
    }
}
