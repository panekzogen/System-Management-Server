using System;
using System.Collections.Generic;
using System.Linq;
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
using Renci.SshNet;

namespace GUIapp
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        MainWindow window;

        /*public delegate void ConnectEventHandler(object sender, ConnectEventArgs e);

        delegate void ShowMessageDelegate(string s);

        public event ConnectEventHandler Connection;*/
        public LoginPage(MainWindow w)
        {
            InitializeComponent();
            window = w;
            window.Title = "Connection";
            this.IPField.Text = "195.50.20.251";
            this.PortField.Text = "22";
            this.UsernameField.Text = "root";
            this.PassField.Password = "hgu77wqq";
            /*this.Connection += new ConnectEventHandler(this.ShowConnectionResult);

            this.Connection += delegate(object sender, ConnectEventArgs e)
            {
                LoginPage temp = (LoginPage)sender;
                if (e.IsConnected)
                {
                    MessageBox.Show("Connected to " + temp.IPField.Text + "\nUsername: " + temp.UsernameField.Text);
                }
                else
                {
                    MessageBox.Show("Connection failed");
                }
            };*/
            

        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var client = new SshClient(this.IPField.Text, Convert.ToInt32(this.PortField.Text), this.UsernameField.Text, this.PassField.Password);
            client.Connect();
            /*ConnectEventArgs arg = new ConnectEventArgs();
            if (client.IsConnected)
            {
                arg.IsConnected = true;
                Connection(this, arg);
            }
            else
            {
                arg.IsConnected = false;
                Connection(this, arg);
            }*/
 
            if (this.Terminal.IsChecked == true)            
                window.toTerminal(client);            
            else
                window.toMainPage(client, this.PassField.Password);  
        }
        /*delegate void ShowMessage(string str);
        public void ShowConnectionResult(object sender, ConnectEventArgs e)
        {
            LoginPage temp = (LoginPage)sender;
            string s;
            ShowMessage ShowMessageBox;
            if (e.IsConnected)
            {
                s = "Connected to " + temp.IPField.Text +"\nUsername: " + temp.UsernameField.Text;
            }
            else
            {
                s = "Connection failed";
            }
            ShowMessageBox = (str) => { MessageBox.Show(str); };
            ShowMessageBox(s);
        }*/
    }

    /*public class ConnectEventArgs
    {
        public bool IsConnected;
        public string username;

    }*/
}
