using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.Devices.Geolocation;

// From: http://msdn.microsoft.com/en-us/library/bb259689.aspx

namespace PhoneApp2
{
    public delegate void MapPositionChangedEventHandler(LatLongCoordinate geocoordinate);

    
    

    public class TileMapView
    {

        public event MapPositionChangedEventHandler MapPositionChanged;

        private Canvas canvas;
        private LatLongCoordinate mapCenter;
        private TileCache tileCache;

        public TileImageList TileImages;


        //private CancellationTokenSource cancelDownload;
        uint visibleTilesX;
        uint visibleTilesY;
        int levelOfDetail = 15;
        
        #region Constructors
        public TileMapView(Canvas canvas)
        {
            this.canvas = canvas;
            this.canvas.SizeChanged += new System.Windows.SizeChangedEventHandler(OnCanvasResized);

            this.tileCache = new TileCache();
            this.TileImages = new TileImageList();

            OnCanvasResized(null, null);
        }
        #endregion

        
        private void OnCanvasResized(Object sender, System.Windows.SizeChangedEventArgs e)
        {
            uint newVisibleTilesX = 1;
            uint newVisibleTilesY = 1;
            uint tileSize = TileSystem.TileSize(tileCache.LevelOfDetail);

            if (canvas.ActualHeight > 0)
            {
                newVisibleTilesY = (uint) Math.Ceiling(canvas.ActualHeight / tileSize);
            }

            if (canvas.ActualWidth > 0)
            {
                newVisibleTilesX = (uint) Math.Ceiling(canvas.ActualWidth / tileSize);
            }

            if ((newVisibleTilesX != visibleTilesX) || (newVisibleTilesY != visibleTilesY))
            {
                visibleTilesX = newVisibleTilesX;
                visibleTilesY = newVisibleTilesY;
                updateTiles();
            }

            RectangleGeometry clipGeometry = new RectangleGeometry();
            clipGeometry.Rect = new System.Windows.Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight);
            canvas.Clip = clipGeometry;
        }

        public LatLongCoordinate MapCenter
        {
            get { return mapCenter; }
        }

        public LatLongCoordinate MapTopLeft
        {
            get
            {
                int pixelX, pixelY;
                double latitude, longitude;

                if (mapCenter == null)
                {
                    return null;
                }

                TileSystem.LatLongToPixelXY(mapCenter.Latitude, mapCenter.Longitude, tileCache.LevelOfDetail, out pixelX, out pixelY);
                pixelY -= (int) (canvas.ActualHeight / 2); // TODO should handle under & overflows
                pixelX -= (int) (canvas.ActualWidth / 2);

                TileSystem.PixelXYToLatLong(pixelX, pixelY, tileCache.LevelOfDetail, out latitude, out longitude);
                return new LatLongCoordinate(latitude, longitude);
            }
        }

        public Rectangle VisibleMapRegion
        {
            get
            {
                int pixelX, pixelY;
                LatLongCoordinate mapTopLeft = MapTopLeft;

                if (mapTopLeft == null)
                {
                    return null;
                }

                TileSystem.LatLongToPixelXY(mapTopLeft.Latitude, mapTopLeft.Longitude, tileCache.LevelOfDetail, out pixelX, out pixelY);
                return new Rectangle(pixelX, pixelY, (uint) canvas.ActualWidth, (uint) canvas.ActualHeight);
            }
        }

        private void updateMapCenter(LatLongCoordinate newValue)
        {
            if (newValue != mapCenter)
            {
                mapCenter = newValue;
                if (MapPositionChanged != null)
                {
                    MapPositionChanged(newValue);
                }
            }
        }

        private void updateTiles()
        {
            //System.Collections.Generic.List<Task<Problem>> tasks = new System.Collections.Generic.List<Task<Problem>>();
            Rectangle visibleMapRegion = VisibleMapRegion;

            if (visibleMapRegion == null)
            {
                return;
            }
    
            foreach (TileImage tile in TileImages)
            {
                if (!tile.IsVisible(visibleMapRegion))
                {
                    if (tile.ImageControl != null)
                    {
                        canvas.Children.Remove(tile.ImageControl);
                    }
                    tile.Dispose();
                }
            }
            //await Task.WhenAll(tasks.ToArray());

            string topLeftQuadKey = TileSystem.PixelXYToQuadKey(visibleMapRegion.X, visibleMapRegion.Y, tileCache.LevelOfDetail);
            TileImage topLeftTile = tileCache.GetTile(topLeftQuadKey);

            // int mapPixelX = topLeftTile.mapPixelX;            
            // while (mapPixelX <= (visibleMapRegion.X + visibleMapRegion.Width))

            for (int mapPixelX = topLeftTile.mapPixelX; mapPixelX <= (visibleMapRegion.X + visibleMapRegion.Width); mapPixelX += (int) tileCache.TileSize)
            {
                for (int mapPixelY = topLeftTile.mapPixelY; mapPixelY <= (visibleMapRegion.Y + visibleMapRegion.Height); mapPixelY += (int) tileCache.TileSize)
                {
                    string quadKey = TileSystem.PixelXYToQuadKey(mapPixelX, mapPixelY, tileCache.LevelOfDetail);
                    TileImage tile = TileImages.FindByQuadKey(quadKey);
                    if (tile == null)
                    {
                        tile = tileCache.GetTile(quadKey);
                        tile.setControl(new Image());
                        canvas.Children.Add(tile.ImageControl);
                        TileImages.Add(tile);
                    }
                    tile.DownloadIfRequired().ConfigureAwait(false);

                    int canvasPixelX, canvasPixelY;
                    mapPixelsToCanvasPixels(mapPixelX, mapPixelY, visibleMapRegion, topLeftTile, out canvasPixelX, out canvasPixelY);
                    Canvas.SetLeft(tile.ImageControl, canvasPixelX);
                    Canvas.SetTop(tile.ImageControl, canvasPixelY);
                    
                    //mapPixelY += (int) tileCache.TileSize;
                }
                // mapPixelX += (int) tileCache.TileSize;
            }
        }

        //public static async void ForgetTask(this Task task)
        //{
        //    await task.ConfigureAwait(false);
            
        //}


        //     ____________________
        //    | topLeftTile    ____|____
        //    |               |visibleMapRegion
        //    |               |    | 
        //    |               |    |         X mapPixel
        //    |               |    |
        //
        // CanvasPixel 0,0 is in the top left of the canvas
        //
        private void mapPixelsToCanvasPixels(int mapPixelX, int mapPixelY, Rectangle visibleMapRegion, TileImage topLeftTile, out int canvasPixelX, out int canvasPixelY)
        {
            // int mapOffsetX = visibleMapRegion.X - topLeftTile.mapPixelX;
            // int mapOffsetY = visibleMapRegion.Y - topLeftTile.mapPixelY;

            canvasPixelX = mapPixelX - visibleMapRegion.X;
            canvasPixelY = mapPixelY - visibleMapRegion.Y;
        }

        public void CenterOnLocation(LatLongCoordinate geocoordinate)
        {//
           // TileImage tileImage;
            //int pixelX, pixelY, tileX, tileY;

            updateMapCenter(geocoordinate);

            //TileSystem.LatLongToPixelXY(geocoordinate.Latitude, geocoordinate.Longitude, tileCache.LevelOfDetail, out pixelX, out pixelY);
            //TileSystem.PixelXYToTileXY(pixelX, pixelY, out tileX, out tileY);
            //string quadKey = TileSystem.TileXYToQuadKey(tileX, tileY, tileCache.LevelOfDetail);

            
            updateTiles();
        }


    }
}
