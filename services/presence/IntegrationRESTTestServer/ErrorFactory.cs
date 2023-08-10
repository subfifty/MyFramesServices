using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationRESTTestServer
{
    internal static class ErrorFactory
    {
        public static T GenerateError<T>(string a_errorMsg, HttpStatusCode a_statusCode, T a_return = default(T))
        {
            throw new WebFaultException<string>(a_errorMsg, a_statusCode);
        }

        public static T GenerateError<T>(string a_errorMsg)
        {
            return GenerateError<T>(a_errorMsg, HttpStatusCode.InternalServerError);
        }

        public static T GenerateError<T>(Exception a_exception)
        {
            return GenerateError<T>(a_exception.ToString(), HttpStatusCode.InternalServerError);
        }
    }
}
