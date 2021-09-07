using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public class GetQna : Dialog
    {
        private readonly string urlPowerBiServiceApiRoot = "https://api.powerbi.com";
        private PowerBIClient _pbiClient;

        [JsonProperty("$kind")]
        public const string Kind = "GetQna";

        [JsonProperty("datasetId")]
        public StringExpression DatasetId { get; set; }

        [JsonProperty("query")]
        public StringExpression Query { get; set; }

        [JsonConstructor]
        public GetQna([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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

                string datasetId = DatasetId.GetValue(dc.State);

                string query = Query.GetValue(dc.State);

                TokenCredentials tokenCredentials = await PbiCache.GetRestTokenAsync(dc, TelemetryClient, cancellationToken);
                _pbiClient = new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);

                GenerateTokenRequest tokenRequest = new GenerateTokenRequest();
                var tokenResponse = _pbiClient.Datasets.GenerateToken(workspaceId, datasetId, tokenRequest);

                QnaRequest request = new QnaRequest()
                {
                    ConversationReference = dc.Context.Activity.GetConversationReference(),
                    AccessToken = tokenResponse.Token,
                    EmbedUrl = "https://app.powerbi.com/qnaEmbed",
                    DatasetId = datasetId,
                    Query = query
                };

                Channel<QnaRequest> channel = dc.Context.TurnState.Get<Channel<QnaRequest>>();
                await channel.Writer.WriteAsync(request);

                return await dc.EndDialogAsync(result: true, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                TelemetryClient.TrackException(ex);
                return await dc.EndDialogAsync(result: false, cancellationToken: cancellationToken);
            }
        }
    }
}
