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
//using Windows.Networking.BackgroundTransfer;
using System.Net;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;
 


// From: http://msdn.microsoft.com/en-us/library/bb259689.aspx

namespace PhoneApp2
{
    public enum Problem { Ok, Cancelled, Other };

    public delegate void TileImageDownloadedEventHandler(TileImage source);

    public class TileImage : IDisposable
    {
        private String quadKey;
        private TileCache cache;
        private System.Windows.Controls.Image imageControl;
        private CancellationTokenSource cancelDownload;
        private int tileX, tileY, levelOfDetail;
        private int pixelX, pixelY;
        private Rectangle tileRectangle;

        public event TileImageDownloadedEventHandler TileImageDownloaded;

        public TileImage(TileCache cache, string quadKey)
        {
            this.cache = cache;
            this.quadKey = quadKey;
            TileSystem.QuadKeyToTileXY(quadKey, out tileX, out tileY, out levelOfDetail);
            TileSystem.TileXYToPixelXY(tileX, tileY, out pixelX, out pixelY);

            tileRectangle = new Rectangle(pixelX, pixelY, cache.TileSize, cache.TileSize);  
        }

        public string QuadKey
        {
            get { return quadKey; }
        }

        public int TileX
        {
            get { return tileX; }
        }

        public int TileY
        {
            get { return tileY; }
        }

        public int mapPixelX
        {
            get { return pixelX; }
        }

        public int mapPixelY
        {
            get { return pixelY; }
        }

        public TileCache Cache
        {
            get { return cache; }
        }

        public bool IsDownloading
        {
            get { return (cancelDownload != null); }
        }

        public bool IsVisible(Rectangle canvas)
        {
            return canvas.Contains(tileRectangle);
        }

        public void CancelDownload()
        {
            if (cancelDownload != null)
            {
                cancelDownload.Cancel();
            }
        }

        public System.Windows.Controls.Image ImageControl
        {
            get { return imageControl; }
        }

        public void setControl(System.Windows.Controls.Image control) {
            if (imageControl != null) throw new Exception("imageControl is already set");
            if (disposed) throw new ObjectDisposedException("TileImage");

            imageControl = control;
            if (fileExists())
            {
                imageControl.Source = this.AsBitmapImage();
            }
        }

        public async Task<Problem> DownloadIfRequired()
        {
            if (IsDownloading || fileExists()) return Problem.Ok;
            return await DownloadFile();
        }
     
        // http://stackoverflow.com/questions/21572276/downloading-and-saving-a-file-async-in-windows-phone-8/21572678#21572678
        public async Task<Problem> DownloadFile()
        {
            if (cancelDownload != null) throw new Exception("already downloading");
            if (fileExists()) throw new Exception("already downloaded");
            if (disposed) throw new ObjectDisposedException("TileImage");

            cancelDownload = new CancellationTokenSource();
            try
            {
                using (IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (Stream stream = await DownloadHelper.CreateStream(fileUrl()))
                    using (IsolatedStorageFileStream file = ISF.CreateFile(this.fileName()))
                    {
                        await DownloadHelper.SaveStream(stream, file, cancelDownload.Token);
                    }

                }

                if (imageControl != null)
                {
                    imageControl.Source = this.AsBitmapImage();
                }
                return Problem.Ok;
            }
            catch (Exception exc)
            {
                if (exc is OperationCanceledException)
                    return Problem.Cancelled;
                else return Problem.Other;
            }
            finally
            {
                cancelDownload = null;
            }
        }

        private bool fileExists() {
            using (IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return ISF.FileExists(this.fileName());
            }
        }

        public string fileName() {
            return quadKey + ".png";
        }

        public Uri fileUrl()
        {
            return new Uri("http://ak.t1.tiles.virtualearth.net/tiles/r" + quadKey + ".png?g=2540&productSet=mmOS");
        }

        public BitmapImage AsBitmapImage() {
            byte[] data; 

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication()) 
            { 
                // Open the file - error handling omitted for brevity 
                // Note: If the image does not exist in isolated storage the following exception will be generated: 
                // System.IO.IsolatedStorage.IsolatedStorageException was unhandled  
                // Message=Operation not permitted on IsolatedStorageFileStream  
                using (IsolatedStorageFileStream isfs = isf.OpenFile(fileName(), FileMode.Open, FileAccess.Read)) 
                { 
                    data = new byte[isfs.Length]; 
                    isfs.Read(data, 0, data.Length); 
                    isfs.Close(); 
                } 
            } 

            MemoryStream ms = new MemoryStream(data); 
            BitmapImage bi = new BitmapImage();
            bi.SetSource(ms);
            return bi;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if(disposed) return;
            CancelDownload();
            TileImageDownloaded = null;
            disposed = true;
        }
    }

    public class TileImageList : ObservableCollection<TileImage>
    {
        public event TileImageDownloadedEventHandler TileImageDownloaded;

        //public TileImageList()
        //{
        //    this.CollectionChanged += tileImageList_CollectionChanged;
        //}

        //private void tileImageList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
            
        //    if (e.Action == NotifyCollectionChangedAction.Remove || 
        //        e.Action == NotifyCollectionChangedAction.Replace || 
        //        e.Action == NotifyCollectionChangedAction.Reset)
        //    {
        //        foreach (TileImage tile in e.OldItems)
        //        {
        //            tile.TileImageDownloaded -= title_ImageDownloaded;
        //        }
        //    }

        //    if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
        //    {
        //        foreach (TileImage tile in e.NewItems)
        //        {
        //            tile.TileImageDownloaded += title_ImageDownloaded;
        //        }
        //    }
        //}

        //private void title_ImageDownloaded(TileImage source)
        //{
        //    if (TileImageDownloaded != null)
        //    {
        //        TileImageDownloaded(source);
        //    }
        //}

        public TileImage FindByQuadKey(string quadKey)
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

        public uint TileSize
        {
            get { return TileSystem.TileSize(levelOfDetail); }
        }

        public TileImage GetTile(string quadKey)
        {
            return new TileImage(this, quadKey);            
        }
    }
}
