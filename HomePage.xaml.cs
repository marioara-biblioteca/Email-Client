using MimeKit;
using MimeKit.Text;
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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    /// 
    public partial class HomePage : Page, INotifyPropertyChanged
    {
        private List<string> queryResult=new List<string>();
        public static string currentFolder = "";
        public List<string> QueryResult
        {
            get { return queryResult; }
            set
            {
                queryResult = value;
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
        public static Frame ContentFrame { get; set; }
        public HomePage()
        {
            InitializeComponent();
            this.DataContext = this;
            searchResult.Visibility = Visibility.Collapsed;
            queryResult.Clear();
            ContentFrame = contentFrame;
            foldersList.ItemsSource = ImapService.GetFolders();
            ClearRoom();
        }
        public static void ClearRoom()
        {
            // Add an initial content
           
            StackPanel panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = "Navigate anywhere.",
                FontSize = 20,
                Width = 200,
                Height = 50
            });

            ContentFrame.Content = panel;
        }

        private async void foldersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            searchResult.Visibility = Visibility.Collapsed;
            EmailFolder folder = (EmailFolder)foldersList.SelectedValue;       
            FolderMessagePage fpage =new FolderMessagePage(folder.Title);
            currentFolder = folder.Title;
            await fpage.Load();
            ContentFrame.Content = fpage;
        }
        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            searchResult.Visibility = Visibility.Collapsed;
            ContentFrame.Content = new CreateAndSend();
            
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (searchButton.Content.ToString() == "Search")
            {
                string query = searchBox.Text;
                if (!String.IsNullOrEmpty(query))
                {

                    foreach (var msg in ImapService.client.messages)
                    {

                        if (msg.Envelope.Subject != null && msg.Envelope.Subject.ToString().ToLower().Contains(query.ToLower()))
                        {
                            queryResult.Add(String.Format("{0}  {1}   {2}", msg.Envelope.Subject.ToString(), msg.Envelope.Date.ToString(), msg.Envelope.Sender.ToString()));
                        }
                    }
                    if (queryResult == null)
                    {
                        var result = queryResult.Select(s => new { value = s }).ToList();
                        searchResult.ItemsSource = result;//queryResult; 
                        searchResult.Columns[0].Visibility = Visibility.Hidden;
                        
                        contentFrame.Visibility = Visibility.Hidden;
                        searchResult.Visibility = Visibility.Visible;
                        searchButton.Content = "X";
                    }
                    else
                    {
                        MessageBox.Show("No results found for your search.");
                        searchBox.Text = "Search";
                    }
                }
                else
                {
                    MessageBox.Show("Enter some text");
                }
            }
            else
            {
                searchResult.Visibility = Visibility.Hidden;
                contentFrame.Visibility= Visibility.Visible;
                searchButton.Content = "Search";
                queryResult.Clear();

            }
        }
        private void searchBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            searchBox.Text = "";
        }

        private void searchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
           
            if (searchBox.Text.Equals("Search"))
            {
                searchBox.Text = "";
            }
        }

        private void searchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

       
    }
}
