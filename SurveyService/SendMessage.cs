using Newtonsoft.Json;
using Npgsql;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SurveyService {
    class SendMessage {
        public static string urlligacao = null;
        public static string urlservice = null;

        public static void Message() {
            NpgsqlConnection conn = Connection.GetConnection();
            CallModel call = new CallModel();
            XElement configXml = XElement.Load(System.AppDomain.CurrentDomain.BaseDirectory + @"\config.xml");
            urlligacao = configXml.Element("UrlLigacao").Value.ToString();
            urlservice = configXml.Element("UrlService").Value.ToString();
            
            string ids = "";
            string result = "";
            string call_id = "";

            try {
                string query = "select survey_id, authorization_key, public_key, call_id, question_id, " +
                    "grade_id, source_number, extension_id, fila_id " +
                    "from survey " +
                    "where processed = false";

                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                NpgsqlDataReader rd = command.ExecuteReader();

                while (rd.Read()) {
                    result = "";
                    int titlePos = rd["call_id"].ToString().IndexOf("_");
                    call_id = rd["call_id"].ToString().Substring(titlePos + 1);
                    call.UrlLigacao = urlligacao + Util.FindDirectory(rd["extension_id"].ToString(), rd["source_number"].ToString(), call_id);
                    call.AuthorizationKey = rd["authorization_key"].ToString();
                    call.PublicKey = rd["public_key"].ToString();
                    call.CallId = rd["call_id"].ToString();
                    call.QuestionId = rd["question_id"].ToString();
                    call.GradeId = rd["grade_id"].ToString();
                    call.SourceNumber = rd["source_number"].ToString();
                    call.ExtensionId = rd["extension_id"].ToString();
                    call.FilaId = rd["fila_id"].ToString();
                    result = Send(JsonConvert.SerializeObject(call), rd["survey_id"].ToString());

                    if (result.Equals("OK")) {
                        ids += rd["survey_id"] + " ";
                    }
                }

                rd.Close();

                if (ids != "") {
                    ids = ids.Trim().Replace(" ", ",");
                    NpgsqlCommand cmd = new NpgsqlCommand("update survey set processed = true where survey_id in (" + ids + ")", conn);
                    cmd.ExecuteReader();
                }
            }
            catch (Exception ex) {
                Util.Log(ex.ToString());
            }
            finally {
                if (conn != null) {
                    conn.Close();
                }
            }
        }

        private static string Send(string json, string id) {
            string code = "ERROR";

            try {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(urlservice);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                byte[] data = Encoding.UTF8.GetBytes(json);
                httpWebRequest.ContentLength = data.Length;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                HttpStatusCode respStatusCode = httpResponse.StatusCode;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                    HttpStatusCode statusCode = ((HttpWebResponse)httpResponse).StatusCode;
                    //var result = streamReader.ReadToEnd();
                    code = statusCode.ToString();
                    Util.Log("ID: " + id + " code: " + code);
                    Util.Log(json);
                }
            }
            catch (Exception ex) {
                Util.Log(ex.ToString());
            }

            return code;
        }
    }
}
