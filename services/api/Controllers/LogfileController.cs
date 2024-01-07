using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XPhoneRestApi.Controllers
{
    //=======================================================================
    // Download Controller
    //=======================================================================
    [Route("[controller]")]
    [ApiController]
    public class DownloadController : XPhoneControllerBase
    {
        private async Task<Stream> GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(s);
            await writer.FlushAsync();
            stream.Position = 0;
            return stream;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Download(string name)
        {
            LogFile logFile = Logfiles.Find(name);

            Stream stream = await GenerateStreamFromString(logFile.ReadAll());

            if (stream == null)
               return NotFound(); // returns a NotFoundResult with Status404NotFound response.

            return File(stream, "application/octet-stream", name + ".log"); // returns a FileStreamResult
        }
    }

    //=======================================================================
    // Logfile Controller
    //=======================================================================
    [Route("[controller]")]
    [ApiController]
    [Authorize("logfile")]
    [AllowAnonymous]
    public class LogfileController : ControllerBase
    {
        private static LicenseObject ControllerLicense = ApiLicense.Instance.ParseLicenseObject("LogFile");

        private string ShowHelp()
        {
            string help =
                  @"/LogFile/{name}/{cmd}[?{options}]" + "\r\n" 
                + @"{name} Name der Logdatei ohne Extension. Ablage in '%CommonProgramData%\C4B\LogFiles\{name}.Log'" + "\r\n"
                + @"{cmd} list, append, read, delete, download";
            return help;
        }

        // GET
        [HttpGet]
        public string Get()
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            return ShowHelp();
        }

        // GET: cmd
        [HttpGet("{cmd}")]
        public string Get(string cmd)
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            if (!IsValidLicense())
                return "License not valid.";

            if ( cmd.ToLower() == "list" )
            {
                return Logfiles.List();
            }
            return Get("default", cmd);
        }

        // GET name/cmd
        [HttpGet("{name}/{cmd}")]
        public string Get(string name, string cmd)
        {
            //if (ApiConfig.Instance.RunningInDMZ())
            //    return ApiConfig.METHOD_NOT_SUPPORTED_IN_DMZ;

            if (!IsValidLicense())
                return "License not valid.";

            LogFile logFile = Logfiles.Find(name);
            switch (cmd)
            {
                case "append":
                    if (Request.QueryString.HasValue)
                    {
                        string q = this.Request.QueryString.ToString().Substring(1);
                        q = System.Web.HttpUtility.UrlDecode(q);
                        logFile.Append(q);
                    }
                    break;

                case "read":
                    return logFile.ReadAll();

                case "download":
                    this.Response.Redirect(this.Request.PathBase + "/" + cmd + "/" + name);
                    break;

                case "delete":
                    logFile.Delete();
                    break;
            }
            return "Logile: name = " + name + ",  cmd = " + cmd;
        }

        private bool IsValidLicense()
        {
            //return ControllerLicense.valid;

            // Logfile Controller is always allowed for now!
            return true;
        }
    }

#if LOGFILES
    #region class Logfiles
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
    #endregion

    #region class LogFile
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
                    if ( WithTimeStamp )
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
    #endregion
#endif
}
