using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using Microsoft.Rest;

namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public sealed class PbiCache
    {
        private const string CACHE_ENTRY_OPTIONS = "_pbiOptions";
        private const string CACHE_ENTRY_TOKEN = "_pbiToken";

        private static readonly Lazy<MemoryCache> lazy = new Lazy<MemoryCache>(() => 
            new MemoryCache(new MemoryCacheOptions()));

        public static IMemoryCache Instance { get { return lazy.Value; } }

        private PbiCache()
        {
        }

        public static PbiOptions GetOptions(DialogContext dc)
        {
            return PbiCache.Instance.GetOrCreate(CACHE_ENTRY_OPTIONS, entry =>
                dc.Parent.State.GetValue<PbiOptions>("settings.pbiOptions"));
        }

        public static async Task<TokenCredentials> GetRestTokenAsync(DialogContext dc, IBotTelemetryClient telemetryClient, CancellationToken cancellationToken)
        {
            IMemoryCache cache = PbiCache.Instance;

            TokenCredentials tokenResponse = await cache.GetOrCreate(CACHE_ENTRY_TOKEN, async entry =>
            {
                telemetryClient.TrackTrace("Creating access token", Severity.Verbose, null);
                await dc.Context.SendActivityAsync(new Activity(type: ActivityTypes.Trace, text: "Creating access token"));

                PbiOptions options = PbiCache.GetOptions(dc);

                var tenantSpecificUrl = options.AuthorityUri.Replace("organizations", options.TenantId);

                IConfidentialClientApplication clientApp = ConfidentialClientApplicationBuilder
                    .Create(options.ClientId)
                    .WithClientSecret(options.ClientSecret)
                    .WithAuthority(tenantSpecificUrl)
                    .Build();

                IEnumerable<string> scopes = new string[] { options.Scope };
                AuthenticationResult authenticationResult = await clientApp.AcquireTokenForClient(scopes).ExecuteAsync();

                telemetryClient.TrackTrace($"Using token {authenticationResult.AccessToken}", Severity.Verbose, null);
                await dc.Context.SendActivityAsync(new Activity(type: ActivityTypes.Trace, text: $"Using token {authenticationResult.AccessToken}"));

                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(50));
                
                return new TokenCredentials(authenticationResult.AccessToken, "Bearer");
            });

            return tokenResponse;
        }
    }
}