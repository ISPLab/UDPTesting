using System;
using log4net;
using log4net.Config;

namespace TestNet4Log
{
    public static class Logger
    {
        private static ILog log = LogManager.GetLogger("Logger");

        public static ILog Log
        {
            get { return log; }
        }
        public static void InitLogger()
        {
            XmlConfigurator.Configure();
        }
    }
}
