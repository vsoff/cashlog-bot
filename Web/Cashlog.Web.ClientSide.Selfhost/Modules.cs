using Cashlog.Web.Client.Core;
using Cashlog.Web.Client.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cashlog.Web.ClientSide.Selfhost
{
    public static class Modules
    {
        public static void RegisterAllServices(this IServiceCollection services)
        {
            services.AddTransient<ReceiptsListViewModel>();
        }
    }
}