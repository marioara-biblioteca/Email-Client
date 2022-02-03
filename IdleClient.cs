using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailClientWpf
{
    class IdleClient
    {
        readonly string host, username, password;
        readonly SecureSocketOptions sslOptions;
        readonly int port;

        CancellationTokenSource done;
        CancellationTokenSource cancel;

        public static string currentUser = "";

        public List<IMessageSummary> messages { get; set; }
        public IMessageSummary GetMeessageSummaryByUID(UniqueId UID)
        {
            foreach (var message in messages)
            {
                if (message.UniqueId == UID)
                    return message;

            }
            return null;
        }
        bool messagesArrived;
        public IMailFolder mailFolder { get; set; }
        static ImapClient client = new ImapClient(new ProtocolLogger(Console.OpenStandardError()));

        public bool IsConnected() { return client.IsConnected; }
        public bool IsAuthenticated() { return client.IsAuthenticated; }
        // public async Task setSpecialFolder(string name)
        public void setSpecialFolder(string name)
        {
            // await  Task.Run(() =>
            // {
            if (mailFolder != null)
            {
                
                mailFolder.Close();
                messages.Clear();
            }
            switch (name)
            {
                case "Inbox":
                    mailFolder = client.Inbox;
                    break;
                case "Spam":
                    mailFolder = client.GetFolder(SpecialFolder.Junk);
                    break;
                case "Draft":
                    mailFolder = client.GetFolder(SpecialFolder.Drafts);
                    break;
                case "Sent":
                    mailFolder = client.GetFolder(SpecialFolder.Sent);
                    break;
                default:
                    mailFolder = client.GetFolder(SpecialFolder.Trash);
                    break;
            }
            mailFolder.Open(FolderAccess.ReadOnly, cancel.Token);
            //  });

        }
        //  public async Task openSpecialFolder() { await mailFolder.OpenAsync(FolderAccess.ReadOnly, cancel.Token); } 
        public IdleClient(string host, int port, SecureSocketOptions sslOptions, string username, string password)
        {
            //this.client = new ImapClient(new ProtocolLogger(Console.OpenStandardError()));
            this.messages = new List<IMessageSummary>();
            this.cancel = new CancellationTokenSource();
            this.sslOptions = sslOptions;
            this.username = username;
            this.password = password;
            this.host = host;
            this.port = port;
        }

        public async Task ReconnectAsync()
        {
            if (!client.IsConnected)
            {
                await client.ConnectAsync(host, port, sslOptions, cancel.Token);

            }

            if (!client.IsAuthenticated)
            {
                 await client.AuthenticateAsync(username, password, cancel.Token);
                
            }
            else
            {
                MainWindow.LoggedIn = true;
            }

        }

        public async Task FetchMessageSummariesAsync(string name)
        {
            IList<IMessageSummary> fetched = null;
            
            do
            {
                try
                {
                   
                    if (mailFolder == null || mailFolder.FullName != name)
                        setSpecialFolder(name);
                    // fetch summary information for messages that we don't already have
                    int startIndex = messages.Count;
                    await Task.Run(() =>
                    {
                        fetched = mailFolder.Fetch(startIndex, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.BodyStructure | MessageSummaryItems.Envelope, cancel.Token);
                    });
                    break;
                }
                catch (ImapProtocolException)
                {
                    await ReconnectAsync();
                }
                catch (IOException)
                {
                    await ReconnectAsync();
                }
            } while (true);

            foreach (var message in fetched)
            {
                Console.WriteLine("{0}: new message: {1}", mailFolder, message.Envelope.Subject);
                messages.Add(message);
            }
        }


        public async Task WaitForNewMessagesAsync()
        {
            do
            {
                try
                {
                    if (client.Capabilities.HasFlag(ImapCapabilities.Idle))
                    {
                        // Note: IMAP servers are only supposed to drop the connection after 30 minutes, so normally
                        // we'd IDLE for a max of, say, ~29 minutes... but GMail seems to drop idle connections after
                        // about 10 minutes, so we'll only idle for 9 minutes.
                        done = new CancellationTokenSource(new TimeSpan(0, 9, 0));
                        try
                        {
                            await client.IdleAsync(done.Token, cancel.Token);
                        }
                        finally
                        {
                            done.Dispose();
                            done = null;
                        }
                    }
                    else
                    {
                        // Note: we don't want to spam the IMAP server with NOOP commands, so lets wait a minute
                        // between each NOOP command.
                        await Task.Delay(new TimeSpan(0, 1, 0), cancel.Token);
                        await client.NoOpAsync(cancel.Token);
                    }
                    break;
                }
                catch (ImapProtocolException)
                {
                    // protocol exceptions often result in the client getting disconnected
                    await ReconnectAsync();
                }
                catch (IOException)
                {
                    // I/O exceptions always result in the client getting disconnected
                    await ReconnectAsync();
                }
            } while (true);
        }

        public async Task IdleAsync()
        {
            do
            {
                try
                {
                    await WaitForNewMessagesAsync();

                    if (messagesArrived && mailFolder != null)
                    {
                        await FetchMessageSummariesAsync(mailFolder.FullName);
                        messagesArrived = false;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            } while (!cancel.IsCancellationRequested);
        }

        public async Task RunAsync()
        {
            // connect to the IMAP server and get our initial list of messages
            try
            {
                await ReconnectAsync();
                if (mailFolder != null)
                    await FetchMessageSummariesAsync(mailFolder.FullName);
            }
            catch (OperationCanceledException)
            {
                await client.DisconnectAsync(true);
                return;
            }
            catch(MailKit.Security.AuthenticationException authex)
            {
                //Console.WriteLine("Wrong credentials, try again");
                throw authex;
            }

            if (mailFolder != null)
            {
                var inbox = mailFolder;

                inbox.CountChanged += OnCountChanged;
                inbox.MessageExpunged += OnMessageExpunged;
                inbox.MessageFlagsChanged += OnMessageFlagsChanged;

                await IdleAsync();

                inbox.MessageFlagsChanged -= OnMessageFlagsChanged;
                inbox.MessageExpunged -= OnMessageExpunged;
                inbox.CountChanged -= OnCountChanged;
            }
            //await client.DisconnectAsync(true);
        }

        // Note: the CountChanged event will fire when new messages arrive in the folder and/or when messages are expunged.
        void OnCountChanged(object sender, EventArgs e)
        {
            var folder = (ImapFolder)sender;
            if (folder.Count > messages.Count)
            {
                int arrived = folder.Count - messages.Count;

                if (arrived > 1)
                    Console.WriteLine("\t{0} new messages have arrived.", arrived);
                else
                    Console.WriteLine("\t1 new message has arrived.");

                // Note: your first instinct may be to fetch these new messages now, but you cannot do
                // that in this event handler (the ImapFolder is not re-entrant).
                // 
                // Instead, cancel the `done` token and update our state so that we know new messages
                // have arrived. We'll fetch the summaries for these new messages later...
                messagesArrived = true;
                done?.Cancel();
            }
        }

        void OnMessageExpunged(object sender, MessageEventArgs e)
        {
            var folder = (ImapFolder)sender;

            if (e.Index < messages.Count)
            {
                var message = messages[e.Index];

                Console.WriteLine("{0}: message #{1} has been expunged: {2}", folder, e.Index, message.Envelope.Subject);

                // Note: If you are keeping a local cache of message information
                // (e.g. MessageSummary data) for the folder, then you'll need
                // to remove the message at e.Index.
                messages.RemoveAt(e.Index);
            }
            else
            {
                Console.WriteLine("{0}: message #{1} has been expunged.", folder, e.Index);
            }
        }

        void OnMessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
        {
            var folder = (ImapFolder)sender;

            Console.WriteLine("{0}: flags have changed for message #{1} ({2}).", folder, e.Index, e.Flags);
        }

        public void Exit()
        {
            cancel.Cancel();
        }

        public void Dispose()
        {
            client.Dispose();
            cancel.Dispose();
        }
    }
}

