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
using System.Windows.Shapes;
using Renci.SshNet;
using System.IO;

namespace GUIapp.Functional
{
    /// <summary>
    /// Interaction logic for Uploader.xaml
    /// </summary>
    public partial class Uploader : Window
    {
        ConnectionInfo Info;
        string password;
        string remotePath = "";
        public Uploader(ConnectionInfo info, string Path, string pass)
        {
            InitializeComponent();
            this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth / 2) - (this.Width / 2);
            this.Top = (System.Windows.SystemParameters.PrimaryScreenHeight / 2) - (this.Height / 2);
            Info = info;
            password = pass;
            remotePath = Path;
            remotePath += "/";
        }

        private void Path_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "File";
            dlg.DefaultExt = ".";
            dlg.Filter = "All|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                PathField.Text = dlg.FileName;
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            using (var sftp = new SftpClient(Info.Host, Info.Username, password))
            {
                sftp.Connect();
                string FilePath = PathField.Text;
                string remoteFileName = remotePath + PathField.Text.Substring(PathField.Text.LastIndexOf("\\") + 1);
                
                Stream fs = File.OpenRead(FilePath);
                sftp.UploadFile(fs, remoteFileName);

                fs.Close();
                sftp.Disconnect();
            }
            this.Close();
        }
    }
}
