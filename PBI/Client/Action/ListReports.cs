using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public class ListReports : Dialog
    {
        private readonly string urlPowerBiServiceApiRoot = "https://api.powerbi.com";
        private PowerBIClient _pbiClient;

        [JsonProperty("$kind")]
        public const string Kind = "ListReports";

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        [JsonConstructor]
        public ListReports([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                PbiOptions pbiOptions = PbiCache.GetOptions(dc);

                Guid workspaceId = new Guid(pbiOptions.WorkspaceId);

                TokenCredentials tokenCredentials = await PbiCache.GetRestTokenAsync(dc, TelemetryClient, cancellationToken);
                _pbiClient = new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);

                Reports reports = DoListReports(workspaceId);

                if (this.ResultProperty != null)
                {
                    dc.State.SetValue(this.ResultProperty.GetValue(dc.State), reports);
                }

                return await dc.EndDialogAsync(result: true, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                TelemetryClient.TrackException(ex);
                return await dc.EndDialogAsync(result: false, cancellationToken: cancellationToken);
            }
        }

        private Reports DoListReports(Guid workspaceId)
        {
            return _pbiClient.Reports.GetReportsInGroup(workspaceId);
        }
    }
}
