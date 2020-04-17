using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Util.Store;
using System.Net.Mail;
using MimeKit;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace ServiceManager
{
    public class GmailServiceSend
    {

        static string ApplicationName = "Service Manager";
        
        static string[] Scopes =
        {
            GmailService.Scope.GmailSend,
        };
      
        public void GenerateEmail(string toP, string fromP, string subjectP, string bodyP)
        {
           
            
            try
            {

                //String serviceAccountEmail = "servicemanager@servicemanager-273905.iam.gserviceaccount.com";

                //var certificate = new X509Certificate2(@"servicemanager.p12", "notasecret", X509KeyStorageFlags.Exportable);

                //ServiceAccountCredential credential = new ServiceAccountCredential(
                //   new ServiceAccountCredential.Initializer(serviceAccountEmail)
                //   {
                //       Scopes = new[] { GmailService.Scope.GmailCompose, GmailService.Scope.GmailSend, GmailService.Scope.GmailInsert },
                //       User = "soltechnacorp@gmail.com"
                //   }.FromCertificate(certificate));

                //// Create the service.
                //var gmail = new GmailService(new BaseClientService.Initializer()
                //{
                //    HttpClientInitializer = credential,
                //    ApplicationName = ApplicationName,
                //});

                //using (var client = new SmtpClient())
                //{
                //    client.Connect("smtp.gmail.com", 587);

                //    // use the OAuth2.0 access token obtained above
                //    var oauth2 = new SaslMechanismOAuth2("mymail@gmail.com", credential.Token.AccessToken);
                //    client.Authenticate(oauth2);

                //    client.Send(message);
                //    client.Disconnect(true);
                //}








                UserCredential credential;
                using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.ReadWrite))
                {
                    string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/testarmin.json");
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync
                        (
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "me",
                        CancellationToken.None,
                        new FileDataStore((credPath), true)
                        ).Result;
                }
                var gmail = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });


                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromP));
                message.To.Add(new MailboxAddress(toP));
                message.Subject = subjectP;
                message.Body = new TextPart("plain") { Text = @"Hey" };


                var rawMessage = "";
                using (var stream = new MemoryStream())
                {

                    message.WriteTo(stream);
                    rawMessage = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length)
                        .Replace('+', '-')
                        .Replace('/', '_')
                        .Replace("=", "");
                }
                var gmailMessage = new Google.Apis.Gmail.v1.Data.Message { Raw = rawMessage };

                Google.Apis.Gmail.v1.UsersResource.MessagesResource.SendRequest request = gmail.Users.Messages.Send(gmailMessage, fromP);
                request.Execute();
               
            }
            catch (Exception e)
            {

              
                throw e;
                

            }
        }

        public static string Encode(string text)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);

            return System.Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
}
