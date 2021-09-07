using System.Threading.Channels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public class PowerBIComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<CreateDataset>(CreateDataset.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ModifyTable>(ModifyTable.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AddRows>(AddRows.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<GetReport>(GetReport.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<ListReports>(ListReports.Kind));
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<GetQna>(GetQna.Kind));
        }
    }
}
