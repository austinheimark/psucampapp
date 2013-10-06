using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Media.Imaging;
using psucampapp.Common;
using Bing.Maps.Search;
using Bing.Maps;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232


namespace psucampapp
{

    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class MainPage : LayoutAwarePage
    {
        private JsonObject response;
        private Model model;
        private Random rand = new Random();

        public MainPage()
        {
            this.InitializeComponent();
            model = null;
            pageTitle.Text = "Restaurant near ";
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            String YWSID = "Bi9Fsbfon92vmD4DkkO4Fg";
            String url;
            if (navigationParameter != null && navigationParameter != "")
            {
                model = (Model)navigationParameter;
            }

            if (model != null)
            {
                url = "http://api.yelp.com/business_review_search?term=food&location=" + model.Location + "&ywsid=" + YWSID;
            }
            else 
            {
                Geolocator locator = new Geolocator();
                Geoposition geoPos = await locator.GetGeopositionAsync();
                Geocoordinate geoCoord = geoPos.Coordinate;
                url = "http://api.yelp.com/business_review_search?term=food&lat=" + geoCoord.Latitude + "&long=" + geoCoord.Longitude + "&ywsid=" + YWSID;
            }

            var httpClient = new HttpClient();
            String content = await httpClient.GetStringAsync(url);
            response = JsonObject.Parse(content);

            if (response.GetNamedArray("businesses").Count == 0)
            {
                if (model != null)
                {
                    model.error = true;
                    GoBack(this, null);
                }
                else
                {
                    this.Frame.Navigate(typeof(LocationEntryPage));
                }
            }
            else
            {
                StopLoading();
                PopulateNewRestaurant();
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            // var selectedItem = this.flipView.SelectedItem;
            // TODO: Derive a serializable navigation parameter and assign it to pageState["SelectedItem"]
        }

        private void Diff_Restaurant_Click(object sender, RoutedEventArgs e)
        {
            PopulateNewRestaurant();
        }

        //Shows all the fields that are hidden originally
        void StopLoading()
        {
            LoadingRing.IsActive = false;
            Diff_Restaurant.Visibility = Visibility.Visible;
            NewLocation.Visibility = Visibility.Visible;
            Url.Visibility = Visibility.Visible;
        }

        //Assumes the response array has been properly populated
        async void PopulateNewRestaurant()
        {
            JsonArray businesses = response.GetNamedArray("businesses");
            JsonObject business = businesses.GetObjectAt((uint)rand.Next(0, businesses.Count-1));
            JsonArray categories = business.GetNamedArray("categories");
            String categoriesStr = "";
            for (uint i = 0; i < categories.Count; i++)
            {
                JsonObject category = categories.GetObjectAt(i);
                if (categoriesStr.Length != 0)
                {
                    categoriesStr += ", ";
                }
                categoriesStr += category.GetNamedString("name");
            }

            Name.Text = business.GetNamedString("name");
            Location.Text = business.GetNamedString("address1") + "\n" + business.GetNamedString("city") + ", " + business.GetNamedString("state") + " " + business.GetNamedString("zip");
            
            string number = business.GetNamedString("phone");
            if (number != "")
            {
                Phone.Text = parsePhoneNumber(number);
            }

            Categories.Text = "Cuisine: " + categoriesStr;
            Rating_Image.Source = new BitmapImage(new Uri(business.GetNamedString("rating_img_url")));
            Image.Source = new BitmapImage(new Uri(business.GetNamedString("photo_url")));
            Url.NavigateUri = new Uri(business.GetNamedString("url"));
            bool isClosed = business.GetNamedBoolean("is_closed");
            string city = business.GetNamedString("city");
            if (city != "")
            {
                pageTitle.Text = "Restaurant near " + city;
            }

            // Restaurant is closed
            if (isClosed)
            {
                OpenClosed.Text = Name.Text + " is closed :(";
                OpenClosedImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/x.jpg"));
            }
            else // open
            {
                OpenClosed.Text = Name.Text + " is open! :)";
                OpenClosedImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/checkmark.jpg"));
            }

            Diff_Restaurant.IsEnabled = false;

            try
            {
                GeocodeRequestOptions requestOptions = new GeocodeRequestOptions(business.GetNamedString("address1") + " " + business.GetNamedString("city") + ", " + business.GetNamedString("state") + " " + business.GetNamedString("zip"));
                Bing.Maps.Search.SearchManager searchManager = bingMap.SearchManager;
                Bing.Maps.Search.LocationDataResponse mapResponse = await searchManager.GeocodeAsync(requestOptions);

                Bing.Maps.Location businessLocation = mapResponse.LocationData.First<GeocodeLocation>().Location;
                bingMap.Children.Clear();

                Pushpin pin = new Pushpin();
                pin.Text = Name.Text;
                MapLayer.SetPosition(pin, businessLocation);
                bingMap.Children.Add(pin);

                bingMap.SetZoomLevel(15, MapAnimationDuration.None);
                bingMap.SetView(businessLocation);
            }
            catch (InvalidOperationException exc)
            {
                String message = exc.Message;
            }
            finally
            {
                Diff_Restaurant.IsEnabled = true;
            }

            // Url.Content = business.GetNamedString("url");
        }

        string parsePhoneNumber(string number)
        {
            string firstPart = number.Substring(0, 3);
            string secondPart = number.Substring(3, 3);
            string thirdPart = number.Substring(6);

            string newNumber = firstPart + '-' + secondPart + '-' + thirdPart;

            return newNumber;
            //string newNumb = "";

            //int count = 0;
            //while (count < number.Length)
            //{
            //    if (count == 3 || count == 6)
            //    {
            //        newNumb += '-';
            //    }
            //    else
            //    {
            //        newNumb += number[count];
            //        count++;
            //    }
            //}

            //return newNumb;
        }

        private void New_Location_Click(object sender, RoutedEventArgs e)
        {
            if (model == null)
            {
                this.Frame.Navigate(typeof(LocationEntryPage), false);
            }
            else
            {
                //TODO: Is this a good idea?
                model.error = false;
                GoBack(this, null);
            }
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
               
        }
    }
}
