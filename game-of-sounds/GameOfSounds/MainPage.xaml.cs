/* 
 * Main Page for GAME OF SOUNDS
 * author: Skyler Goodell
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GameOfSounds.Resources;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

/*******************************
 * THE GAME OF SOUNDS
 * Main Screen- the happy screen that is always ready to greet you!
 * 
 * author: Skyler Goodell
 *******************************/

namespace GameOfSounds
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
        }
    }
}