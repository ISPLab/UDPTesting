using System;

namespace TestNet4Log
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Logger testing...");
            Console.WriteLine("Logger init..");
            Logger.InitLogger();
            Console.WriteLine("test info message");
            for(int i=0;i<4;i++)
            Logger.Log.Info("Info message done");
            Console.ReadLine();
        }
    }
}
