using MailKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for MessagePage.xaml
    /// </summary>
    public partial class MessagePage : Page
    {
        public FolderMessagePage parent;
        public static Frame MessageFrame { get; set; }
        public MessagePage()
        {
            InitializeComponent();
            this.DataContext = this;
            MessageFrame = messageFrame;
        }
        public MessagePage(string message)
        {
            InitializeComponent();
            this.DataContext = this;
            MessageFrame = messageFrame;

            string []subs = message.Split('\t');
           // subject.Text = subs[0];
            //body.Text = subs[1];
            //time.Text = subs[2];
            //momentan
            attachments.Text = "No attachments";
            UniqueId UID = new MailKit.UniqueId(UInt32.Parse(subs[3]));
            IMessageSummary messageSummary = ImapService.client.GetMeessageSummaryByUID(UID);

            subject.Text = messageSummary.NormalizedSubject;

            try
            {
                //asta merge pe clientul care comunica cu GMAIL, dar aici nu stiu cum sa configurez serverul nostru de imap ca sa ii placa
                BodyPartText bodyPart = messageSummary.TextBody;
                var bodyText = ImapService.client.mailFolder.GetBodyPartAsync(messageSummary.UniqueId, bodyPart);

                string tmp = bodyText.Result.ToString();
                int posStart = tmp.IndexOf(bodyText.Result.ContentType.ToString()) + bodyText.Result.ContentType.ToString().Length;

                body.Text = tmp.Substring(posStart, tmp.Length - posStart);


            }
            catch (Exception e)
            {
                //asa ca aici PETICIM
                string inboxPath = "C:\\INBOX";
                
                string userAddress = messageSummary.Envelope.To.ToString();
                string username = extracUserFromMail(userAddress);
                inboxPath = System.IO.Path.Combine(inboxPath, username);
                if(HomePage.currentFolder != "Inbox")
                    inboxPath = System.IO.Path.Combine(inboxPath, HomePage.currentFolder.ToLower());
                inboxPath = System.IO.Path.Combine(inboxPath, messageSummary.UniqueId.ToString() + ".txt");

                MimeKit.MimeMessage messagePeticit = MimeKit.MimeMessage.Load(inboxPath);
                body.Text = messagePeticit.TextBody.ToString();
            }

            //body.Text = "BODY";
            time.Text = messageSummary.Date.ToString();
        }
        public MessagePage(string message,FolderMessagePage parent)
        {
            InitializeComponent();
            this.DataContext = this;
            MessageFrame = messageFrame;
            this.parent = parent;
            string[] subs = message.Split('\t');
            // subject.Text = subs[0];
            //body.Text = subs[1];
            //time.Text = subs[2];
            //momentan
            attachments.Text = "No attachments";
            UniqueId UID = new MailKit.UniqueId(UInt32.Parse(subs[3]));
            IMessageSummary messageSummary = ImapService.client.GetMeessageSummaryByUID(UID);

            subject.Text = messageSummary.NormalizedSubject;

            //try
            //{
            //    //asta merge pe clientul care comunica cu GMAIL, dar aici nu stiu cum sa configurez serverul nostru de imap ca sa ii placa
            //    BodyPartText bodyPart = messageSummary.TextBody;
            //    var bodyText = ImapService.client.mailFolder.GetBodyPartAsync(messageSummary.UniqueId, bodyPart);

            //    string tmp = bodyText.Result.ToString();
            //    int posStart = tmp.IndexOf(bodyText.Result.ContentType.ToString()) + bodyText.Result.ContentType.ToString().Length;

            //    body.Text = tmp.Substring(posStart, tmp.Length - posStart);


            //}
            //catch (Exception e)
            {
                //asa ca aici PETICIM
                string inboxPath = "C:\\INBOX";
                //string userAddress = messageSummary.Envelope.To.ToString();
                //string username = extracUserFromMail(userAddress);
                string username = IdleClient.currentUser;
                inboxPath = System.IO.Path.Combine(inboxPath, username);
                if(HomePage.currentFolder == "Sent")
                    inboxPath = System.IO.Path.Combine(inboxPath, "sent");
                inboxPath = System.IO.Path.Combine(inboxPath, messageSummary.UniqueId.ToString() + ".txt");

                MimeKit.MimeMessage messagePeticit = MimeKit.MimeMessage.Load(inboxPath);
                body.Text = messagePeticit.TextBody.ToString();
            }

            //body.Text = "BODY";
            time.Text = messageSummary.Date.ToString();
        }

        private static string[] extractMails(string text)
        {
            Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*",
                RegexOptions.IgnoreCase);
            MatchCollection emailMatches = emailRegex.Matches(text);
            List<string> result = new List<string>();
            foreach (Match emailMatch in emailMatches)
                result.Add(emailMatch.Value);
            return result.ToArray();
        }
        public static string extracUserFromMail(string mailAddres)
        {

            string result = "";
            foreach (var character in mailAddres)
            {

                if (character == '@')
                    break;
                result += character;
            }
            return result;
        }
        
        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            messageFrame.Content = parent;
            NavigationService.Navigate(null);
            //zice ca contentul e null???
        }
    }
}
