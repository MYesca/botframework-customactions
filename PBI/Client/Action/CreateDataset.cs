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
    public class CreateDataset : Dialog
    {
        private readonly string urlPowerBiServiceApiRoot = "https://api.powerbi.com";
        private PowerBIClient _pbiClient;

        [JsonProperty("$kind")]
        public const string Kind = "CreateDataset";

        [JsonProperty("datasetDefinition")]
        public ValueExpression DatasetDefinition { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        [JsonConstructor]
        public CreateDataset([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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

                CreateDatasetRequest request = DatasetDefinition?.EvaluateExpression<CreateDatasetRequest>(dc.State);

                TokenCredentials tokenCredentials = await PbiCache.GetRestTokenAsync(dc, TelemetryClient, cancellationToken);
                _pbiClient = new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);

                string datasetId = DoCreateDataset(workspaceId, request);

                if (this.ResultProperty != null)
                {
                    dc.State.SetValue(this.ResultProperty.GetValue(dc.State), datasetId);
                }

                return await dc.EndDialogAsync(result: datasetId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                TelemetryClient.TrackException(ex);
                return await dc.EndDialogAsync(result: false, cancellationToken: cancellationToken);
            }
        }

        private string DoCreateDataset(Guid workspaceId, CreateDatasetRequest request)
        {
            var datasets = _pbiClient.Datasets.GetDatasetsInGroup(workspaceId);
            Dataset dataset = datasets.Value.FirstOrDefault(d => d.Name == request.Name);
            if (dataset != null)
            {
                return dataset.Id;
            }

            request.DefaultMode = DatasetMode.Push;

            dataset = _pbiClient.Datasets.PostDatasetInGroup(workspaceId, request, defaultRetentionPolicy: DefaultRetentionPolicy.BasicFIFO);

            return dataset.Id;
        }
    }
}
