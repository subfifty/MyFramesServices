using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Microsoft.Web.Administration;
using Microsoft.Win32;
using System.Runtime;

namespace XpRestApiInstaller
{
    class Program
    {
        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }

        private static string m_InstallDir = null;
        private static string XPhoneInstallDir
        {
            get
            {
                if (m_InstallDir == null)
                {
                    try
                    {
                        using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("Software\\C4B\\XPhoneServer"))
                        {
                            m_InstallDir = (string)regKey.GetValue("InstallDir");
                            return m_InstallDir;
                        }
                    }
                    catch
                    {
                        string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                        m_InstallDir = Path.Combine(ProgramData, "C4B\\" + "XPhone Connect Server");
                        Directory.CreateDirectory(m_InstallDir);
                    }
                }

                return m_InstallDir;
            }
        }

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        
        private static void VerifyConfigFileExists(string targetPath)
        {
            string targetFile = Path.Combine(targetPath, "config.xml");
            string sourceFile = Path.Combine(targetPath, "config.xml.install");
            try
            {
                if (!File.Exists(targetFile))
                {
                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, targetFile, true);
                    }
                }
            }
            catch { }
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string sourceFile in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                string targetFile = sourceFile.Replace(sourcePath, targetPath);

                File.Copy(sourceFile, targetFile, true);

                FileAttributes attributes = File.GetAttributes(targetFile);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    attributes = RemoveAttribute(attributes, FileAttributes.ReadOnly);
                    File.SetAttributes(targetFile, attributes);
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("XPhone MyFrames Installer");
                Console.WriteLine("=========================");
                Console.WriteLine("");
                Console.WriteLine("If you proceed, this program will modify your system:");
                Console.WriteLine("   - Install/Update IIS ApplicationPool 'XPhoneConnectApi'");
                Console.WriteLine("   - Install/Update IIS ApplicationPool 'XPhoneConnectMyFramesApi'");
                if (Directory.Exists(Path.Combine(AssemblyDirectory, "PresenceApi")))
                    Console.WriteLine("   - Install/Update REST service 'PresenceApi'");
                if (Directory.Exists(Path.Combine(AssemblyDirectory, "RestApi")))
                    Console.WriteLine("   - Install/Update REST service 'RestApi'");
                if (Directory.Exists(Path.Combine(AssemblyDirectory, "Powershell")))
                    Console.WriteLine("   - Install/Update Powershell scripts used by 'RestApi'");
                if (Directory.Exists(Path.Combine(AssemblyDirectory, "Applink")))
                    Console.WriteLine("   - Install/Update Applink Resources (e.g. graphics)");
                if (Directory.Exists(Path.Combine(AssemblyDirectory, "ApiLicense")))
                    Console.WriteLine("   - Install/Update REST API license");
                if (Directory.Exists(Path.Combine(AssemblyDirectory, "ApiConfig")))
                    Console.WriteLine("   - Install/Update REST API configuration (keep existing config.xml)");
                if (Directory.Exists(Path.Combine(AssemblyDirectory, "MyFrames")))
                    Console.WriteLine("   - Install/Update MyFrames Framework");
                Console.WriteLine("");
                Console.WriteLine("Target directory: '" + XPhoneInstallDir + "'");
                Console.WriteLine("");
                Console.WriteLine("Press <Enter> to continue installation/update:");
                Console.ReadKey();
                Console.WriteLine("");

                //Console.WriteLine("XPhone Program Directory: \t" + XPhoneInstallDir);
                //Console.WriteLine("Installation Directory: \t" + AssemblyDirectory);

                if (Directory.Exists(XPhoneInstallDir) == false)
                {
                    Console.WriteLine("\r\nERROR: XPhone Program Directory not found!");
                    Console.WriteLine("\r\nPress <Enter> to terminate");
                    Console.ReadKey();
                    return;
                }

                ServerManager serverManager = new ServerManager();

                ApplicationPool appPoolApi = null;
                ApplicationPool appPoolMyFramesSites = null;
                ApplicationPool appPoolMyFramesApi = null;
                try
                {
                    appPoolApi = serverManager.ApplicationPools.Add("XPhoneConnectApi");
                }
                catch
                {
                    appPoolApi = serverManager.ApplicationPools["XPhoneConnectApi"];
                }
                appPoolApi.ManagedRuntimeVersion = "";

                try
                {
                    appPoolMyFramesSites = serverManager.ApplicationPools.Add("MyFramesSites");
                }
                catch
                {
                    appPoolMyFramesSites = serverManager.ApplicationPools["MyFramesSites"];
                }

                try
                {
                    appPoolMyFramesApi = serverManager.ApplicationPools.Add("MyFramesApi");
                }
                catch
                {
                    appPoolMyFramesApi = serverManager.ApplicationPools["MyFramesApi"];
                }
                appPoolMyFramesApi.ManagedRuntimeVersion = "";

                Console.WriteLine("...Application Pool: \t\t" + appPoolApi.Name);
                Console.WriteLine("...Application Pool: \t\t" + appPoolMyFramesSites.Name);
                Console.WriteLine("...Application Pool: \t\t" + appPoolMyFramesApi.Name);

                try { appPoolApi.Stop(); } catch { }
                try { appPoolMyFramesApi.Stop(); } catch { }
                try { appPoolMyFramesSites.Stop(); } catch { }

                Site site = serverManager.Sites.First(s => s.Id >= 1);

                Application app = null;
                string apiDir;

                apiDir = "PresenceApi";
                if (Directory.Exists(Path.Combine(AssemblyDirectory, apiDir)))
                {
                    Console.WriteLine("...Install/Update: \t\t" + apiDir);
                    try
                    {
                        if (Directory.Exists(Path.Combine(XPhoneInstallDir, apiDir)))
                            Directory.Delete(Path.Combine(XPhoneInstallDir, apiDir), true);
                    }
                    catch
                    {
                    }
                    Directory.CreateDirectory(Path.Combine(XPhoneInstallDir, apiDir));
                    CopyFilesRecursively(Path.Combine(AssemblyDirectory, apiDir), Path.Combine(XPhoneInstallDir, apiDir));

                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect", XPhoneInstallDir);
                    }
                    catch
                    {
                    }

                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect/" + apiDir, Path.Combine(XPhoneInstallDir, apiDir));
                    }
                    catch
                    {
                        app = site.Applications.First(s => s.Path == "/XPhoneConnect/" + apiDir);
                    }

                }

                apiDir = "RestApi";
                if (Directory.Exists(Path.Combine(AssemblyDirectory, apiDir)))
                {
                    Console.WriteLine("...Install/Update: \t\t" + apiDir);
                    try
                    {
                        if (Directory.Exists(Path.Combine(XPhoneInstallDir, apiDir)))
                            Directory.Delete(Path.Combine(XPhoneInstallDir, apiDir), true);
                    }
                    catch
                    {
                    }
                    Directory.CreateDirectory(Path.Combine(XPhoneInstallDir, apiDir));
                    CopyFilesRecursively(Path.Combine(AssemblyDirectory, apiDir), Path.Combine(XPhoneInstallDir, apiDir));
                    
                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect", XPhoneInstallDir);
                    }
                    catch
                    {
                    }

                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect/" + apiDir, Path.Combine(XPhoneInstallDir, apiDir));
                    }
                    catch
                    {
                        app = site.Applications.First(s => s.Path == "/XPhoneConnect/" + apiDir);
                    }
                    app.ApplicationPoolName = appPoolApi.Name;
                }

                apiDir = "Powershell";
                if (Directory.Exists(Path.Combine(AssemblyDirectory, apiDir)))
                {
                    Console.WriteLine("...Install/Update: \t\t" + apiDir);
                    string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    Directory.CreateDirectory(Path.Combine(ProgramData, "C4B\\" + apiDir));
                    CopyFilesRecursively(Path.Combine(AssemblyDirectory, apiDir), Path.Combine(ProgramData, "C4B\\" + apiDir));
                }

                apiDir = "Applink";
                if (Directory.Exists(Path.Combine(AssemblyDirectory, apiDir)))
                {
                    Console.WriteLine("...Install/Update: \t\t" + apiDir);
                    //Directory.CreateDirectory(Path.Combine(XPhoneInstallDir, apiDir));
                    CopyFilesRecursively(Path.Combine(AssemblyDirectory, apiDir), Path.Combine(XPhoneInstallDir, apiDir));
                }

                apiDir = "ApiLicense";
                if (Directory.Exists(Path.Combine(AssemblyDirectory, apiDir)))
                {
                    Console.WriteLine("...Install/Update: \t\t" + apiDir);
                    string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    Directory.CreateDirectory(Path.Combine(ProgramData, "C4B\\" + apiDir));
                    CopyFilesRecursively(Path.Combine(AssemblyDirectory, apiDir), Path.Combine(ProgramData, "C4B\\" + apiDir));
                }

                apiDir = "ApiConfig";
                if (Directory.Exists(Path.Combine(AssemblyDirectory, apiDir)))
                {
                    Console.WriteLine("...Install/Update: \t\t" + apiDir);
                    string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    Directory.CreateDirectory(Path.Combine(ProgramData, "C4B\\" + apiDir));
                    CopyFilesRecursively(Path.Combine(AssemblyDirectory, apiDir), Path.Combine(ProgramData, "C4B\\" + apiDir));
                    VerifyConfigFileExists(Path.Combine(ProgramData, "C4B\\" + apiDir));
                }

                apiDir = "MyFrames";
                if (Directory.Exists(Path.Combine(AssemblyDirectory, apiDir)))
                {
                    Console.WriteLine("...Install/Update: \t\t" + apiDir);
                    try
                    {
                        if (Directory.Exists(Path.Combine(XPhoneInstallDir, apiDir)))
                            Directory.Delete(Path.Combine(XPhoneInstallDir, apiDir), true);
                    }
                    catch
                    {
                    }
                    Directory.CreateDirectory(Path.Combine(XPhoneInstallDir, apiDir));
                    CopyFilesRecursively(Path.Combine(AssemblyDirectory, apiDir), Path.Combine(XPhoneInstallDir, apiDir));

                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect", XPhoneInstallDir);
                    }
                    catch
                    {
                    }

                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect/" + apiDir, Path.Combine(XPhoneInstallDir, apiDir));
                    }
                    catch
                    {
                        app = site.Applications.First(s => s.Path == "/XPhoneConnect/" + apiDir);
                    }

                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect/" + apiDir + "/TeamsApp", Path.Combine(Path.Combine(XPhoneInstallDir, apiDir), "TeamsApp"));
                    }
                    catch
                    {
                        app = site.Applications.First(s => s.Path == "/XPhoneConnect/" + apiDir + "/TeamsApp");
                    }

                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect/" + apiDir + "/presence", Path.Combine(Path.Combine(XPhoneInstallDir, apiDir), "presence"));
                    }
                    catch
                    {
                        app = site.Applications.First(s => s.Path == "/XPhoneConnect/" + apiDir + "/presence");
                    }
                    app.ApplicationPoolName = appPoolMyFramesSites.Name;

                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect/" + apiDir + "/xphone", Path.Combine(Path.Combine(XPhoneInstallDir, apiDir), "xphone"));
                    }
                    catch
                    {
                        app = site.Applications.First(s => s.Path == "/XPhoneConnect/" + apiDir + "/xphone");
                    }
                    app.ApplicationPoolName = appPoolMyFramesSites.Name;

                    app = null;
                    try
                    {
                        app = site.Applications.Add("/XPhoneConnect/" + apiDir + "/api", Path.Combine( Path.Combine(XPhoneInstallDir, apiDir), "api") );
                    }
                    catch
                    {
                        app = site.Applications.First(s => s.Path == "/XPhoneConnect/" + apiDir + "/api");
                    }
                    app.ApplicationPoolName = appPoolMyFramesApi.Name;
                }

                serverManager.CommitChanges();

                try { appPoolApi.Start(); } catch { }
                try { appPoolMyFramesApi.Start(); } catch { }
                try { appPoolMyFramesSites.Start(); } catch { }

            }
            finally
            {
                Console.WriteLine("\r\nPress <Enter> to exit.");
                Console.ReadKey();
            }
        }
    }
}
