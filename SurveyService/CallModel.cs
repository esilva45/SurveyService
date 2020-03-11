
namespace SurveyService {
    class CallModel {
        public CallModel() { }

        public CallModel(string url_ligacao, string authorization_key, string public_key, string call_id, string question_id,
            string grade_id, string source_number, string extension_id, string fila_id) {
            UrlLigacao = url_ligacao;
            AuthorizationKey = authorization_key;
            PublicKey = public_key;
            CallId = call_id;
            QuestionId = question_id;
            GradeId = grade_id;
            SourceNumber = source_number;
            ExtensionId = extension_id;
            FilaId = fila_id;
        }

        public string UrlLigacao { get; set; }

        public string AuthorizationKey { get; set; }

        public string PublicKey { get; set; }

        public string CallId { get; set; }

        public string QuestionId { get; set; }

        public string GradeId { get; set; }

        public string SourceNumber { get; set; }

        public string ExtensionId { get; set; }

        public string FilaId { get; set; }
    }
}
