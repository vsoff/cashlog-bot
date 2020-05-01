using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Cashlog.Web.Client.Core;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Cashlog.Web.ClientSide.Selfhost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.RegisterAllServices();
            await builder.Build().RunAsync();
        }
    }
}