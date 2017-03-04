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
using System.IO;
using Renci.SshNet;

namespace GUIapp
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        MainWindow window;
        SshClient client;
        ShellStream rw;
        StreamReader reader;
        StreamWriter writer;
        string password;

        string buffer = null;
        
        public MainPage(MainWindow w, SshClient client, string pass)
        {
            InitializeComponent();
            window = w;
            window.Title = "Management system for server";
            password = pass;

            this.client = client;
            client.ConnectionInfo.Encoding = System.Text.ASCIIEncoding.ASCII;
            rw = client.CreateShellStream("dumb", 80, 24, 640, 300, 1024);
            reader = new StreamReader(rw);
            writer = new StreamWriter(rw);
            writer.AutoFlush = true;

            SessionInfo.Content = client.ConnectionInfo.Host + " :: " + client.ConnectionInfo.Username;
        }

        private void Logo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Output.ItemsSource = null;
            SetColumns(null, "Home page");
            SetButtons(null);
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            rw.Dispose();
            client.Disconnect();
            window.toLoginPage();
        }

        private void SetColumns(List<string> columns, string pageInfo)
        {
            OutputColumns.Columns.Clear();
            PageInfo.Content = pageInfo;
            if (columns != null)
                foreach (var column in columns)
                {
                    OutputColumns.Columns.Add(new GridViewColumn()
                    {
                        Header = column,
                        Width = (Output.Width - 20) / columns.Count,
                        DisplayMemberBinding = new Binding(column)
                    });
                }
        }
        private void SetButtons(List<string> buttons)
        {
            ButtonsContainer.Items.Clear();
            if (buttons != null)
            {
                Image imgButton;
                foreach (var button in buttons)
                {
                    imgButton = new Image()
                    {
                        Name = button,
                        Width = 32,
                        Height = 32,
                        Cursor = Cursors.Hand,
                        Margin = new Thickness(6, 6, 6, 6),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        ToolTip = button,
                        Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName + "./Pictures/controls/" + button))
                    };
                    imgButton.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.ImageButton_MouseDoubleClick);
                    ButtonsContainer.Items.Add(imgButton);
                }
            }                   
        }
        
        private void ImageButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(FileManager.IsEnabled)
            {
                switch((sender as Image).Name)
                {
                    case "Rename":
                        Functional.TextField newNameInput = new Functional.TextField("Enter new filename");
                        newNameInput.FieldValue = (Output.SelectedItem as FileManager.File).Filename;
                        newNameInput.Height -= 15;
                        newNameInput.ShowDialog();
                        client.RunCommand("mv " + StatusBarContent.Text + "/" + (Output.SelectedItem as FileManager.File).Filename + " " + StatusBarContent.Text + "/" + newNameInput.FieldValue);
                        break;
                    case "Create":
                        Functional.TextField nameInput = new Functional.TextField("Directory or file name");
                        nameInput.FieldValue = "NewFile";
                        nameInput.ShowDialog();                        
                        if (nameInput.Directory.IsChecked == true)
                            client.RunCommand("mkdir " + StatusBarContent.Text + "/" + nameInput.FieldValue);
                        else
                            client.RunCommand("touch " + StatusBarContent.Text + "/" + nameInput.FieldValue);
                        break;
                    case "Edit":
                        if ((Output.SelectedItem as FileManager.File).Permissions[0] == 'd' || Output.SelectedIndex < 0) break;
                        Functional.TextEditor editor = new Functional.TextEditor(client.ConnectionInfo, StatusBarContent.Text + "/" + (Output.SelectedItem as FileManager.File).Filename, password);
                        editor.ShowDialog();
                        break;
                    case "Info":
                        if (Output.SelectedIndex >= 0)
                            MessageBox.Show(client.RunCommand("stat " + StatusBarContent.Text + "/" + (Output.SelectedItem as FileManager.File).Filename).Result, "Information");
                        break;
                    case "Delete":
                        if (Output.SelectedIndex >= 0) 
                            using (var sftp = new SftpClient(client.ConnectionInfo.Host, client.ConnectionInfo.Username, password))
                            {
                                sftp.Connect();
                                FileManager.File inf = Output.SelectedItem as FileManager.File;
                                sftp.Delete(StatusBarContent.Text + "/" + inf.Filename);
                                sftp.Disconnect();
                            }
                        break;
                    case "Paste":
                        if ( buffer != "" )
                        {
                            if ( buffer[0] == 'c' )
                            {
                                client.RunCommand("cp " + buffer.Substring(1) + " " + StatusBarContent.Text);
                            }
                            else
                            {
                                client.RunCommand("mv " + buffer.Substring(1) + " " + StatusBarContent.Text + buffer.Substring(buffer.LastIndexOf("/")));
                            }
                            buffer = "";
                        }
                        break;
                    case "Copy":
                        buffer = "c" + StatusBarContent.Text + "/" + (Output.SelectedItem as FileManager.File).Filename;
                        break;
                    case "Cut":
                        buffer = "r" + StatusBarContent.Text + "/" + (Output.SelectedItem as FileManager.File).Filename;
                        break;
                    case "Download":
                        if(Output.SelectedIndex >= 0)
                            if ((Output.SelectedItem as FileManager.File).Permissions[0] != 'd')
                            {
                                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

                                saveFileDialog.Filter = "All files (*.*)|*.*";
                                saveFileDialog.FilterIndex = 1;
                                saveFileDialog.RestoreDirectory = true;
                                saveFileDialog.FileName = (Output.SelectedItem as FileManager.File).Filename;

                                saveFileDialog.ShowDialog();

                                using (var sftp = new SftpClient(client.ConnectionInfo.Host, client.ConnectionInfo.Username, password))
                                {
                                    sftp.Connect();
                                    string FilePath = saveFileDialog.FileName;
                                    string remoteFileName = StatusBarContent.Text + "/" + (Output.SelectedItem as FileManager.File).Filename;

                                    Stream fs = File.OpenWrite(FilePath);
                                    sftp.DownloadFile(remoteFileName, fs);

                                    fs.Close();
                                    sftp.Disconnect();
                                }
                            }
                            else MessageBox.Show("Files Only");
                        break;
                    case "Upload":
                        Functional.Uploader upd = new Functional.Uploader(client.ConnectionInfo, StatusBarContent.Text, password);                    
                        upd.ShowDialog();
                        break;
                    default:
                        break;
                }
                FileManager fm = new FileManager(reader, writer);
                if (StatusBarContent.Text == "") Output.ItemsSource = fm.GetFilesList("/");
                else Output.ItemsSource = fm.GetFilesList(StatusBarContent.Text);
            }
        }
        
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileManager.IsEnabled)
            {
                FileManager fm = new FileManager(reader, writer);
                var item = ((FrameworkElement)e.OriginalSource).DataContext as FileManager.File;
                if (item != null)
                {
                    if (item.Permissions[0] == 'd')
                    {
                        Output.ItemsSource = fm.GetFilesList(item.Filename);
                        if (item.Filename != "..") StatusBarContent.Text = StatusBarContent.Text + "/" + item.Filename;
                        else StatusBarContent.Text = StatusBarContent.Text.Substring(0, StatusBarContent.Text.LastIndexOf('/'));
                    }
                }
            }
        }

        private void FileManager_Selected(object sender, RoutedEventArgs e)
        {
            StatusBarContent.Text = "";
            SetColumns(new List<string>(new string[] 
                    { 
                        "Filename",
                        "Modified",
                        "Size",
                        "Group",
                        "Owner",
                        "Links",
                        "Permissions" 
                    }), "File manager");
            SetButtons(new List<string>(new string[] 
                    { 
                        "Upload",
                        "Download",
                        "Cut",
                        "Copy",
                        "Paste",
                        "Delete",
                        "Info",
                        "Edit",
                        "Create",
                        "Rename"
                    }));
            FileManager fm = new FileManager(reader, writer);
            Output.ItemsSource = fm.GetFilesList("/");
        }
        private void Administrators_Selected(object sender, RoutedEventArgs e)
        {

        }
        public class User
        {
            public string USER { get; set; }
            public string TTY { get; set; }
            public string FROM { get; set; }
            public string LOGIN { get; set; }
            public string IDLE { get; set; }
            public string JCPU { get; set; }
            public string PCPU { get; set; }
            public string WHAT { get; set; }
        }

        public int ext(User a, List<User> users)
        {
            if (users.IndexOf(a) > 0)
                return 1;
            else
                return 0;
        }
        public string deletespaces(string str)
        {
            int k = 0;
            while (str[k] == ' ')
            {
                k++;
            }
            str = str.Substring(k);
            return str;
        }
        private void Users_Selected(object sender, RoutedEventArgs e)
        {
            List<User> users = new List<User>();
            SetColumns(new List<string>(new string[] 
                    { 
                        "USER","TTY","FROM","LOGIN","IDLE","JCPU","PCPU","WHAT"
                    }), "Users");
            SetButtons(null);
            writer.Flush();
            writer.WriteLine("w");
            System.Threading.Thread.Sleep(300);
            string inf = reader.ReadToEnd();
            inf = inf.Substring(inf.IndexOf("WHAT") + 6);
            while (true)
            {
                User a = new User();
                try
                {
                    a.USER = inf.Substring(0, inf.IndexOf(' '));
                }
                catch
                {
                    break;
                }
                inf = inf.Substring(inf.IndexOf(' ') + 1);
                inf = deletespaces(inf);
                a.TTY = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                int j = 0;
                for (int i = 0; i < inf.Length; i++)
                {
                    if (inf[i] == ' ')
                        j++;
                    else
                        break;
                    if (j > 5)
                    {
                        a.FROM = "  ";
                        break;
                    }
                }
                if (j < 5)
                {
                    inf = deletespaces(inf);
                    a.FROM = inf.Substring(0, inf.IndexOf(' '));
                    inf = inf.Substring(inf.IndexOf(' '));
                }
                inf = deletespaces(inf);
                a.LOGIN = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.IDLE = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.JCPU = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.PCPU = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.WHAT = inf.Substring(0, inf.IndexOf('\n'));

                inf = inf.Substring(inf.IndexOf('\n') + 1);
                users.Add(a);
                if (inf.Length < 40)
                    break;
            }
            Output.ItemsSource = users;
        }

        public class Monitor
        {
            public string Name { get; set; }
            public string Value { get; set; }

        }
        private void Monitor_Selected(object sender, RoutedEventArgs e)
        {

            List<Monitor> monitor = new List<Monitor>();
            SetColumns(new List<string>(new string[] 
                    { 
                        "Name", "Value"
                    }), "Monitor");
            SetButtons(null);
            //        writer.WriteLine("cat /proc/cpuinfo");
            string contain;
            int p;


            writer.Flush();
            reader.ReadToEnd();
            writer.WriteLine("cat /proc/cpuinfo");
            System.Threading.Thread.Sleep(300);
            string contain2 = reader.ReadToEnd();
            contain2 = contain2.Substring(19);
            contain2 = contain2.Substring(0, contain2.IndexOf(client.ConnectionInfo.Username));

            int i = 0;
            while (i < 22)
            {
                Monitor a = new Monitor();

                a.Name = contain2.Substring(0, contain2.IndexOf(':') - 1);
                contain2 = contain2.Substring(contain2.IndexOf(':') + 1);
                a.Value = contain2.Substring(0, contain2.IndexOf('\n') - 1);
                contain2 = contain2.Substring(contain2.IndexOf('\n') + 1);
                monitor.Add(a);
                i++;
            }


            Output.ItemsSource = monitor;



        }
        private void Databases_Selected(object sender, RoutedEventArgs e)
        {

        }
        private void Services_Selected(object sender, RoutedEventArgs e)
        {

        }
        private void Reboot_Selected(object sender, RoutedEventArgs e)
        {

        }
      
        private void Traffic_Selected(object sender, RoutedEventArgs e)
        {

        }
        private void ErrorLog_Selected(object sender, RoutedEventArgs e)
        {

        }
        private void Parameters_Selected(object sender, RoutedEventArgs e)
        {

        }
        public class Route
        {
            public string Destination { get; set; }
            public string Gateway { get; set; }
            public string Genmask { get; set; }
            public string Flags { get; set; }
            public string Metric { get; set; }
            public string Ref { get; set; }
            public string Use { get; set; }
            public string Iface { get; set; }
        }
        private void IPaddresses_Selected(object sender, RoutedEventArgs e)
        {
            List<Route> route = new List<Route>();
            SetColumns(new List<string>(new string[] 
                    { 
                        "Destination","Gateway","Genmask","Flags","Metric","Ref","Use","Iface"
                    }), "Route");
            SetButtons(null);
            writer.Flush();
            writer.WriteLine("route");
            System.Threading.Thread.Sleep(100);
            string inf = reader.ReadToEnd();
            inf = inf.Substring(inf.IndexOf("Iface") + 7);
            while (true)
            {
                Route a = new Route();
                try
                {
                    a.Destination = inf.Substring(0, inf.IndexOf(' '));
                }
                catch
                {
                    break;
                }
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.Gateway = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.Genmask = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.Flags = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.Metric = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.Ref = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.Use = inf.Substring(0, inf.IndexOf(' '));
                inf = inf.Substring(inf.IndexOf(' '));
                inf = deletespaces(inf);
                a.Iface = inf.Substring(0, inf.IndexOf('\n'));
                inf = inf.Substring(inf.IndexOf('\n') + 1);
                route.Add(a);
                if (inf.Length < 40)
                    break;
            }
            Output.ItemsSource = route;
        }
        private void Features_Selected(object sender, RoutedEventArgs e)
        {

        }
        private void Email_Selected(object sender, RoutedEventArgs e)
        {

        }
        private void Reference_Selected(object sender, RoutedEventArgs e)
        {

        }

    }
    public class FileManager
    {
        public class File
        {
            public string Filename { get; set; }
            public string Modified { get; set; }
            public string Size { get; set; }
            public string Group { get; set; }
            public string Owner { get; set; }
            public string Links { get; set; }
            public string Permissions { get; set; }
            
        }
        public List<File> files = new List<File>();
        StreamReader reader;
        StreamWriter writer;
        public FileManager(StreamReader sr, StreamWriter sw)
        {
            reader = sr;
            writer = sw;
        }
        public List<File> GetFilesList(string Directory)
        {
            string textList;
            writer.WriteLine("cd " + Directory);
            writer.WriteLine("ls -al");
            System.Threading.Thread.Sleep(200);
            textList = reader.ReadToEnd();

            textList = textList.Substring(textList.IndexOf("total"));
            List<string> sepList = new List<string>(textList.Split('\n'));
            sepList.RemoveRange(sepList.Count - 2, 2);
            sepList.RemoveRange(0, 2);

            string[] bs;
            foreach (var elem in sepList)
            {
                bs = elem.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (bs[8][bs[8].Length - 1] == '\r') bs[8] = bs[8].Substring(0, bs[8].Length - 1);
                files.Add(new File()
                {
                    Filename = bs[8],
                    Modified = bs[5] + " " + bs[6] + " " + bs[7],
                    Size = bs[4],
                    Group = bs[3],
                    Owner = bs[2],
                    Links = bs[1],
                    Permissions = bs[0]
                });
            }
            return files;
        }
    }
}