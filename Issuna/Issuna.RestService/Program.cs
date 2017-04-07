using System;
using Happer;
using Happer.Hosting.Self;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;

namespace Issuna.RestService
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            ILog log = Logger.Get<Program>();

            var container = new ModuleContainer();
            //container.AddModule(new TestModule());

            var bootstrapper = new Bootstrapper();
            var engine = bootstrapper.BootWith(container);

            string uri = "http://localhost:3202/";
            var host = new SelfHost(engine, new Uri(uri));
            host.Start();
            Console.WriteLine("RestService is listening on [{0}].", uri);

            Console.WriteLine("Type something to stop ...");
            Console.ReadKey();

            host.Stop();
            Console.WriteLine("Stopped. Goodbye!");
        }
    }
}
