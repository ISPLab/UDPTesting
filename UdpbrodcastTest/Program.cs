using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace UdpbrodcastTest
{
    class Program
    {
        static ILog logger;
        static UdpClient u_client;
        static int udp_sock = 4445;
        static void Main(string[] args)
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            Console.WriteLine("Adapters:");
            foreach (var a in adapters)
               Console.WriteLine($"{a.Id}");
            CancellationTokenSource t_source = new CancellationTokenSource();
            var token = t_source.Token;
            Task.Run(async () =>
            {
                int count_message = 0;
                while (true)
                {
                    var cmd = Console.ReadLine();
                    switch (cmd)
                    {
                        case "send":
                            try
                            {
                            Console.WriteLine("creating udp sock for send");
                            var ipAddress = "127.0.0.1";
                            var ip = IPAddress.Parse(ipAddress);
                            u_client = new UdpClient();
                            u_client.EnableBroadcast = true;
                            var data2 = Encoding.UTF8.GetBytes($"{count_message} message");
                            u_client.Send(data2, data2.Length, "255.255.255.255", udp_sock);
                            count_message++;
                            Console.WriteLine($"{count_message} message is sent");
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            break;
                        case "receive":
                            await Task.Run(() =>
                             {
                                 if (u_client == null)
                                     CreateSock(udp_sock);
                                 logger?.Debug("receive operation is runnig");
                                 while (!token.IsCancellationRequested)
                                 {
                                     var from = new IPEndPoint(0, 0);
                                     var recvBuffer = u_client.Receive(ref from);
                                     Console.WriteLine(Encoding.UTF8.GetString(recvBuffer));
                                     System.Threading.Thread.Sleep(500);
                                 }
                                 u_client.Client.Close();
                             });
                            break;
                        case "set_default_logging":
                            SetDefaultLog();
                            break;

                        case "disabale_log":
                            throw new NotImplementedException("disable_log");
                            logger?.Debug("set_log operation is runnig");
                            logger?.Info("This is Info message");
                            Disable_logging();
                            break;

                        case "set_console_logging":
                            SetConsole_logging();
                            break;
                        case "set_console_logging_false":
                            SetConsole_logging(false);
                            break;

                        case "set_udp_logging":
                            SetUdpLogging();
                            break;

                        case "set_costom_log":
                            throw new NotImplementedException("set_costom_log");
                            break;
                        case "write_log4net_xml":
                            WriteXml();
                            break;

                        case "add_to_log":
                            logger.Debug($"Info Random guid {Guid.NewGuid().ToString()}");
                            break;
                        case "show_all_verbose":
                            logger.Trace("Trace message for test");
                            logger.Debug("Debug message for test");
                            logger.Info($"Info log message for test");
                            logger.Error($"Error log message for test");
                            logger.Fatal("Fatal message for test");
                            logger.Info($"And after set NDC and MDC");
                            ThreadContext.Stacks["NDC"].Push("ndc message");
                            log4net.MDC.Set("MDC", "MDC message");
                            logger.Trace("Trace message for test");
                            logger.Debug("Debug message for test");
                            logger.Info($"Info log message for test");
                            logger.Error($"Error log message for test");
                            logger.Fatal("Fatal message for test");
                            logger.Info($"Review in another thread");
                            await Task.Run(() => {
                                ThreadContext.Stacks["NDC2"].Push("ndc2 message");
                                log4net.MDC.Set("MDC2", "MDC2 message");
                                logger.Trace("Anather thread: Trace message for test");
                                logger.Debug("Anather thread: Debug message for test");
                                logger.Info($"Anather thread: Info log message for test");
                                logger.Error($"Anather thread: Error log message for test");
                                logger.Fatal("Anather thread: Fatal message for test", new InvalidOperationException("Invalid operation Excerption for Test"));
                            });

                            break;
                        case "reader_test":
                            var config = ReadEmbededFile("defaultconsole.config");

                            MemoryStream stream = new MemoryStream(config);
                            await SetConfig(stream);
                            await TestReader(stream);

                            break;

                        case "stop":
                            t_source.Cancel();
                            return;
                        default:
                            Console.WriteLine($"Possible {cmd} contains spell error");
                            break;
                    }
                }
            });
            /*  var data = Encoding.UTF8.GetBytes("Test init message");
              u_client.Send(data, data.Length, "255.255.255.255", port);*/
            Console.WriteLine(
                $"UDP testing commands: {System.Environment.NewLine}" +
                $"send: to send test counting message. {System.Environment.NewLine}" +
                $"receive: to reseive port { System.Environment.NewLine}" +
                $"stop: to cancel upb prodcasting. { System.Environment.NewLine}" +
                $"set_default_logging: to set console logging { System.Environment.NewLine}" +
                $"set_console_logging: to set console logging { System.Environment.NewLine}" +
                $"set_console_logging_false: to stop console logging { System.Environment.NewLine}" +
                $"set_udp_logging] add udp appender {System.Environment.NewLine}" +
                $"add_to_log: to add log something {System.Environment.NewLine}" +
                $"show_all_verbose: to show all type verbose of Logger" +
                $"disabale_log: to disable logging { System.Environment.NewLine}" +
                $"reader_test: for test xml reading configuration file {System.Environment.NewLine}" +
                $"write_log4net_xml (not tested) { System.Environment.NewLine}"
             );

            while (!token.IsCancellationRequested)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        public static void CreateSock(int port)
        {
            try
            {
              
                Console.WriteLine("creating udp sock");
                //  IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddress = IPAddress.Any;// "127.0.0.1";
                //  var ip = IPAddress.Parse(ipAddress);
                // var ipAddress = iPHostEntry.AddressList.SingleOrDefault(ip => ip.ToString().Contains("192.168."));
                u_client = new UdpClient();
                u_client.EnableBroadcast = true;
                u_client.Client.Bind(new IPEndPoint(ipAddress, port));
                Console.WriteLine($"Udp sock is created on {ipAddress}:{port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
            
        public static void WriteXml()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.NamespaceHandling = NamespaceHandling.Default;
            settings.DoNotEscapeUriAttributes= true;
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            XmlTextWriter writer = new XmlTextWriter(Console.Out);
            writer.Namespaces = false;
            // Write the book element.
            writer.WriteStartElement("a:b");
         
            // Write the title element.
            writer.WriteStartElement("title");
            writer.WriteString("Pride And Prejudice");
            writer.WriteEndElement();

            // Write the close tag for the root element.
            writer.WriteEndElement();
          
            // Write the XML and close the writer.
            writer.Close();
          

        }

        public static void SetConsole_logging(bool state =true)
        {
            var repository = LogManager.GetRepository();
            var hierarchy = (Hierarchy)repository;
            var consoleappender = repository.GetAppenders().SingleOrDefault(c => c.Name == "ColoredConsoleAppender");
            //hierarchy.ResetConfiguration();
            
            if (hierarchy.Root.Appenders.Contains(consoleappender))
            {
                if (!state)
                    hierarchy.Root.RemoveAppender(consoleappender);
                return;
            }
            else
            {
                hierarchy.Root.AddAppender(consoleappender);
            }
            var appes = hierarchy.Root.Appenders;
          
            hierarchy.Root.Level = log4net.Core.Level.All;

            // hierarchy.ResetConfiguration();

          //  log4net.Config.BasicConfigurator.Configure(repository);
          //     log4net.Config.XmlConfigurator.Configure(repository);
            logger = LogManager.GetLogger("UpdatedLogger");
            logger.Info("Log with console is enabled");
            logger = LogManager.GetLogger("TwoUpdatedLogger");
            logger.Debug("This is Debug message");
            /*var config = ReadEmbededFile("baseconfig.config");
            MemoryStream stream = new MemoryStream(config);
            log4net.Config.XmlConfigurator.Configure(stream);
            logger = log4net.LogManager.GetLogger("Programm");
            logger.Info("Log with console is enabled");*/
        }

        private static void SetUdpLogging(bool state =true, int? localport=null, int? remoteport=null, string remote_address=null)
        {
            var repository = LogManager.GetRepository();
            var hierarchy = (Hierarchy)repository;
            var udp_appender = LogManager.GetRepository().GetAppenders().SingleOrDefault(c => c.Name == "UdpAppender");
            if (hierarchy.Root.Appenders.Contains(udp_appender))
            {
                if (!state)
                    hierarchy.Root.RemoveAppender(udp_appender);
                return;
            }
            if (localport != null)
                ((log4net.Appender.UdpAppender)udp_appender).LocalPort = (int)localport;
            if (remoteport != null)
                ((log4net.Appender.UdpAppender)udp_appender).RemotePort = (int)remoteport;
            if (remote_address != null)
            {
                var ip = IPAddress.Parse(remote_address);
                ((log4net.Appender.UdpAppender)udp_appender).RemoteAddress = ip;
            }
            //     ((log4net.Appender.UdpAppender)udp_appender).ActivateOptions();
        
            hierarchy.Root.AddAppender(udp_appender);
          //  log4net.Config.BasicConfigurator.Configure(repository);
            logger = LogManager.GetLogger("UdpLog");
            logger.Info("Set udp logging done");
            /*
            log4net.Appender.UdpAppender udp_appender = GetUdpAppender();
            udp_appender.ClearFilters();
            log4net.Filter.LevelRangeFilter filter = new log4net.Filter.LevelRangeFilter();
            filter.LevelMin = log4net.Core.Level.Trace;
            filter.LevelMax = log4net.Core.Level.Fatal;
            udp_appender.AddFilter(filter);
            if (localport != null)
                udp_appender.LocalPort = (int)localport;
            if (remoteport != null)
                udp_appender.RemotePort = (int)remoteport;
            if (remote_address != null)
            {
                var ip =  IPAddress.Parse(remote_address);
                udp_appender.RemoteAddress = ip;
            }
            udp_appender.ActivateOptions();
            var res = log4net.LogManager.GetRepository().GetLogger("ForAddingAppender");
            var l = (log4net.Repository.Hierarchy.Logger)res;
            var apps = l.Appenders;
            l.Appenders.Add(udp_appender);
            logger.Info("Set udp logging done");*/
        }



        private static void StopUdpLogging()
        {
            var logger = (log4net.Repository.Hierarchy.Logger)log4net.LogManager.GetLogger("UDPLogger");

           // GetUdpAppender().Close();
        }

        private static log4net.Appender.UdpAppender GetUdpAppender()
        {
           var aps = log4net.LogManager.GetRepository().GetAppenders();
           var res = log4net.LogManager.GetRepository().GetAppenders().SingleOrDefault(c => c.Name == "UdpAppender");
           return (log4net.Appender.UdpAppender)res;
        }

        public static void SetDefaultLog()
        {
            var config=  ReadEmbededFile("defaultconfig.config");
            SetCostomConfig(new byte[0]);
        }

        

        static  async Task TestReader(System.IO.Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = true;
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                while (await reader.ReadAsync())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            Console.WriteLine("Start Element {0}", reader.Name);
                            if(reader.Name.Equals("root"))
                            {
                                XmlWriterSettings wsettings = new XmlWriterSettings();
                                wsettings.Async = true;
                                using (XmlWriter writer = XmlWriter.Create(stream, wsettings))
                                {
                                    await writer.WriteStartElementAsync("pf", "root", "http://ns");
                                }
                            }
                            break;
                        case XmlNodeType.Text:
                            Console.WriteLine("Text Node: {0}",
                                     await reader.GetValueAsync());
                            break;
                        case XmlNodeType.EndElement:
                            Console.WriteLine("End Element {0}", reader.Name);
                            break;
                        default:
                            Console.WriteLine("Other node {0} with value {1}",
                                            reader.NodeType, reader.Value);
                            break;
                    }
                }
            }
        }

        public static async Task SetConfig(Stream stream)
        {
            XmlWriterSettings wsettings = new XmlWriterSettings();
            wsettings.Async = true;
            using (XmlWriter writer = XmlWriter.Create(stream, wsettings))
            {
                await writer.WriteStartElementAsync("pf", "root2", "http://ns");
            }
        }

        public static void Disable_logging()
        {
            logger?.Info("Disable_logging");
            var config = ReadEmbededFile("log4net-config-file_disable.config");
            MemoryStream stream = new MemoryStream(config);
            log4net.Config.XmlConfigurator.Configure(stream);
            logger = log4net.LogManager.GetLogger("Programm");
            logger.Info("Disable_logging");
        }



        public static void SetCostomConfig(byte[] config)
        {
            if (logger != null)
            {
                log4net.LogManager.Shutdown();
                log4net.LogManager.ShutdownRepository();
            }

            if (config.Length == 0)
            {
                log4net.Config.XmlConfigurator.Configure();
            }
            else
            {
                MemoryStream stream = new MemoryStream(config);
                log4net.Config.XmlConfigurator.Configure(stream);
            }
            logger = log4net.LogManager.GetLogger("Programm");
            logger.Info("Log is enabled");
        }


        public static void SetCostomConfigWhy (byte[] config)
        {
            if (logger != null)
            {
                log4net.LogManager.Shutdown();
                log4net.LogManager.ShutdownRepository();
            }
            if (config.Length==0)
            {
                log4net.Config.XmlConfigurator.Configure();
            }
            else
            {
                MemoryStream stream = new MemoryStream(config);
                log4net.Config.XmlConfigurator.Configure(stream);
            }
            logger = log4net.LogManager.GetLogger("Programm");
            logger.Info("Log is enabled");
        }


   


        public static byte[] ReadEmbededFile(string filename)
        {
            var assembly = typeof(Program).Assembly;
            byte[] buffer = EmbeddedResources.ResourceLoader.GetEmbeddedResourceBytes(assembly, filename);
            return buffer;
        }

    }
                
}
