using MailKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace EmailClientWpf
{
    /// <summary>
    /// Interaction logic for FolderMessagePage.xaml
    /// </summary>
    

    public partial class FolderMessagePage : Page, INotifyPropertyChanged
    {
        private string folderTitle;

        public string FolderTitle
        {
            get { return folderTitle; }
            set
            {
                folderTitle = value;
                NotifyPropertyChanged();
            }
        }
        private List<string> messagesForFolder=new List<string>();

       
        public List<string> MessagesForFolder
        {
            get { return messagesForFolder; }
            set
            {
                messagesForFolder = value;
                NotifyPropertyChanged();
            }
        }
        private string singleMessage;
        public string SingleMessage
        {
            get { return singleMessage; }
            set
            {
                singleMessage = value;
                NotifyPropertyChanged();
            }
        }
  
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public FolderMessagePage()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public  FolderMessagePage(string name)
        {
            InitializeComponent();
            folderTitle = name;
           // ImapService.client.setSpecialFolder(name);
            this.DataContext = this;
        }

        public async Task Load()
        {
            // ImapService.client.setSpecialFolder(folderTitle);
            //await ImapService.client.openSpecialFolder();
            await ImapService.client.FetchMessageSummariesAsync(folderTitle);
            await Task.Run(() =>
            {
                foreach (var msg in ImapService.client.messages)
                {
                    if (msg.Body.ToString() != null)
                    {
                        //serializam mesajul ca sa putem sa il deserializam in MessagePage
                        messagesForFolder.Add(String.Format("{0}\t{1}\t{2}\t{3}", msg.Envelope.From.Mailboxes.First().Name, msg.NormalizedSubject.ToString(),msg.Date.Date.ToString(),msg.UniqueId.ToString()));
                        
                    }
                }
            });
            
            messagesList.ItemsSource = messagesForFolder;
        }
        private void messagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the message
            var messages = messagesList.SelectedItems;

            if (messages.Count==1)
            {
               // HomePage.ContentFrame.Content = new MessagePage(messages[0].ToString());
                HomePage.ContentFrame.Content = new MessagePage(messages[0].ToString(),this);
            }
            else
            {
                MessageBox.Show("Only one email should be selected at a time!");
            }
        }
    }
}
