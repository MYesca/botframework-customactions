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
    public class AddRows : Dialog
    {
        private readonly string urlPowerBiServiceApiRoot = "https://api.powerbi.com";
        private PowerBIClient _pbiClient;

        [JsonProperty("$kind")]
        public const string Kind = "AddRows";

        [JsonProperty("datasetId")]
        public StringExpression DatasetId { get; set; }

        [JsonProperty("tableName")]
        public StringExpression TableName { get; set; }

        [JsonProperty("rowsDefinition")]
        public ValueExpression RowsDefinition { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        [JsonConstructor]
        public AddRows([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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

                string tableName = TableName.GetValue(dc.State);

                PostRowsRequest request = RowsDefinition?.EvaluateExpression<PostRowsRequest>(dc.State);
                    
                TokenCredentials tokenCredentials = await PbiCache.GetRestTokenAsync(dc, TelemetryClient, cancellationToken);
                _pbiClient = new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);

                DoAddRows(workspaceId, datasetId, tableName, request);

                if (this.ResultProperty != null)
                {
                    dc.State.SetValue(this.ResultProperty.GetValue(dc.State), true);
                }

                return await dc.EndDialogAsync(result: true, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                if (this.ResultProperty != null)
                {
                    dc.State.SetValue(this.ResultProperty.GetValue(dc.State), false);
                }

                TelemetryClient.TrackException(ex);
                return await dc.EndDialogAsync(result: false, cancellationToken: cancellationToken);
            }
        }

        private void DoAddRows(Guid workspaceId, string datasetId, string tableName, PostRowsRequest request)
        {
            Tables tables = _pbiClient.Datasets.GetTablesInGroup(workspaceId, datasetId);
            Table table = tables.Value.FirstOrDefault(d => d.Name == tableName);
            if (table == null)
            {
                throw new Exception($"Table {tableName} not found for inserts, aborting.");
            }

            _pbiClient.Datasets.PostRowsInGroup(workspaceId, datasetId, tableName, request);
        }
    }
}
