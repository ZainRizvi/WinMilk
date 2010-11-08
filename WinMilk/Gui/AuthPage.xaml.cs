﻿using System;
using System.Windows;
using IronCow;
using Microsoft.Phone.Controls;

namespace WinMilk.Gui
{
    public partial class AuthPage : PhoneApplicationPage
    {

        #region IsLoading

        /// <summary>
        /// IsLoading Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(AuthPage),
                new PropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the IsLoading property. This dependency property 
        /// indicates whether we are currently loading.
        /// </summary>
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        #endregion

        private string Frob { get; set; }

        public AuthPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            this.IsLoading = true;

            App.RtmClient.GetFrob((string frob) =>
            {
                Frob = frob;
                string url = App.RtmClient.GetAuthenticationUrl(frob, AuthenticationPermissions.Delete);

                Dispatcher.BeginInvoke(() => 
                {
                    this.IsLoading = false;
                    webBrowser1.Navigate(new Uri(url));
                });
            });
        }

        private void AuthDoneButton_Click(object sender, EventArgs e)
        {
            // only do something if Frob is present

            if (!string.IsNullOrEmpty(Frob))
            {
                IsLoading = true;
                App.RtmClient.GetToken(Frob, (string token, User user) =>
                {
                    // create timeline
                    App.RtmClient.GetOrStartTimeline((int timeline) =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            IsLoading = false;

                            if (NavigationService.CanGoBack)
                            {
                                PivotLandingPage.sReload = true;
                                NavigationService.GoBack();
                            }
                        });
                    });
                });
            }
        }

        private void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.IsLoading = false;
        }

        private void webBrowser1_Navigating(object sender, NavigatingEventArgs e)
        {
            this.IsLoading = true;
        }
    }
}