using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public class VisualRenderWorker : BackgroundService
    {
        private readonly Channel<QnaRequest> _channel;
        private readonly ILogger<VisualRenderWorker> _logger;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly string _renderUrl;
        private readonly string _browserWsEndpoint;
        private readonly string _browserDelayMilis;

        public VisualRenderWorker(
            Channel<QnaRequest> channel,
            ILogger<VisualRenderWorker> logger,
            IBotFrameworkHttpAdapter adapter,
            IConfiguration configuration)
        {
            this._channel = channel;
            this._logger = logger;
            this._adapter = adapter;
            this._appId = configuration["MicrosoftAppId"];
            this._renderUrl = configuration["pbiOptions:renderUrl"];
            this._browserWsEndpoint = configuration["pbiOptions:browserWsEndpoint"];
            this._browserDelayMilis = configuration["pbiOptions:browserDelayMilis"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!_channel.Reader.Completion.IsCompleted)
            {
                var msg = await _channel.Reader.ReadAsync();
                try
                {
                    Browser browser;

                    if(string.IsNullOrWhiteSpace(_browserWsEndpoint))
                    {
                        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

                        browser = await Puppeteer.LaunchAsync(new LaunchOptions
                        {
                            Headless = true
                        });
                    }
                    else
                    {
                        var connectOptions = new ConnectOptions() { BrowserWSEndpoint = _browserWsEndpoint };
                        browser = await Puppeteer.ConnectAsync(connectOptions);
                    }

                    Page page = await browser.NewPageAsync();

                    string url = string.Format(_renderUrl, WebUtility.UrlEncode(msg.DatasetId), WebUtility.UrlEncode(msg.AccessToken), WebUtility.UrlEncode(msg.Query)); 

                    await page.GoToAsync(url);

                    while (page.Frames.Length == 0)
                    {
                        await Task.Delay(200);
                    }

                    await Task.Delay(string.IsNullOrWhiteSpace(_browserDelayMilis) 
                        ? 3000 
                        : int.Parse(_browserDelayMilis));

                    var container = await page.QuerySelectorAsync("#report-container");
                    // var container = await page.Frames[1].WaitForSelectorAsync("content explorationContainer newPaneColors contentReady");

                    string imageData = await container.ScreenshotBase64Async();

                    await ((BotAdapter)_adapter).ContinueConversationAsync(
                        _appId, 
                        msg.ConversationReference, 
                        async (context, token) => 
                        {
                            IMessageActivity activity = MessageFactory.Attachment(new Attachment()
                                {
                                    Name = "result.png",
                                    ContentType = "image/png",
                                    ContentUrl = $"data:image/png;base64,{imageData}"
                                });
                            await context.SendActivityAsync(activity, token);
                        }, 
                        default(CancellationToken));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "notification failed");
                }
            }
        }
    }
}