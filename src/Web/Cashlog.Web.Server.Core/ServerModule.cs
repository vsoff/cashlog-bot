using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services;
using Cashlog.Core.Services.Abstract;
using Cashlog.Core.Services.Main;

namespace Cashlog.Web.Server.Core
{
    public class ServerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CustomerService>().As<ICustomerService>().SingleInstance();
            builder.RegisterType<ReceiptService>().As<IReceiptService>().SingleInstance();
            builder.RegisterType<WebServerDatabaseContextProvider>().As<IDatabaseContextProvider>().SingleInstance();
            builder.RegisterType<WebServerSettingsService>().As<ISettingsService<WebServerSettings>>().SingleInstance();
        }
    }
}