using System;
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



        int width;
        int height;

        #region Constructors
        public TileMapView(int width = 100, int height = 100)
        {
            this.width = width;
            this.height = height;
        }
        #endregion

        public int Width
        {
            get { return width; }
            set { width = value; } // TODO: recalculate tile grid
        }

        public int Height
        {
            get { return height; }
            set { height = value; }  // TODO: recalculate tile grid
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
            updateMapPosition(geocoordinate);
        }
    }
}
