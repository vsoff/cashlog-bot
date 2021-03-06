﻿using Autofac;
using Serilog;
using System;
using System.IO;
using Cashlog.Core.Logging;
using ILogger = Cashlog.Core.Common.ILogger;

namespace Cashlog.Core.Ioc
{
    public class LoggerModule : Module
    {
        private static ILogger _logger = null;

        public static ILogger GetLogger()
        {
            if (_logger == null)
            {

                var fullPathFormat = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "Main_{Date}.txt");
                var serilogLogger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.RollingFile(fullPathFormat, outputTemplate: "{Timestamp:HH:mm:ss.fff zzz} [{Level}]: {Message}{NewLine}{Exception}")
                    .CreateLogger();

                _logger = new SerilogLogger(serilogLogger);
            }

            return _logger;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(GetLogger());
        }
    }
}
