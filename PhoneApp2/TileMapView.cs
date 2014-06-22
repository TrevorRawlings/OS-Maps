using System;
using System.Collections.ObjectModel;
using System.Text;
using Windows.Devices.Geolocation;

// From: http://msdn.microsoft.com/en-us/library/bb259689.aspx

namespace PhoneApp2
{
    public delegate void MapPositionChangedEventHandler(Geocoordinate geocoordinate);

    
    

    public class TileMapView
    {

        public event MapPositionChangedEventHandler MapPositionChanged;
        
        private Geocoordinate mapPosition;
        private TileCache tileCache;

        public TileImageList TileImages;

        int canvasWidth;
        int canvasHeight;

        int levelOfDetail = 15;
        
        
        int pixelX, pixelY, tileX, tileY;

        #region Constructors
        public TileMapView(int width = 100, int height = 100)
        {
            this.canvasWidth = width;
            this.canvasHeight = height;
            this.tileCache = new TileCache();
            this.TileImages = new TileImageList();
        }
        #endregion

        public int Width
        {
            get { return canvasWidth; }
            set { canvasWidth = value; } // TODO: recalculate tile grid
        }

        public int Height
        {
            get { return canvasHeight; }
            set { canvasHeight = value; }  // TODO: recalculate tile grid
        }

        public Geocoordinate MapPosition
        {
            get { return mapPosition; }
        }

        private void updateMapPosition(Geocoordinate newValue)
        {
            if (newValue != mapPosition)
            {
                mapPosition = newValue;
                if (MapPositionChanged != null)
                {
                    MapPositionChanged(newValue);
                }
            }
        }
        
        public void CenterOnLocation(Geocoordinate geocoordinate)
        {
            TileImage tileImage;

            updateMapPosition(geocoordinate);

            TileSystem.LatLongToPixelXY(geocoordinate.Latitude, geocoordinate.Longitude, tileCache.LevelOfDetail, out pixelX, out pixelY);
            TileSystem.PixelXYToTileXY(pixelX, pixelY, out tileX, out tileY);
            string quadKey = TileSystem.TileXYToQuadKey(tileX, tileY, tileCache.LevelOfDetail);

            tileImage = TileImages.findByQuadKey(quadKey);
            if (tileImage == null) {
                TileImages.Add(tileCache.GetFile(quadKey));
            }
        }


    }
}
