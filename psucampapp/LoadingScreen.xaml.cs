using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Data.Json;
using System.Net.Http;
using System.Threading.Tasks;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace psucampapp
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class BasicPage1 : psucampapp.Common.LayoutAwarePage
    {
        private String location;
        private JsonObject response;
        private Random rand = new Random();

        public BasicPage1()
        {
            this.InitializeComponent();
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

            Geolocator locator = new Geolocator();
            Geoposition geoPos = await locator.GetGeopositionAsync();
            Geocoordinate geoCoord = geoPos.Coordinate;

            String YWSID = "Bi9Fsbfon92vmD4DkkO4Fg";
            String url;

            if (navigationParameter != "")
            {
                url = "http://api.yelp.com/business_review_search?term=food&location=" + navigationParameter + "&ywsid=" + YWSID;
            }
            else
            {
                url = "http://api.yelp.com/business_review_search?term=food&lat=" + geoCoord.Latitude + "&long=" + geoCoord.Longitude + "&ywsid=" + YWSID;
            }
            var httpClient = new HttpClient();
            String content = await httpClient.GetStringAsync(url);
            response = JsonObject.Parse(content);

            this.Frame.Navigate(typeof(MainPage),response);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        String getYelpV2URL()
        {
            Int32 unixTimestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + 14400;

            String oauth_key = "YqNAMJO6NeIZXNxubgPxVQ";
            String oauth_token = "rb0Tlb96TjmXAZBfvUML_0lkODiCvIyp";
            String oauth_token_secret = "seGKBoCP7S_oftgcKsbYPzh5CAk";
            String oauth_signature_method = "HMAC-SHA1";
            return "http://api.yelp.com/v2/search?location=" + location + "&oauth_consumer_key=" + oauth_key + "&oauth_nonce=psuapphack" + rand.Next(1, 100) + "&oauth_signature_method=" + oauth_signature_method + "&oauth_timestamp=" + unixTimestamp + "&oauth_token=" + oauth_token + "&term=food&oauth_signature=" + oauth_token_secret;
        }

    }
}
