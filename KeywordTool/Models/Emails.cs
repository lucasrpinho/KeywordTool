using System;
using System.ComponentModel.DataAnnotations;

namespace KeywordTool.Models
{
    public class Emails
    {
        private  long _id;
        private string _NomeArquivo;
        private string _Remetente;
        private string _Destinatario;
        private DateTime _DatEmail;
        private string _EmailCorpo;
        private string _Assunto;

        [Required]
        public long Id {
            get => _id; set => _id = value;
        }
        [Required]
        public string Nome_Arquivo
        {
            get => _NomeArquivo; set => _NomeArquivo = value;
        }
        [Required]
        [EmailAddress(ErrorMessage = "O e-mail do remetente consta como inválido.")]
        public string Remetente
        {
            get => _Remetente; set => _Remetente = value;
        }
        [Required(ErrorMessage = "O valor de destinatário não pode ser nulo ou vazio.")]
        public string Destinatario
        {
            get => _Destinatario; set => _Destinatario = value;
        }
        [Required(ErrorMessage = "A data de envio do e-mail precisa ser preenchida.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ssZ}", ApplyFormatInEditMode = true)]
        public DateTime DatEmail
        {
            get => _DatEmail; set => _DatEmail = value;
        }
        [Required]
        public string EmailCorpo
        {
            get => _EmailCorpo; set => _EmailCorpo = value;
        }
        [Required]
        public string Assunto
        {
            get => _Assunto; set => _Assunto = value;
        }
    }
}
