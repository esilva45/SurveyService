using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace SurveyService {
    class Util {
        public static string FindDirectory(string extension, string source, string call_id) {
            XElement configXml = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + @"config.xml");
            string patch = configXml.Element("PathFile").Value.ToString();
            string result = extension + "/file%20not%20found";
            string file_name = "";

            try {
                DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(patch + extension);
                FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + source + "*" + extension + "*(" + call_id + ")*.wav");

                foreach (FileInfo foundFile in filesInDir) {
                    file_name = foundFile.Name.Replace(" ", "%20");
                    result = extension + "/" + file_name.Replace("%3A", "%253A");
                }
            }
            catch (Exception ex) {
                Log(ex.ToString());
            }

            return result;
        }

        public static void VerifyDir(string path) {
            try {
                DirectoryInfo dir = new DirectoryInfo(path);

                if (!dir.Exists) {
                    dir.Create();
                }
            }
            catch { }
        }

        public static void Log(string lines) {
            VerifyDir(AppDomain.CurrentDomain.BaseDirectory + @"logs");
            string fileName = DateTime.Now.Day.ToString("00") + DateTime.Now.Month.ToString("00") + DateTime.Now.Year.ToString() + "_Logs.out";

            try {
                StreamWriter file = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"logs" + Path.DirectorySeparatorChar + fileName, true);
                file.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff", CultureInfo.InvariantCulture) + " " + lines);
                file.Close();
            }
            catch (Exception) { }
        }
    }
}
