namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public class PbiOptions
    {
        public string AuthorityUri { get; set; } 
        public string TenantId { get; set; } 
        public string ClientId { get; set; } 
        public string ClientSecret { get; set; } 
        public string Scope { get; set; } 
        public string WorkspaceId { get; set; } 
        public string RenderUrl { get; set; } 
        public string BrowserDelayMilis { get; set; }
    }
}