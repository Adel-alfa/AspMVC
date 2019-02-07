using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public sealed class Log: ILog
    {
        private Log()
        {

        }
        private static readonly Lazy<Log> instance = new Lazy<Log>(() => new Log());
        public static Log GetInstance
        {
            get
            {
                return instance.Value;
            }
        }

        public void LogException(string message)
        {
            //string logFile  = string.Format("{0}exception-{1:yyyy-MM-dd_hh-mm-ss-tt}.log",
            //                          AppDomain.CurrentDomain.BaseDirectory,
            //                          DateTime.Now);
            string logFile  = string.Format("{0}exception-{1:yyyy-MM-dd_hh-mm-ss-tt}.log", AppDomain.CurrentDomain.BaseDirectory, DateTime.Now);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("---------------------------------------------");
            sb.AppendLine(DateTime.Now.ToString());
            sb.AppendLine("---------------------------------------------");
            sb.AppendLine(message);
            using (StreamWriter Swriter = new StreamWriter(logFile, true))
            {
                Swriter.Write(sb.ToString());
                Swriter.Flush();
                Swriter.Close();
            }

        }
    }
}
