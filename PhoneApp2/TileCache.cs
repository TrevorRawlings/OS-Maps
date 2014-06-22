using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using System.IO.IsolatedStorage;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;
using System.Net;


// From: http://msdn.microsoft.com/en-us/library/bb259689.aspx

namespace PhoneApp2
{
    enum Problem { Ok, Cancelled, Other }; 

    public class TileImage
    {
        private String quadKey;
        private TileCache cache;

        

        public TileImage(TileCache cache, string quadKey)
        {
            this.cache = cache;
            this.quadKey = quadKey;
        }

        public String QuadKey
        {
            get { return quadKey; }
        }

        public TileCache Cache
        {
            get { return cache; }
        }

        private static Task<Stream> DownloadFile(Uri url)
        {
            var tcs = new TaskCompletionSource<Stream>();
            var wc = new WebClient();
            wc.OpenReadCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };
            wc.OpenReadAsync(url);
            return tcs.Task;
        }

        // http://stackoverflow.com/questions/21572276/downloading-and-saving-a-file-async-in-windows-phone-8/21572678#21572678
        public async Task<Problem> downloadFileIfMissing(CancellationToken cToken)
        {
            try
            {                
                using (IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (ISF.FileExists(this.fileName())) return Problem.Ok;

                    using (Stream mystr = await DownloadFile(fileUrl()))
                    using (IsolatedStorageFileStream file = ISF.CreateFile(this.fileName()))
                        {
                            const int BUFFER_SIZE = 1024;
                            byte[] buf = new byte[BUFFER_SIZE];

                            int bytesread = 0;
                            while ((bytesread = await mystr.ReadAsync(buf, 0, BUFFER_SIZE)) > 0)
                            {
                                cToken.ThrowIfCancellationRequested();
                                file.Write(buf, 0, bytesread);
                            }
                        }
                    
                }
                return Problem.Ok;
            }
            catch (Exception exc)
            {
                if (exc is OperationCanceledException)
                    return Problem.Cancelled;
                else return Problem.Other;
            }




        }

        private bool fileExists() {
            return false;
        }

        public string fileName() {
            return quadKey + ".png";
        }

        public Uri fileUrl()
        {
            return new Uri("http://ak.t1.tiles.virtualearth.net/tiles/r" + quadKey + ".png?g=2540&productSet=mmOS");
        }
    }

    public class TileImageList : ObservableCollection<TileImage>
    {

        public TileImage findByQuadKey(string quadKey)
        {
            foreach (TileImage image in this)
            {
                if (image.QuadKey == quadKey)
                {
                    return image;
                }
            }
            return null;
        }
    }


    public class TileCache
    {
        private int levelOfDetail = 15;
        public WebClient WebClient;

        public TileCache()
        {
            WebClient = new WebClient();
        }

        public int LevelOfDetail
        {
            get { return levelOfDetail; }
        }

        public TileImage GetFile(string quadKey)
        {
            TileImage image = new TileImage(this, quadKey);
            


            Debug.WriteLine("QuadKey: " + quadKey);

            return image;
        }
    }
}
