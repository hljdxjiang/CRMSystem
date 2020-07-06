using System;
using Microsoft.Extensions.Logging;

namespace ConsoleTest
{
    public class Test
    {
        private static readonly ILogger _logger = new LoggerFactory().AddLog4Net().CreateLogger<Test>();

        public void Print() {
            _logger.LogInformation("infor test");
        }
    }
}