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
using System.ComponentModel;
using System.Collections.ObjectModel;
using Renci.SshNet;


namespace GUIapp
{
    
    public partial class Terminal : Page
    {
        ConsoleContent dc;
        SshClient client;
        ShellStream rw;
        public Terminal(SshClient client)
        {
            InitializeComponent();
            client.ConnectionInfo.Encoding = System.Text.ASCIIEncoding.ASCII;
            rw = client.CreateShellStream("dumb", 80, 24, 640, 300, 1024);
            dc = new ConsoleContent(rw);
            DataContext = dc;
            Loaded += MainWindow_Loaded;
            this.client = client;
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InputBlock.KeyDown += InputBlock_KeyDown;
            InputBlock.Focus();
            InputBlock.Select(InputBlock.Text.Length, 0);
        }

        void InputBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dc.ConsoleInput = InputBlock.Text;
                dc.RunCommand();
                InputBlock.Focus();
                InputBlock.Select(InputBlock.Text.Length, 0);
                Scroller.ScrollToBottom();
            }
        }
    }
    public class ConsoleContent : INotifyPropertyChanged
    {
        string consoleInput = string.Empty;
        ObservableCollection<string> consoleOutput = new ObservableCollection<string>();
        
        System.IO.StreamReader reader;
        System.IO.StreamWriter writer;
        ShellStream rw;

       
        public ConsoleContent(ShellStream rw)
        {
            this.rw = rw;
            reader = new System.IO.StreamReader(rw);
            writer = new System.IO.StreamWriter(rw);
            writer.AutoFlush = true;

            System.Threading.Thread.Sleep(1000);
            List<string> buf = new List<string>(reader.ReadToEnd().Split('\n'));
            foreach (var str in buf.GetRange(0, buf.Count - 1))
                ConsoleOutput.Add(str.Substring(0, str.Length - 1));
            ConsoleInput = buf.Last();
        }

        public string ConsoleInput
        {
            get
            {
                return consoleInput;
            }
            set
            {
                consoleInput = value;
                OnPropertyChanged("ConsoleInput");
            }
        }

        public ObservableCollection<string> ConsoleOutput
        {
            get
            {
                return consoleOutput;
            }
            set
            {
                consoleOutput = value;
                OnPropertyChanged("ConsoleOutput");
            }
        }

        public void RunCommand()
        {
            ConsoleOutput.Add(ConsoleInput);
            List<string> result;
            writer.WriteLine(ConsoleInput.Substring(ConsoleInput.IndexOf("#") + 1));
            long i = 0;
            do
            {
                i = rw.Length;
                System.Threading.Thread.Sleep(300);
                result = new List<string>(reader.ReadToEnd().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
             
            } while (rw.Length != i);
            result.RemoveRange(0, 1);
            foreach (var str in result.GetRange(0, result.Count - 2))
            {
                ConsoleOutput.Add(str.Substring(0, str.Length - 1));
                LogWriter lg = new LogWriter(str);

            }
            ConsoleInput = result.Last();
        }
        public void WriteLog(string log)
        {
            System.IO.File.WriteAllText("logfile", log);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
