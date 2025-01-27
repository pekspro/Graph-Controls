// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;
using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;

namespace SampleTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2407
        public DateTime Today => DateTimeOffset.Now.Date.ToUniversalTime();

        // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2407
        public DateTime ThreeDaysFromNow => Today.AddDays(3);

        public IBaseRequestBuilder CalendarViewBuilder;
        public IBaseRequestBuilder MessagesBuilder;
        public IBaseRequestBuilder PlannerTasksBuilder;
        public IBaseRequestBuilder TeamsChannelMessagesBuilder;

        public MainPage()
        {
            this.InitializeComponent();

            ProviderManager.Instance.ProviderUpdated += this.OnProviderUpdated;
            ProviderManager.Instance.ProviderStateChanged += this.OnProviderStateChanged;
        }

        private void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            if (e.NewState == ProviderState.SignedIn)
            {
                var graphClient = ProviderManager.Instance.GlobalProvider.GetClient();

                CalendarViewBuilder = graphClient.Me.CalendarView;
                MessagesBuilder = graphClient.Me.Messages;
                PlannerTasksBuilder = graphClient.Me.Planner.Tasks;
                TeamsChannelMessagesBuilder = graphClient.Teams["02bd9fd6-8f93-4758-87c3-1fb73740a315"].Channels["19:d0bba23c2fc8413991125a43a54cc30e@thread.skype"].Messages;
            }
            else
            {
                ClearRequestBuilders();
            }
        }

        private void OnProviderUpdated(object sender, IProvider provider)
        {
            if (provider == null)
            {
                ClearRequestBuilders();
            }
        }

        private void ClearRequestBuilders()
        {
            CalendarViewBuilder = null;
            MessagesBuilder = null;
            PlannerTasksBuilder = null;
            TeamsChannelMessagesBuilder = null;
        }

        public static string ToLocalTime(DateTimeTimeZone value)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2407
            return value.ToDateTimeOffset().LocalDateTime.ToString("g");
        }

        public static string ToLocalTime(DateTimeOffset? value)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2654
            return value?.LocalDateTime.ToString("g");
        }

        public static string RemoveWhitespace(string value)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2654
            return Regex.Replace(value, @"\t|\r|\n", " ");
        }

        public static bool IsTaskCompleted(int? percentCompleted)
        {
            return percentCompleted == 100;
        }
    }
}
