using Microsoft.Bot.Schema;

namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public class QnaResponse
    {
        public ConversationReference ConversationReference { get; set; } 
        public string ImageData { get; set; }
    }
}