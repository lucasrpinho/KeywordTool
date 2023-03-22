using KeywordTool.DAO;
using KeywordTool.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeywordTool
{
    public class EmailsProcessor
    {
        static void Main(string[] args)
        {
            string machineName = Environment.MachineName.ToString().ToUpper();
            bool readFromProject = true;
            string choice = "N";
            string filePath = string.Empty;
            Console.WriteLine($"Olá, {machineName}");
            
            Console.WriteLine("Deseja ler o arquivo de mensagens automaticamente (1) ou definir um caminho para o arquivo? (0)");
            choice = Console.ReadLine();
            readFromProject = choice[0] != '0';

            if (!readFromProject) 
            {
                Console.WriteLine("Digite um caminho válido para o arquivo: ");
                filePath = Console.ReadLine();
            }
            else
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string pastaProj = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(exePath)));
                filePath = pastaProj + ConfigurationManager.AppSettings.Get("FILE_PATH").ToString();
            }
            DirectoryInfo dir = new DirectoryInfo(filePath);
            FileInfo[] files = dir.GetFiles("*.txt");

            if (files.Length == 0)
            {
                Console.WriteLine("A busca não retornou resultados! ");
            }
            else
            {
                string connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
                EmailDAO dao = new EmailDAO(connString);
                List<Emails> lstEmails = new List<Emails>();

                foreach (FileInfo file in files)
                {
                    //if (!ValidateFile(file))
                    //{
                    //    continue;
                    //}

                    byte[] bytes = File.ReadAllBytes(file.FullName);
                    var pegadinha = Encoding.Convert(Encoding.GetEncoding("iso-8859-1"), Encoding.UTF8, bytes);
                    var strEncoded = Encoding.UTF8.GetString(pegadinha);
                    string[] content = strEncoded.Split('\n');

                    Emails oEmail = GetEmails(content, file.Name);
                    lstEmails.Add(oEmail);
                }

                if (lstEmails.Count > 0)
                {
                    var isDuplicado = dao.Get();
                    if (isDuplicado != null && isDuplicado.Count > 0) { lstEmails.RemoveAll(e => isDuplicado.Any(x => x.Nome_Arquivo == e.Nome_Arquivo)); }                    
                    dao.SaveEmails(lstEmails);
                    Console.ReadKey();
                }
            }
        }
        private static Emails GetEmails(string[] content, string fileName)
        {             
            string remetente = string.Empty;
            string destinatario = string.Empty;
            string assunto = string.Empty;
            DateTime dataEnvio = DateTime.MinValue;
            string corpoEmail = string.Empty;
            List<Emails> lstEmails = new List<Emails>();
            string formatosData = "dddd, d 'de' MMMM 'de' yyyy HH:mm" ;
            bool flagAssunto = false;

            foreach (string linha in content) 
            {
                if (flagAssunto)
                {
                    if (!string.IsNullOrEmpty(linha))
                    {
                        assunto = assunto.Trim() + linha.Trim();
                        break;
                    }
                }
                int pos = linha.IndexOf(':');
                if (pos > 0) 
                {
                    string atributo = linha.Substring(0, pos).Trim();
                    byte[] bytesVal = Encoding.UTF8.GetBytes(linha.Substring(pos + 1).Trim());
                    string valor = Encoding.UTF8.GetString(bytesVal);
                    switch (atributo)
                    {
                        case "De":
                            remetente = valor;
                            break;
                        case "Enviado em":
                            dataEnvio =  DateTime.ParseExact(valor, formatosData, CultureInfo.GetCultureInfo("pt-BR"));
                            break;
                        case "Para":
                            destinatario = valor;
                            break;
                        case "Assunto":
                            assunto = valor;
                            flagAssunto = true;
                            break;
                        default: 
                            break;
                    }
                }
            }

            corpoEmail = String.Join(Environment.NewLine, content);
            corpoEmail = corpoEmail.Trim();
            corpoEmail = corpoEmail
                .Replace('\t', ' ')
                .Replace('\n', ' ')
                .Replace('\r', ' ')
                .Replace("\r\n", " ");
            corpoEmail = corpoEmail
                .Replace("Enviado em: " + dataEnvio.ToString("dddd, d 'de' MMMM 'de' yyyy HH:mm"), "")
                .Replace("Para: " + destinatario, "")
                .Replace("De: " + remetente, "")
                .Replace("Assunto: ", "")
                .Replace(assunto, "");

            Emails e = new Emails
            {
                DatEmail = dataEnvio,
                EmailCorpo = corpoEmail,
                Destinatario = destinatario,
                Remetente = remetente,
                Assunto = assunto,
                Nome_Arquivo = fileName
            };

            var resultado = new List<ValidationResult>();
            var contexto = new ValidationContext(e);
            bool isValid = Validator.TryValidateObject(e, contexto, resultado, true);
            if (isValid)
                return e;
            else
            {
                Console.WriteLine($"O arquivo: {fileName} apresentou inconsistência nas informações e não será salvo!");
                return null;
            }
        }
    }
}
