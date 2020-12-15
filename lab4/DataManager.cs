using System.ServiceProcess;
using HelpLibrary;

namespace Lab4
{
    public partial class DataManager : ServiceBase
    {
        readonly DataBaseWorker appInsights;
        readonly DataOptions dataOptions;
        public DataManager(DataOptions dataOptions, DataBaseWorker appInsights)
        {
            InitializeComponent();
            this.dataOptions = dataOptions;
            this.appInsights = appInsights;
        }
        protected override void OnStart(string[] args)
        {
            DataBaseWorker reader = new DataBaseWorker(dataOptions.ConnectionString);
            FileTransfer fileTransfer = new FileTransfer(dataOptions.OutputFolder, dataOptions.SourcePath);
            string customersFileName = "customers";
            reader.GetCustomers(dataOptions.OutputFolder, appInsights, customersFileName);
            fileTransfer.SendFileToFtp($"{customersFileName}.xml");
            fileTransfer.SendFileToFtp($"{customersFileName}.xsd");
            appInsights.InsertInsight("Files uploaded");
        }
        protected override void OnStop()
        {
            appInsights.InsertInsight("Stop");
            appInsights.WriteInsightsToXml(dataOptions.OutputFolder);
        }
    }
}
