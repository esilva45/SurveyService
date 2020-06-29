using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Xml.Linq;

namespace SurveyService {
    public partial class Service1 : ServiceBase {
        private Timer _timer;

        public Service1() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            XElement configXml = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "config.xml");
            string license = configXml.Element("LicenseKey").Value.ToString();
            Console.WriteLine("license " + license);

            if (!License.VerifyLicence(license)) {
                //this.Stop();
                Environment.Exit(0);
            }

            _timer = new Timer(ProcessorManager, null, 0, 60000);
        }

        protected override void OnStop() {
        }

        private void ProcessorManager(object state) {
            SendMessage.Message();
        }
    }
}
