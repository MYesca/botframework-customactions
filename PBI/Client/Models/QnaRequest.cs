using Microsoft.Bot.Schema;

namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public class QnaRequest
    {
        public ConversationReference ConversationReference { get; set; } 
        public string AccessToken { get; set; } 
        public string EmbedUrl { get; set; } 
        public string DatasetId { get; set; } 
        public string Query { get; set; } 
    }
}