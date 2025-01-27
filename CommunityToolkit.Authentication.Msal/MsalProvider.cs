// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// <a href="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet">MSAL.NET</a> provider helper for tracking authentication state.
    /// </summary>
    public class MsalProvider : BaseProvider
    {
        /// <inheritdoc />
        public override string CurrentAccountId => _account?.HomeAccountId?.Identifier;

        /// <summary>
        /// Gets the MSAL.NET Client used to authenticate the user.
        /// </summary>
        protected IPublicClientApplication Client { get; private set; }

        /// <summary>
        /// Gets an array of scopes to use for accessing Graph resources.
        /// </summary>
        protected string[] Scopes { get; private set; }

        private IAccount _account;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalProvider"/> class.
        /// </summary>
        /// <param name="clientId">Registered ClientId.</param>
        /// <param name="redirectUri">RedirectUri for auth response.</param>
        /// <param name="scopes">List of Scopes to initially request.</param>
        /// <param name="autoSignIn">Determines whether the provider attempts to silently log in upon instantionation.</param>
        public MsalProvider(string clientId, string[] scopes = null, string redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient", bool autoSignIn = true)
        {
            var client = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                .WithRedirectUri(redirectUri)
                .WithClientName(ProviderManager.ClientName)
                .WithClientVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString())
                .Build();

            Scopes = scopes ?? new string[] { string.Empty };

            Client = client;

            if (autoSignIn)
            {
                _ = TrySilentSignInAsync();
            }
        }

        /// <inheritdoc/>
        public override async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            string token = await GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <inheritdoc/>
        public override async Task<bool> TrySilentSignInAsync()
        {
            if (_account != null && State == ProviderState.SignedIn)
            {
                return true;
            }

            State = ProviderState.Loading;

            var token = await GetTokenAsync(true);
            if (token == null)
            {
                await SignOutAsync();
                return false;
            }

            State = ProviderState.SignedIn;
            return true;
        }

        /// <inheritdoc/>
        public override async Task SignInAsync()
        {
            if (_account != null || State != ProviderState.SignedOut)
            {
                return;
            }

            State = ProviderState.Loading;

            var token = await GetTokenAsync();
            if (token == null)
            {
                await SignOutAsync();
            }
            else
            {
                State = ProviderState.SignedIn;
            }
        }

        /// <inheritdoc />
        public override async Task SignOutAsync()
        {
            if (_account != null)
            {
                await Client.RemoveAsync(_account);
                _account = null;
            }

            State = ProviderState.SignedOut;
        }

        /// <inheritdoc/>
        public override async Task<string> GetTokenAsync(bool silentOnly = false, string[] scopes = null)
        {
            var tokenScopes = scopes ?? Scopes;

            AuthenticationResult authResult = null;
            try
            {
                var account = _account ?? (await Client.GetAccountsAsync()).FirstOrDefault();
                if (account != null)
                {
                    authResult = await Client.AcquireTokenSilent(tokenScopes, account).ExecuteAsync();
                }
            }
            catch (MsalUiRequiredException)
            {
            }
            catch
            {
                // Unexpected exception
                // TODO: Send exception to a logger.
            }

            if (authResult == null && !silentOnly)
            {
                try
                {
                    authResult = await Client.AcquireTokenInteractive(tokenScopes).WithPrompt(Prompt.SelectAccount).ExecuteAsync();
                }
                catch
                {
                    // Unexpected exception
                    // TODO: Send exception to a logger.
                }
            }

            _account = authResult?.Account;

            return authResult?.AccessToken;
        }
    }
}
