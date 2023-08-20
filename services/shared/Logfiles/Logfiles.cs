using System.Collections.Generic;
using System.IO;
using System;

namespace XPhoneRestApi
{
    public class Logfiles
    {
        private static Dictionary<string, LogFile> logFiles;
        static Logfiles()
        {
            logFiles = new Dictionary<string, LogFile>();
            try
            {
                string[] files = Directory.GetFiles(RootDirectory);
                foreach (var file in files)
                {
                    string name = Path.GetFileNameWithoutExtension(file).ToLower();
                    string logging = ApiConfig.Instance.ReadAttributeValue(name, "Logging", "on").ToLower();
                    logFiles.Add(name, new LogFile(name, logging));
                }
            }
            catch { }
        }

        internal static string RootDirectory
        {
            get
            {
                return Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"C4B\LogFiles\");
            }
        }

        public static LogFile Find(string name)
        {
            name = name.ToLower();

            LogFile logFile = null;
            logFiles.TryGetValue(name, out logFile);
            if (logFile == null)
            {
                string logging = ApiConfig.Instance.ReadAttributeValue(name, "Logging", "on").ToLower();
                logFile = new LogFile(name, logging);
                logFiles.Add(name, logFile);
            }

            return logFile;
        }

        public static void Remove(string name)
        {
            name = name.ToLower();
            logFiles.Remove(name);
        }

        public static string List()
        {
            string result = "";

            foreach (var p in logFiles)
            {
                result = result + Path.GetFileNameWithoutExtension(p.Value.Name) + " = " + p.Value.Name + "\r\n";
            }

            return result;
        }
    }

    public class LogFile
    {
        private string m_Path;
        private string m_name;
        private string m_logging;
        public LogFile(string name, string logging = "on")
        {
            name = name.ToLower();
            m_name = name;
            m_Path = Path.Combine(Logfiles.RootDirectory, name + ".log");
            m_logging = logging;
            Init();
        }

        public string Name
        {
            get
            {
                return m_Path;
            }
        }

        private StreamWriter sw = null;
        private void Init()
        {
            try
            {
                if (sw != null)
                    sw.Close();

                // if the file doesn't exist, create it
                if (!File.Exists(m_Path))
                {
                    string p = Path.GetDirectoryName(m_Path);
                    Directory.CreateDirectory(p);
                    FileStream fs = File.Create(m_Path);
                    fs.Close();
                }

                // open up the streamwriter for writing..
                sw = File.AppendText(m_Path);
            }
            catch
            {

            }
        }

        public string ReadAll()
        {
            string txt = "";

            lock (sw)
            {
                sw.Close();
                txt = File.ReadAllText(m_Path);
                sw = File.AppendText(m_Path);
            }

            return txt;
        }

        public void Append(String message, bool WithTimeStamp = false)
        {
            if (m_logging.ToLower() != "on")
                return;

            try
            {
                lock (sw)
                {
                    if (WithTimeStamp)
                    {
                        string dt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        message = dt + " " + message;
                    }
                    sw.WriteLine(message);
                    sw.Flush();
                }
            }
            catch
            {
            }
        }

        public void Delete()
        {
            try
            {
                lock (sw)
                {
                    sw.Close();
                    File.Delete(m_Path);
                    Logfiles.Remove(m_name);
                    //Init();
                }
            }
            catch
            {
            }
        }
    }
}
