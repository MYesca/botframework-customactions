using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;

namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public static class StartupExtensions
    {
        public static void AddPbiClient(this IServiceCollection services)
        {
            services.AddSingleton(Channel.CreateUnbounded<QnaRequest>());
            services.AddHostedService<VisualRenderWorker>();
        }

        public static void UsePbiClient(this IApplicationBuilder builder)
        {
            IBotFrameworkHttpAdapter adapter = builder.ApplicationServices.GetService<IBotFrameworkHttpAdapter>();
            ((BotAdapter)adapter).Use(new RegisterClassMiddleware<Channel<QnaRequest>>(builder.ApplicationServices.GetService<Channel<QnaRequest>>()));
        }
    }
}