using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using PSInfisicalAPI.Errors;
using PSInfisicalAPI.Logging;

namespace PSInfisicalAPI.Http
{
    public sealed class InfisicalHttpClient : IInfisicalHttpClient
    {
        private const string Component = "HttpClient";
        private readonly IInfisicalLogger _logger;
        private readonly int _timeoutSeconds;

        public InfisicalHttpClient(IInfisicalLogger logger, int timeoutSeconds = 100)
        {
            _logger = logger ?? NullInfisicalLogger.Instance;
            _timeoutSeconds = timeoutSeconds;
        }

        public InfisicalHttpResponse Send(InfisicalHttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Uri == null)
            {
                throw new InfisicalHttpException("Request URI must be provided.");
            }

            string operation = string.IsNullOrEmpty(request.OperationName) ? request.EndpointName : request.OperationName;

            _logger.Verbose(Component, string.Concat("Attempting HTTP ", request.Method, " to ", request.Uri.GetLeftPart(UriPartial.Path), ". Please Wait..."));

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(request.Uri);
            webRequest.Method = string.IsNullOrEmpty(request.Method) ? "GET" : request.Method.ToUpperInvariant();
            webRequest.Accept = "application/json";
            webRequest.UserAgent = "PSInfisicalAPI";
            webRequest.Timeout = _timeoutSeconds * 1000;
            webRequest.ReadWriteTimeout = _timeoutSeconds * 1000;
            webRequest.UseDefaultCredentials = true;

            IWebProxy systemProxy = WebRequest.GetSystemWebProxy();
            if (systemProxy != null)
            {
                systemProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                webRequest.Proxy = systemProxy;
            }

            ApplyHeaders(webRequest, request.Headers);

            if (!string.IsNullOrEmpty(request.Body))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(request.Body);
                webRequest.ContentType = string.IsNullOrEmpty(request.ContentType) ? "application/json" : request.ContentType;
                webRequest.ContentLength = bytes.Length;

                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
            }

            InfisicalHttpResponse response = new InfisicalHttpResponse
            {
                ContainsSecretMaterialInResponse = request.ContainsSecretMaterialInResponse
            };

            try
            {
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    PopulateResponse(response, webResponse);
                }

                _logger.Verbose(Component, string.Concat("HTTP ", webRequest.Method, " completed with status ", response.StatusCode.ToString(System.Globalization.CultureInfo.InvariantCulture), "."));

                return response;
            }
            catch (WebException webException)
            {
                HttpWebResponse errorResponse = webException.Response as HttpWebResponse;
                if (errorResponse != null)
                {
                    PopulateResponse(response, errorResponse);
                    errorResponse.Close();
                    return response;
                }

                throw new InfisicalHttpException(string.Concat("HTTP request failed: ", webException.Message), webException);
            }
        }

        private static void ApplyHeaders(HttpWebRequest webRequest, IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> entry in headers)
            {
                if (string.IsNullOrEmpty(entry.Key))
                {
                    continue;
                }

                string headerName = entry.Key;
                string headerValue = entry.Value ?? string.Empty;

                if (string.Equals(headerName, "Accept", StringComparison.OrdinalIgnoreCase)) { webRequest.Accept = headerValue; continue; }
                if (string.Equals(headerName, "User-Agent", StringComparison.OrdinalIgnoreCase)) { webRequest.UserAgent = headerValue; continue; }
                if (string.Equals(headerName, "Content-Type", StringComparison.OrdinalIgnoreCase)) { webRequest.ContentType = headerValue; continue; }

                webRequest.Headers[headerName] = headerValue;
            }
        }

        private static void PopulateResponse(InfisicalHttpResponse response, HttpWebResponse webResponse)
        {
            response.StatusCode = (int)webResponse.StatusCode;
            response.ReasonPhrase = webResponse.StatusDescription;
            response.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string headerName in webResponse.Headers.AllKeys)
            {
                response.Headers[headerName] = webResponse.Headers[headerName];
            }

            using (Stream responseStream = webResponse.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        response.Body = reader.ReadToEnd();
                    }
                }
                else
                {
                    response.Body = string.Empty;
                }
            }
        }
    }
}
