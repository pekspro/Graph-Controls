// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using Windows.UI.Xaml;

namespace CommunityToolkit.Graph.Uwp
{
    /// <summary>
    /// A StateTrigger for detecting when the global authentication provider has been signed in.
    /// </summary>
    public class ProviderStateTrigger : StateTriggerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderStateTrigger"/> class.
        /// </summary>
        public ProviderStateTrigger()
        {
            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
        }

        private void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            var isSignedIn = provider?.State == ProviderState.SignedIn;

            SetActive(isSignedIn);
        }
    }
}
