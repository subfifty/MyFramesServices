using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using C4B.Atlas;
using C4B.Atlas.Cryptography;
using C4B.Atlas.VDir;
using C4B.Atlas.Visualization;
using C4B.Atlas.Visualization.VDir;
using C4B.Atlas.Visualization.Wcf;
using C4B.Atlas.Visualization.Wcf.Contracts.VDir;
using C4B.VDir.WebService.Models;
using C4B.GUI.Framework;
using C4B.GUI.Framework.Web;
using System.Text.RegularExpressions;
using System.Threading;
using C4B.Atlas.VDir.Mapping;
using C4B.Dashboard;

namespace C4B.VDir.WebService
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public bool IsDesignMode { get; set; }
        public bool IsDebug { get; set; }

        // hash of tree data, to detect config changes, that triggers a tree reload
        public static int TreeHashCode { get; set; }
        static ReaderWriterLock _configLock = new ReaderWriterLock();
        public DebugInfo CurrentDebugInfo
        {
            get
            {
                return HttpContext.Current.Items["DebugInfo"] as DebugInfo;
            }

            set
            {
                HttpContext.Current.Items["DebugInfo"] = value;
            }
        }

        public bool IsAdminSession
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session["IsAdminSesion"] == null)
                    return false;
                else
                    if (HttpContext.Current.Session["IsAdminSesion"] as bool? == true)
                        return true;
                    else
                        return false;
            }

            set
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Session["IsAdminSesion"] = true;
        }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // deny dasboard instance to access the admin UI
            if( HttpContext.Current.Request.Url.AbsolutePath.ToUpper().EndsWith("APPLINK2/DESIGNER"))
                Response.Redirect(Regex.Replace(HttpContext.Current.Request.Url.AbsolutePath, "applink2", "Applink", RegexOptions.IgnoreCase));
            //EnableConfigFilesWatcher();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            FRWClientConnector.Instance.Dispose();
        }

        protected void Application_Start()
        {
            // WCF-Kommunikation initialisieren.
            FRWClientConnector.Instance.GetBaseContractType
                += Instance_GetBaseContractType;

            FRWClientConnector.Instance.GetCredentials
                += Instance_GetCredentials;

            FRWClientConnector.Instance.GetSessionID
                += Instance_GetSessionID;

            AreaRegistration.RegisterAllAreas();

            #region register known types
            // Bekannte WCF-Typen für die Serialisierung registrieren.
            //   -> Grundlegende .NET-Typen.
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(DataSet));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(DictionaryEntry));

            //   -> Grundlegende Typen für die Client-Visualisierung.
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualActionInfo));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualActionInfoCollection));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualActionInfoPropertyCollection));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualActionResult));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualActionResultError));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualActionResultOk));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualActionResultWarning));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualFilter));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualFilterElement));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualFilterElementDateTime));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualFilterElementGuid));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualFilterElementInteger));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualFilterElementString));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualFilterFactory.FilterElementType));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualFilterService.Concat));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualFilterService.Operators));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualGenericParameter));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualListItem));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualObject));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualObject.ResourceType));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualObjectInfo));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualPagingColumnSortingInfo));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualPagingColumnSortingInfos));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualPagingInformation));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParamBoolean));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParamChangeListener));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParameterInfo));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParameterInfoCollection));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParamGeneric));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParamGuid));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParamObject));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParamObjectError));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParamObjectMultiSelect));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParamObjectWarning));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualParamString));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualResource));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualResourceCollection));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualResourceContainer));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualResourceTypes));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualServices.EnActionResult));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualServices.EnActionResultErrorCodes));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualServices.EnDisplayStyle));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualServices.EnResourceContainerResultCodes));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualServices.EnVisualInfo));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATVisualTransportContainer));

            ATVisualKnownTypesProvider.RegisterKnownType(typeof(Dictionary<string, string>));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(Dictionary<string, IATVisualFilterElement>));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(Dictionary<string, ATVisualGenericParameter>));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(Dictionary<string, ATVisualObjectInfo>));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(List<string>));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(List<IATVisualFilterElement>));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(List<ATVisualParamChangeListener>));

            //   -> Grundlegende Typen.
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(ATGuid));

            //   -> Typen bzgl. VDirWebClient.
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(VDWebClientAuthenticationType));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(VDWindowsAccountDetails.CredentialConditions));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(VDRecord));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(VDRecord[]));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(VDValue));
            ATVisualKnownTypesProvider.RegisterKnownType(typeof(VDColumn));

            ATVisualKnownTypesProvider.RegisterKnownType(typeof(List<DictionaryEntry>));
            #endregion

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            RegisterGlobalFilters(GlobalFilters.Filters);
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new LocalizationFilterAttribute());
        }

        private void Instance_GetBaseContractType(object sender, FRWClientConnector_EventArgsBaseContractType e)
        {
            e.BaseContractType
                = typeof(IVDContract);
        }

        private void Instance_GetCredentials(object sender, FRWClientConnector_EventArgsCredentials e)
        {
            TraceExtension.Info("start");
            if (HttpContext.Current != null)
            {
                FRWClientLoginData frwClientLoginData = (FRWClientLoginData)HttpContext.Current.Session["UserCredentials"];
                if (frwClientLoginData == null)
                {
                    TraceExtension.Warn("no UserCredentials set in session");
                    SetInternalAuthentication(HttpContext.Current.Session);
                    frwClientLoginData = (FRWClientLoginData)HttpContext.Current.Session["UserCredentials"];
                }

                e.Credentials = frwClientLoginData.Credentials;
                TraceExtension.Info("Credentials set");
            }
            TraceExtension.Info("stop");
        }

        private void Instance_GetSessionID(object sender, FRWClientConnector_EventArgsSessionID e)
        {
            //TraceExtension.Info("start");
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                FRWClientLoginData frwClientLoginData = null;
                if ((frwClientLoginData = (FRWClientLoginData)HttpContext.Current.Session["UserCredentials"]) != null && frwClientLoginData.Name != "ExternalSystemAccess")
                {
                    if (IsAdminSession == true || HttpContext.Current.Request.CurrentExecutionFilePath.EndsWith("Login.aspx"))
                        e.SessionID = HttpContext.Current.Session.SessionID;
                    else
                        e.SessionID = "{0A239D48-7FB8-4148-A262-2AD6FB5A475E}";

                    TraceExtension.Info("user != ExternalSystemAccess");
                }
                else
                {
                    HttpContext.Current.Session["IsFakeSessionID"] = true;
                    e.SessionID = "{0A239D48-7FB8-4148-A262-2AD6FB5A475E}";
                    //TraceExtension.Info("user == ExternalSystemAccess || user == null");
                }

            }
            else
            {
                TraceExtension.Warn("HttpContext or Session is null!");
                e.SessionID = "{0A239D48-7FB8-4148-A262-2AD6FB5A475E}";
            }
        }

        private const string LOGIN_INFO
        = "0vd2gobq5QpB+gj5UoYPNu37lUyRsigf"
          + "4zJg0T+KaCf/I/iCZiNSxsMsDwlLIbu0"
          + "dweYGzMIsyYBGPN2271OP88QCQ2tzoy2"
          + "lwpVYPpRMJXg9p0jV7yr+kovMW/zu4dz"
          + "AtNK1pAHUscXbMVf9wIKYBnWTADahk/U"
          + "ewtgACqODa8=";

        void Session_Start(object sender, EventArgs e)
        {
            TraceExtension.Info("Session start");
            string sessionId = Session.SessionID;
            SetInternalAuthentication(Session);
        }

        void Session_End(object sender, EventArgs e)
        {
            TraceExtension.Info("Session end");
            //if (Session["IsFakeSessionID"] == null)
            //    FRWClientConnector.Instance.DisposeSession();
            //WriteToChangeLog();
        }

        public static void SetInternalAuthentication(HttpSessionState session)
        {
            TraceExtension.Info("set internal auth");
            try
            {
                string uid = "";
                string pwd = "";

                RetrieveLoginInfo(LOGIN_INFO, out uid, out pwd);

                if (uid != null && pwd != null)
                    FRWWebClientUtilService.SetUserCredentials(session, uid, pwd, false, new TimeSpan(2, 0, 0));
            }

            catch
            {
            }
        }

        private static void RetrieveLoginInfo(string loginInfo, out string uid, out string pwd)
        {
            uid = null;
            pwd = null;

            Encoding encodingASCII = Encoding.ASCII;
            byte[] outBytes = null;
            byte[] outBytes64 = null;

            try
            {
                ATCryptographyBaseServices.FromBase64(encodingASCII.GetBytes(loginInfo), out outBytes64);
                ATKey key = ATCryptographyBaseServices.CreateKey4Digest("");
                ATCryptographyBaseServices.Decrypt(outBytes64, key, out outBytes);

                if (outBytes.Length > 0)
                {
                    Encoding encodingUnicode = Encoding.Unicode;
                    MemoryStream memoryStream = new MemoryStream(outBytes);
                    BinaryReader binaryReader = new BinaryReader(memoryStream, encodingUnicode);
                    uid = binaryReader.ReadString();
                    pwd = binaryReader.ReadString();
                }
            }

            catch
            {
                uid = null;
                pwd = null;
            }
        }

        public string ConfigPath
        {
            get
            {
                return (string)Application["Dashboard.ConfigPath"];
            }
        }


    }
}