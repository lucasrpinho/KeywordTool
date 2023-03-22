using KeywordTool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeywordTool.DAO
{
    public class EmailDAO
    {
        private string _connString;
        private static EmailDAO _instance;

        public EmailDAO(string connStr)
        {
            _connString = connStr;
        }

        public static EmailDAO GetInstance(string connStr)
        {
            if (_instance == null)
            {
                _instance = new EmailDAO(connStr);
            }
            return _instance;
        }

        public List<Emails> Get()
        {
            List<Emails> list = new List<Emails>();
            using (SqlConnection connection = new SqlConnection(_connString))
            {
                connection.Open();

                string query = "SELECT cod_email, nom_arquivo, des_remetente, des_destinatario, dat_email, des_assunto, des_corpo_email FROM email_dados";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Emails e = new Emails();
                            e.Id = (long)reader["cod_email"];
                            e.Nome_Arquivo = (string)reader["nom_arquivo"];
                            e.Remetente = (string)reader["des_remetente"];
                            e.Destinatario = (string)reader["des_destinatario"];
                            e.DatEmail = (DateTime)reader["dat_email"];
                            e.Assunto = (string)reader["des_assunto"];
                            e.EmailCorpo = (string)reader["des_corpo_email"];
                            
                            list.Add(e);
                        }
                    }
                }
            }
            return list;
        }

        public void SaveEmails(List<Emails> emails)
        {
            using (SqlConnection connection = new SqlConnection(_connString))
            {
                connection.Open();

                string query = "INSERT INTO email_dados (nom_arquivo, des_remetente, des_destinatario, dat_email, des_assunto, des_corpo_email) " +
                               "VALUES (@NomeArquivo, @Remetente, @Destinatario, @DatEmail, @Assunto, @EmailCorpo)";

                    SqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        
                        foreach (Emails email in emails)
                        {
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Transaction = transaction;
                                command.Parameters.AddWithValue("@NomeArquivo", email.Nome_Arquivo);
                                command.Parameters.AddWithValue("@Remetente", email.Remetente);
                                command.Parameters.AddWithValue("@Destinatario", email.Destinatario);
                                command.Parameters.AddWithValue("@DatEmail", email.DatEmail);
                                command.Parameters.AddWithValue("@Assunto", email.Assunto);
                                command.Parameters.AddWithValue("@EmailCorpo", email.EmailCorpo);

                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    Console.WriteLine("Email salvo com sucesso!");
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception(ex.Message + "\r\n Reprovado! Obrigado por participar!");
                    }
                    finally { 
                        transaction.Commit();
                        transaction?.Dispose();
                        connection.Close();
                    }
                
            }
        }
    }
}
