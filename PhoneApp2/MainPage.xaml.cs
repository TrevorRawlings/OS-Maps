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
using System.Diagnostics;
using System.Windows.Media;


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

        private void StackPanel_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            
            e.ManipulationContainer = this;
            e.Handled = true;
        }

        private void StackPanel_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            

         //   TransformGroup transformGroup = (TransformGroup) MapCanvas.RenderTransform;

            // Get the Rectangle and its RenderTransform matrix.
         //   Rectangle rectToMove = e.OriginalSource as Rectangle;
          //  Matrix rectsMatrix = ((MatrixTransform)rectToMove.RenderTransform).Matrix;

          

            // Resize the Rectangle.  Keep it square 
            // so use only the X value of Scale.
          //  rectsMatrix.ScaleAt(e.DeltaManipulation.Scale.X,
          //                      e.DeltaManipulation.Scale.X,
          //                      e.ManipulationOrigin.X,
          //                      e.ManipulationOrigin.Y);

            // Move the Rectangle.
            TranslateTransform translateTransform = new TranslateTransform();
            translateTransform.X = e.DeltaManipulation.Translation.X;
            translateTransform.Y = e.DeltaManipulation.Translation.Y;

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(MapCanvas.RenderTransform);
            transformGroup.Children.Add(translateTransform);

            // Apply the changes to the Rectangle.
            MapCanvas.RenderTransform = transformGroup;




            // Check if the rectangle is completely in the window.
            // If it is not and intertia is occuring, stop the manipulation.
  //          if (e.IsInertial && !containingRect.Contains(shapeBounds))
//            {
  //              e.Complete();
    //        }


            e.Handled = true;
        }

        private void StackPanel_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (MapCanvas.RenderTransform is TransformGroup) {
                mapView.ApplyTransformMatrix(((TransformGroup) MapCanvas.RenderTransform).Value);
            }
            
         
        }

        private void canvasContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RectangleGeometry clipGeometry = new RectangleGeometry();
            clipGeometry.Rect = new System.Windows.Rect(0, 0, canvasContainer.ActualWidth, canvasContainer.ActualHeight);
            canvasContainer.Clip = clipGeometry;
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