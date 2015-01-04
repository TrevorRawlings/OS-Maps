using System;
using System.Diagnostics;
using System.Text;
using Windows.Devices.Geolocation;



// From: http://msdn.microsoft.com/en-us/library/bb259689.aspx

namespace PhoneApp2
{


    public class LatLongCoordinate
    {
        private double latitude, longitude;

        public LatLongCoordinate(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public LatLongCoordinate(Geocoordinate geocoordinate)
        {
            this.latitude = geocoordinate.Latitude;
            this.longitude = geocoordinate.Longitude;
        }

        public double Latitude
        {
            get { return latitude; }
        }

        public double Longitude
        {
            get { return longitude; }
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            LatLongCoordinate c = (LatLongCoordinate)obj;
            return (latitude == c.latitude) && (longitude == c.longitude);
        }
        public override int GetHashCode()
        {
            return latitude.GetHashCode() ^ longitude.GetHashCode();
        }
    }
}
