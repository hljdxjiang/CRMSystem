using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using JHC.Logger;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensions
    {
        //add 日志文件创建规则，分割规则，格式化规则，过滤规则 to appsettings.json
        public static ILoggerFactory AddFile(this ILoggerFactory factory, IConfiguration configuration)
        {
            return AddFile(factory, new LoggerSettings(configuration));
        }
        public static ILoggerFactory AddFile(this ILoggerFactory factory, LoggerSettings LoggerSettings)
        {
            factory.AddProvider(new LoggerProvider(LoggerSettings));
            return factory;
        }
    }
}