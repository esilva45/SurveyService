using System;
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
            XElement configXml = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + @"config.xml");
            string license = configXml.Element("LicenseKey").Value.ToString();

            if (!License.VerifyLicence(license)) {
                this.Stop();
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
