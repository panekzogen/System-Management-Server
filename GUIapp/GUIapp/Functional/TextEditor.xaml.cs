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
using System.IO;
using Renci.SshNet;

namespace GUIapp.Functional
{
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : Window
    {
        SftpClient sftp;
        string filePath;
        public TextEditor(ConnectionInfo inf, string path, string password)
        {
            InitializeComponent();
            this.Title = path;
            this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth / 2) - (this.Width / 2);
            this.Top = (System.Windows.SystemParameters.PrimaryScreenHeight / 2) - (this.Height / 2);
            ContentField.Document.LineHeight = 1;
            sftp = new SftpClient(inf.Host, inf.Username, password);
            sftp.Connect();
            filePath = path;

            TextRange t = new TextRange(ContentField.Document.ContentStart, ContentField.Document.ContentEnd);
            Renci.SshNet.Sftp.SftpFileStream fs = sftp.OpenRead(filePath);
            t.Load(fs, System.Windows.DataFormats.Text);
            fs.Close();
            ContentField.Focus();
            ContentField.CaretPosition = ContentField.Document.ContentEnd;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindow sw = new SaveWindow();
            sw.ShowDialog();
            if (sw.action == true)
            {
                TextRange t = new TextRange(ContentField.Document.ContentStart, ContentField.Document.ContentEnd);
                sftp.DeleteFile(filePath);
                Renci.SshNet.Sftp.SftpFileStream fs = sftp.OpenWrite(filePath);
                t.Save(fs, System.Windows.DataFormats.Text);
                fs.Close();
            }
            sftp.Disconnect();
        }
    }
}