using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace Chat
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        TcpClient client;
        string Username = string.Empty;
        //private string messageType;

        private void btnVerbinden_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Username = tbNameInput.Text;

                var ipAddress = IPAddress.Parse(tbIPAdressInput.Text);
                client = new TcpClient();
                client.Connect(ipAddress, 5000);

                tbTextSenden.IsEnabled = true;
                lblStatus.Foreground = Brushes.Green;
                lblStatus.Content = "ONLINE";
                tblockChat.Text = "";
                var thread = new Thread(() => ReceiveData(client));
                thread.Start();
                SendData(string.Format("connect|{0} ist online!", Username));
            }
            catch(FormatException)
            {
                MessageBox.Show("Ungültige IP-Adresse");
            }
            catch(SocketException)
            {
                MessageBox.Show("Server nicht erreichbar");
            }
        }

        private void tbTextSenden_Click(object sender, RoutedEventArgs e)
        {
            var messageText = string.Format("message|{0}|{1}", Username, tbEingabe.Text);
            tbEingabe.Text = string.Empty;
            SendData(messageText);

        }

        private void SendData(string messageText)
        {
            var stream = client.GetStream();

            var messageTextBytes = Encoding.UTF8.GetBytes(messageText);
            stream.Write(messageTextBytes, 0, messageTextBytes.Length);
        }

        private void ReceiveData(TcpClient client)
        {
            while (true)
            {
                var Stream = client.GetStream();

                var buffer = new byte[1024];
                var byteCount = Stream.Read(buffer, 0, buffer.Length);

                if (byteCount > 0)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    data = data.Replace("\0", string.Empty);

                    var messageParts = data.Split('|');

                    switch (messageParts[0])
                    {
                        case "message":
                            Dispatcher.Invoke(() =>
                            {
                                tblockChat.Text += messageParts[1] + ": " + messageParts[2] + Environment.NewLine;
                            }); break;
                        case "userCount":
                            Dispatcher.Invoke(() =>
                            {
                                lblUserCounter.Content = messageParts[1];
                            }); break;
                        case "connect":
                            Dispatcher.Invoke(() =>
                            {
                                tblockChat.Text += messageParts[1] + Environment.NewLine;
                            }); break;
                        case "user_list":
                            var userList = messageParts[1].Split(',');
                            string userListText = string.Empty;
                            foreach (var userName in userList)
                            {
                                userListText += userName + Environment.NewLine;
                            }
                            Dispatcher.Invoke(() =>
                            {
                                listboxUser.DataContext = userListText;
                            }); break;
                        default: break;
                    }
                }
            }
        }
        
    }
}
