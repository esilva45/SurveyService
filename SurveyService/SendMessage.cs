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
        public static string cdr_file = null;
        public static bool queue = false;

        public static void Message() {
            NpgsqlConnection conn = Connection.GetConnection();
            CallModel call = new CallModel();
            XElement configXml = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "config.xml");
            urlligacao = configXml.Element("UrlLigacao").Value.ToString();
            urlservice = configXml.Element("UrlService").Value.ToString();
            cdr_file = configXml.Element("CDRFile").Value.ToString();
            queue = bool.Parse(configXml.Element("WithQueue").Value.ToString());

            string ids = "";
            string result = "";
            string call_id = "";
            string extension_id = "";
            string[] words = null;
            string[] newwords = null;

            try {
                string query = "select survey_id, authorization_key, public_key, call_id, question_id, " +
                    "grade_id, source_number, extension_id, fila_id " +
                    "from survey " +
                    "where processed = false and extension_id != ''";

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
                    Update("", ids, "send");
                }

                query = "select survey_id, call_id, question_id " +
                    "from survey " +
                    "where extension_id = ''";

                command = new NpgsqlCommand(query, conn);
                NpgsqlDataReader rd1 = command.ExecuteReader();

                while (rd1.Read()) {
                    foreach (var line in File.ReadAllLines(cdr_file)) {
                        if (line.Contains(rd1["call_id"].ToString())) {
                            words = line.Split(',');

                            if (queue) {
                                newwords = words[19].Split(';');
                                extension_id = string.Join("", System.Text.RegularExpressions.Regex.Split(newwords[2], @"[^\d]"));                               
                            } else {
                                extension_id = string.Join("", System.Text.RegularExpressions.Regex.Split(words[8], @"[^\d]"));
                            }

                            if (extension_id != "") {
                                Update(extension_id, rd1["survey_id"].ToString(), "search");
                                extension_id = "";
                            }
                        }
                    }
                }

                rd1.Close();
            }
            catch (Exception ex) {
                Util.Log("Message: " + ex.ToString());
            }
            finally {
                if (conn != null) {
                    conn.Close();
                }
            }
        }

        private static void Update(string extension, string id, string action) {
            NpgsqlConnection conn = Connection.GetConnection();
            string query = "";

            try {
                if (action.Equals("send")) {
                    query = "update survey set processed = true where survey_id in (" + id + ")";               
                } else {
                    query = "update survey set extension_id = " + extension + " where survey_id = " + id;
                }

                NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) {
                Util.Log("Update: " + ex.ToString());
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
                Util.Log(json);

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
                    HttpStatusCode statusCode = httpResponse.StatusCode;
                    code = statusCode.ToString();
                    Util.Log("ID: " + id + " code: " + code);                   
                }
            }
            catch (Exception ex) {
                Util.Log("Send: " + ex.ToString());
            }

            return code;
        }
    }
}
