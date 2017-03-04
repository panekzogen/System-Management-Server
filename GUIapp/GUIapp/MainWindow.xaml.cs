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
using System.Runtime.InteropServices;

namespace GUIapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.toLoginPage();
        }
        public void toLoginPage()
        {
            this.Height = 325;
            this.Width = 525;
            this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth / 2) - (this.Width / 2);
            this.Top = (System.Windows.SystemParameters.PrimaryScreenHeight / 2) - (this.Height / 2);
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            LoginPage lpage = new LoginPage(this);
            this.Content = lpage;
        }
        public void toMainPage(SshClient client, string pass)
        {
            this.Height = 768;
            this.Width = 1024;
            this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth / 2) - (this.Width / 2);
            this.Top = (System.Windows.SystemParameters.PrimaryScreenHeight / 2) - (this.Height / 2);
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            MainPage mpage = new MainPage(this, client, pass);
            this.Content = mpage;
        }
        public void toTerminal(SshClient client)
        {
            Terminal tpage = new Terminal(client);
            this.Height = 425;
            this.Width = 675;
            this.Content = tpage;
        }
    }
}
