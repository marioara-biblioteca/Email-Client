
using MimeKit;
using MimeKit.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;

namespace EmailClientWpf
{
    /// <summary>
    /// Interaction logic for CreateAndSend.xaml
    /// </summary>
    public partial class CreateAndSend : Page
    {
        static int lastUID = 1000;
        public CreateAndSend()
        {
            InitializeComponent();
        }
        private void sendBtn_Click(object sender, RoutedEventArgs e)
        {
            using (MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect("mail.cristianavaleca.com", 25);
                //client.EnableSsl = true;
                List<string> credentials = new List<string>();
                foreach (string line in System.IO.File.ReadLines(@"C:\Users\student\Desktop\SALUUUT\wetransfer_asyncclient-rar_2021-12-16_0918\EmailClientWpf\credentials.txt"))
                {
                    credentials.Add(line);
                }
                client.Authenticate(credentials[0], credentials[1]);
                //client.Credentials = new NetworkCredential(credentials[0], credentials[1]);

                HeaderId[] headersToSign = new HeaderId[] { HeaderId.From, HeaderId.Subject, HeaderId.Date };
                string domain = "cristianavaleca.com";
                string selector = "1638004860.cristianavaleca";
                DkimSigner signer = new DkimSigner(@"C:\Users\student\Desktop\SALUUUT\wetransfer_asyncclient-rar_2021-12-16_0918\EmailClientWpf\dkim_private.pem", domain, selector)
                {
                    SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha256,
                    AgentOrUserIdentifier = "@cristianavaleca.com",
                    QueryMethod = "dns/txt",
                };
                MimeMessage mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(credentials[0], credentials[2]));
                mimeMessage.To.Add(new MailboxAddress("prieten", to.Text));
                mimeMessage.Subject = subject.Text;
                mimeMessage.Body = new TextPart("plain") { Text = body.Text };
                mimeMessage.Prepare(EncodingConstraint.SevenBit);
                signer.Sign(mimeMessage, headersToSign);
                //mimeMessage.WriteTo("MIMEMSG.txt");
                client.Send(mimeMessage);

                //SaveToSendFolder(mimeMessage);

                HomePage.ClearRoom();

            }
        }
        

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            HomePage.ClearRoom();
        }
    }   
}
