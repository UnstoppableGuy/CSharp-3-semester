using System;
using System.ServiceProcess;
using System.IO;
using System.Threading;
using System.Xml.Schema;
using System.Xml.Linq;

namespace FWS
{
    public partial class Service1 : ServiceBase
    {
        Logger logger;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ConfigManager configManager;
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml")) && File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xsd")))
            {
                XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
                xmlSchemaSet.Add(string.Empty, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xsd"));
                XDocument xDocument = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml"));
                xDocument.Validate(xmlSchemaSet, ValidationEventHandler);
                configManager = new ConfigManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml"), typeof(Options));
            }
            else if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")))
            {
                configManager = new ConfigManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), typeof(Options));
            }
            else
            {
                throw new ArgumentNullException($"No config was found");
            }

            Options options = configManager.GetOptions<Options>();

            logger = new Logger(options);

            Thread loggerThread = new Thread(new ThreadStart(logger.Start));

            loggerThread.Start();
        }

        protected override void OnStop()
        {
            if (!(logger is null))
            {
                logger.Stop();
            }
        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (Enum.TryParse("Error", out XmlSeverityType type) && type == XmlSeverityType.Error)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
