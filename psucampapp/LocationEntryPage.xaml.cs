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
using psucampapp.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace psucampapp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LocationEntryPage : psucampapp.Common.LayoutAwarePage
    {
        Model model;

        public LocationEntryPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            model = null;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if ((model != null && model.error) || (bool)e.Parameter == true)
            {
                errorLabel.Text = "Error: There were no results for your query.";
            }
            else
            {
                errorLabel.Text = "";
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            model = new Model();
            model.Location = Location.Text;
            this.Frame.Navigate(typeof(MainPage), model);
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        //private void Entry_Field_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    TextBox temp = (TextBox) sender;
        //    temp.Text = "";
        //}

        private void Entry_Field_Tapped(object sender, RoutedEventArgs e)
        {
            TextBox temp = (TextBox)sender;
            temp.Text = string.Empty;
            temp.GotFocus -= Entry_Field_Tapped;
        }
    }
}
