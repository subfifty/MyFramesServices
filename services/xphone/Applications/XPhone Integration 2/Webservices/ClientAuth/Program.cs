using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using C4B.Atlas.Connection;
using System.Runtime.InteropServices;
using System.Security;
using System.Web.Script.Serialization;
using JWT.Builder;
using JWT.Algorithms;

namespace ClientAuth
{

    public static class CredentialStoreHelper
    {

        /// <summary>
        /// Liefert aus einem SecureString den lesbaren String zurück
        /// </summary>
        public static String GetStringFromSecureString(SecureString a_protectedString)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(a_protectedString);
            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(bstr);
            }
        }

        /// <summary>
        /// Liefert aus einem SecureString den lesbaren String zurück
        /// </summary>
        public static SecureString CreateSecureStringFromString(String a_stringToProtect)
        {
            if (a_stringToProtect == null)
                a_stringToProtect = string.Empty;

            var result = new SecureString();

            foreach (var c in a_stringToProtect)
                result.AppendChar(c);

            return result;
        }
    }
    
    public class AppUser
    {
        public bool IsAuthenticated { get; set; }
        public bool IsAdmin { get; set; }
        public string SessionGUID { get; set; }
        public string UserGUID { get; set; }
        public string CultureInfoName { get; set; }
        public string FullName { get; set; }
        public string Token { get; set; }
        public string Error { get; set; }
    }

    public class BaseService : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ---------- Managed Ressourcen freigeben: ----------


            }

            // ---------- Native Ressourcen hier freigeben: ----------
        }
    }

    public class XPhoneAuthenticationService : BaseService
    {
        public AppUser TryGetXPhoneUser(string hostname, string userName, string password, out string errorMessage, out ATClientConnector clientConnector)
        {
            errorMessage = String.Empty;
            clientConnector = null;

            try
            {
                var connector = ATClientConnector.CreateWithCustomLogin<IXPhoneContract>
                (
                    hostname,
                    new ATClientLoginData(ATClientLoginMode.Atlas)
                    {
                        UcUsername = userName,
                        Password = CredentialStoreHelper.CreateSecureStringFromString(password)
                    }
                );

                var conResult = connector.OpenInstantly();

                if (conResult.State == ATConnectionState.Connected)
                {
                    // Anmeldung erfolgreich AppUser zurückgeben
                    var userInfo = connector.ConnectionStateInfo.UserInfo;

                    //ATClientConnectionIdentityToken idt = new ATClientConnectionIdentityToken(connector.Unity);
                    //string token = idt.GetOrCreateIdentityToken();

                    // ATClientConnector zurückgeben
                    clientConnector = connector;

                    return new AppUser()
                    {
                        IsAuthenticated = true,
                        IsAdmin = userInfo.IsAdmin,
                        SessionGUID = Guid.Empty.ToString(), // Dummy
                        UserGUID = userInfo.Guid.ToString(),
                        CultureInfoName = userInfo.Language.Name,
                        FullName = userInfo.Fullname,
                        Error = ""
                    };
                }

                // Im Fehlerfall ATClientConnector abräumen
                connector.Close();
                connector.Dispose();

                errorMessage = "ResX.Ids_CollabLogin_ServerNotYetStarted";
                return null;
            }
            catch (ATClientConnectorServerUnavailableException)
            {
                errorMessage = "ResX.Ids_CollabLogin_ServerNotYetStarted";
                return null;
            }

            catch
            {
                errorMessage = "ResX.Ids_CollabLogin_AuthenticationFailed";
                return null;
            }

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            const string secret = "CBLPe31qj9hIxcyD3NDD8mTjqGlhpU5YMvgq8ulpTGHRCip5brSl3s6ccHAJFKeT";

            if ( args.Length == 1  && args[0] == "token")
            {
                const string myToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE2NDg5OTU0NzMsIlVzZXJHdWlkIjoiN2FkYTVlNTQtM2EyNC00MmE2LTg0ZDYtYmIwOGYzOTIzNzljIn0.CJBTipRzqLS-iL_i4h63AmRIv968-g1r5cqsE3qQlAo";
                var jwt = JwtBuilder.Create()
                                     .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                                     .WithSecret(secret)
                                     .MustVerifySignature()
                                     .Decode(myToken);
                Console.WriteLine(jwt);
                Console.ReadLine();
                return;
            }

            string hostname = "localhost";
            string username = "";
            string password = "";

            int i = 0;
            if (args.Length > 0) username = args[i++];
            if (args.Length > 1) password = args[i++];
            if (args.Length > 2) hostname = args[i++];

            if ( string.IsNullOrEmpty(hostname) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) )
            {
                AppUser usr = new AppUser() { Error = "Missing hostname, username oder password." };
                var json = new JavaScriptSerializer().Serialize(usr);
                Console.WriteLine(json);
                return;
            }

            ATClientConnector client = null;
            try
            {
                XPhoneAuthenticationService svc = new XPhoneAuthenticationService();
                
                string errMsg = null;

                AppUser usr = svc.TryGetXPhoneUser(hostname, username, password, out errMsg, out client);

                if ( usr != null )
                {
                    var token = JwtBuilder.Create()
                                          .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                                          .WithSecret(secret)
                                          .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                                          .AddClaim("UserGuid", usr.UserGUID)
                                          .Encode();

                    usr.Token = token;

                    //const string myToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE2NDg5OTU0NzMsIlVzZXJHdWlkIjoiN2FkYTVlNTQtM2EyNC00MmE2LTg0ZDYtYmIwOGYzOTIzNzljIn0.CJBTipRzqLS-iL_i4h63AmRIv968-g1r5cqsE3qQlAo";
                    //var jwt = JwtBuilder.Create()
                    //                     .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                    //                     .WithSecret(secret)
                    //                     .MustVerifySignature()
                    //                     .Decode(myToken);
                    //Console.WriteLine(jwt);

                    var json = new JavaScriptSerializer().Serialize(usr);
                    Console.WriteLine(json);
                }
                else
                {
                    usr = new AppUser() { Error = "AUTHENTICATION FAILED: " + errMsg };
                    var json = new JavaScriptSerializer().Serialize(usr);
                    Console.WriteLine(json);
                    return;
                }

            }
            catch (Exception ex)
            {
                AppUser usr = new AppUser() { Error = "AUTHENTICATION FAILED: " + ex.Message };
                var json = new JavaScriptSerializer().Serialize(usr);
                Console.WriteLine(json);
                return;
            }
            finally
            {
                if ( client != null )
                {
                    // Client wieder abräumen!
                    client.Close();
                    client.Dispose();
                }
            }
        }
    }
}
