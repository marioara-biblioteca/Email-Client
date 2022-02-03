using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

namespace EmailClientWpf
{
    class ImapService
    {
        public static IdleClient client { get; set; }
        static CancellationTokenSource cancel;

        //public static string ImapServer = "imap.gmail.com";
        //public static int ImapPort = 993;
        //static SecureSocketOptions SslOptions = SecureSocketOptions.Auto;

        public static string ImapServer = "213.177.4.166";
        public static int ImapPort = 143;
        static SecureSocketOptions SslOptions = SecureSocketOptions.None;


        // public static List<IMessageSummary> messages;
        //daca aici lasam pe none, face fite cu imapul de gmail si nu ii permite cumva sa se autentifice si crapa pt ca nu ii poate accesa mesajele

        public static void Initialize(string pass,string username)
        {
            client = new IdleClient(ImapServer, ImapPort, SslOptions, username, pass);
           // client.Reconnect();
            
        }
    
        public static List<EmailFolder> GetFolders()
        {
            List<EmailFolder> tmp = new List<EmailFolder>();
            tmp.Add(new EmailFolder { Title = "Inbox" });
            tmp.Add(new EmailFolder { Title = "Spam" });
            tmp.Add(new EmailFolder { Title = "Sent" });
            tmp.Add(new EmailFolder { Title = "Draft" });
            tmp.Add(new EmailFolder { Title = "Thrash" });
            return tmp;
            
        }
       public async void GetMessagesForFolder(string name)
       {
            if(client.mailFolder!=null)
                await client.FetchMessageSummariesAsync(client.mailFolder.FullName); 
       }
    }
    class EmailFolder
    {
        public string Title { get; set; }
    }
   
}
