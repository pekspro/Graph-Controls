// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using CommunityToolkit.Authentication;
using Windows.UI.Xaml;

namespace CommunityToolkit.Graph.Uwp
{
    /// <summary>
    /// A StateTrigger for detecting when the global authentication provider has been signed in.
    /// </summary>
    public class ProviderStateTrigger : StateTriggerBase
    {
        private ProviderState _state;

        /// <summary>
        /// Gets or sets the expected ProviderState.
        /// </summary>
        public ProviderState State
        {
            get => _state;
            set
            {
                // Doesn't fire when initially set in XAML.
                if (_state != value)
                {
                    _state = value;
                    UpdateState();
                }
            }
        }

        public ProviderStateTrigger()
        {
            // Doesn't fire when constructed in XAML.
            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
        }

        private void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            SetActive(provider?.State == State);
        }
    }
}