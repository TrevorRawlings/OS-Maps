using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneApp2.Resources;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.Collections.Specialized;


namespace PhoneApp2
{
    public partial class MainPage : PhoneApplicationPage
    {
        private MapControl mapControl;
        private TileMapView mapView;

        // Constructor
        public MainPage()
        {
            InitializeComponent();


            mapView = new TileMapView(MapCanvas);
            mapControl = new MapControl(mapView);
            mapControl.StatusChanged += new StatusChangedEventHandler(OnMapStatusChanged);
            mapView.MapPositionChanged += new MapPositionChangedEventHandler(OnMapPositionChanged);
            mapView.TileImages.CollectionChanged += new NotifyCollectionChangedEventHandler(OnTileImagesChanged);
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
        
            mapControl.CheckForLocationConsent();
        }

        private void OnMapStatusChanged(string value)
        {
            StatusTextBlock.Text = value;
        }

        private void OnMapPositionChanged(LatLongCoordinate geocoordinate)
        {
            LatitudeTextBlock.Text = geocoordinate.Latitude.ToString("0.00");
            LongitudeTextBlock.Text = geocoordinate.Longitude.ToString("0.00");
        }

        private void OnTileImagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        private void OneShotLocation_Click(object sender, RoutedEventArgs e)
        {
            mapControl.navigateToCurrentGeoposition();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}