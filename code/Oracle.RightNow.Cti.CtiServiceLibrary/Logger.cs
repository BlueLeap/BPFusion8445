using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Oracle.RightNow.Cti.Logger;
namespace Blueleap.Finesse
{
    public sealed class Logger
    {
        private static volatile Logger instance;
        private static object syncRoot = new Object();

        //private String filepath = @"C:\BlueLeapLogs\";
        //private String fileName;
        //private FileStream fs;
        //private StreamWriter sw;

        private object lockObject = new object();

        private Logger()
        {

            //filepath = filepath + DateTime.Now.ToString("yyyyMMdd");

            //fileName = filepath + @"\client_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log";


            //if (!Directory.Exists(filepath))
            //{
            //    Directory.CreateDirectory(filepath);
            //}

            //if (File.Exists(fileName))
            //{
            //    fs = new FileStream(fileName, FileMode.Append, FileAccess.Write);
            //}
            //else
            //{
            //    fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write);
            //}
            //sw = new StreamWriter(fs, Encoding.Default);

        }


        public static Logger getInstance()
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                }
            }
            return instance;
        }

        public void write(String methodName, String msg)
        {
            lock (lockObject)
            {
                Oracle.RightNow.Cti.Logger.Logger.Log.Debug(string.Format("[{0} {1}]", methodName, msg));
            }
        }

    }
}
