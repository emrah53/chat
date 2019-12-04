using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        private void btnVerbinden_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ipAddress = IPAddress.Parse(tbIPAdressInput.Text);
                client = new TcpClient();
                client.Connect(ipAddress, 5000);

                tbTextSenden.IsEnabled = true;
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
            var messageText = tbEingabe.Text;
            tbEingabe.Text = string.Empty;

            var stream = client.GetStream();

            var messageTextBytes = Encoding.ASCII.GetBytes(messageText);
            stream.Write(messageTextBytes, 0 , messageTextBytes.Length);
        }
    }
}
