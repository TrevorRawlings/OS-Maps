using System;
using System.Text;
using System.IO.IsolatedStorage;
using System.Windows;
using Windows.Devices.Geolocation;

// From: http://msdn.microsoft.com/en-us/library/bb259689.aspx

namespace PhoneApp2
{
    public delegate void StatusChangedEventHandler(string value);
    
    public class MapControl
    {
        public event StatusChangedEventHandler StatusChanged;
        private string status;
        private TileMapView mapView;

        public MapControl(TileMapView mapView)
        {
            this.mapView = mapView;
        }

        public string Status
        {
            get { return status; }
        }

        private void setStatus(string newValue) {
            if (newValue != status) {
                status = newValue;
                if (StatusChanged != null) {
                    StatusChanged(newValue);
                }
            }
        }

        public void CheckForLocationConsent()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                // User has opted in or out of Location
                return;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("This app accesses your phone's location. Is that ok?", "Location", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }
     
        public async void navigateToCurrentGeoposition()
        {
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                setStatus("The user has opted out of Location.");                
                return;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            setStatus("Getting location...");

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                mapView.CenterOnLocation(geoposition.Coordinate);               
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    setStatus("location  is disabled in phone settings."); // the application does not have the right capability or the location master switch is off                    
                }
                //else
                {
                    // something else happened acquring the location
                }
            }

            setStatus("Loading image..."); 

        }
    }
}
