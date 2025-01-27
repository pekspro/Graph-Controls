﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpMsalProviderSample
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Sets the global IProvider instance for authentication using the official MSAL library.
        /// </summary>
        void ConfigureGlobalProvider()
        {
            DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                if (ProviderManager.Instance.GlobalProvider == null)
                {
                    string clientId = "YOUR-CLIENT-ID-HERE";
                    string[] scopes = new string[] { "User.Read" };
                    ProviderManager.Instance.GlobalProvider = new MsalProvider(clientId, scopes);
                }
            });
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();

                ConfigureGlobalProvider();
            }
        }
    }
}
