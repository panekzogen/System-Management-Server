using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
namespace GUIapp
{
    class LogWriter
    {
        static Mutex mutexObj = new Mutex();
        static Thread LogThread;
        static string FilePath = System.DateTime.Today.Day.ToString() + "." + System.DateTime.Today.Month.ToString() + "." + System.DateTime.Today.Year.ToString() + "_" + "log";
        public LogWriter(string str)
        {
            
              LogThread = new Thread(new ParameterizedThreadStart(Write));
                LogThread.Start(str);
            
        }
        public  static void Write(object str)
        {
            mutexObj.WaitOne();
                
                File.AppendAllText(FilePath, ((string)str)+"\n");
           
            mutexObj.ReleaseMutex();
        }

    }
}
