﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Autofac;
using Autofac.Core;
using Cashlog.Core;
using Cashlog.Core.Common;
using Cashlog.Core.Core;
using Cashlog.Core.Core.Services;
using Cashlog.Core.Core.Services.Abstract;
using Cashlog.Core.Messengers;
using Cashlog.Core.Messengers.Menu;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Data;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using ZXing;

namespace Cashlog.Application.Selfhost
{
    class Program
    {
        /// <summary>
        /// Название файла с конфигурацией бота.
        /// </summary>
        private const string _botConfigFileName = "botconfig.json";

        static void Main(string[] args)
        {
            Console.WriteLine("Selfhost started!");

            if (!System.IO.File.Exists(_botConfigFileName))
            {
                Console.WriteLine($"file {_botConfigFileName} not exists!");
                return;
            }

            var config = ReadConfig();

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(config);
            builder.RegisterType<TelegramMessenger>().As<IMessenger>().SingleInstance().AutoActivate();
            builder.RegisterType<MessagesHandler>().As<IMessagesHandler>().SingleInstance().AutoActivate();
            builder.RegisterType<QueryDataSerializer>().As<IQueryDataSerializer>().SingleInstance();
            builder.RegisterType<ReceiptHandleService>().As<IReceiptHandleService>().SingleInstance();
            builder.RegisterType<DebtsCalculator>().As<IDebtsCalculator>().SingleInstance();
            builder.RegisterType<MainLogicService>().As<IMainLogicService>().SingleInstance();
            builder.RegisterType<BillingPeriodService>().As<IBillingPeriodService>().SingleInstance();
            builder.RegisterType<MoneyOperationService>().As<IMoneyOperationService>().SingleInstance();
            builder.RegisterType<CustomerService>().As<ICustomerService>().SingleInstance();
            builder.RegisterType<ReceiptService>().As<IReceiptService>().SingleInstance();
            builder.RegisterType<TelegramMenuProvider>().As<IMenuProvider>().SingleInstance();
            builder.RegisterType<GroupService>().As<IGroupService>().SingleInstance();
            builder.RegisterType<ConsoleLogger>().As<ILogger>().SingleInstance();
            IContainer container = builder.Build();
            Console.ReadLine();
        }

        static CashlogSettings ReadConfig()
        {
            var json = System.IO.File.ReadAllText(_botConfigFileName);
            return JsonConvert.DeserializeObject<CashlogSettings>(json);
        }
    }
}