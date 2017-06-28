using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using System.Web;


namespace ExtranetChina.Controllers
{
        class FileResult : IHttpActionResult
        {
            private readonly string _filePath;
            private readonly string _contentType;

            public FileResult(string filePath, string contentType = null)
            {
                if (filePath == null) throw new ArgumentNullException("filePath");

                _filePath = filePath;
                _contentType = contentType;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.Run(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(File.OpenRead(_filePath))
                    };

                    var contentType = _contentType ?? MimeMapping.GetMimeMapping(Path.GetExtension(_filePath));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    return response;

                }, cancellationToken);
            }
        }

        class StreamResult : IHttpActionResult
        {
            private readonly Stream _stream;
            private readonly string _contentType;
            private readonly string _fileName;


            public StreamResult(Stream stream, string fileName, string contentType = null)
            {
                if (stream == null) throw new ArgumentNullException("stream");

                _stream = stream;
                _contentType = contentType;
                _fileName = fileName;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.Run(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(_stream)
                    };

                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = _fileName
                    };
                    var contentType = _contentType ?? MimeMapping.GetMimeMapping(Path.GetExtension(_fileName));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    return response;

                }, cancellationToken);
            }
        }
    
    
        public class RequestManager
        {
            public string LastResponse { protected set; get; }

            CookieContainer cookies = new CookieContainer();

            internal string GetCookieValue(Uri SiteUri, string name)
            {
                Cookie cookie = cookies.GetCookies(SiteUri)[name];
                return (cookie == null) ? null : cookie.Value;
            }

            public string GetResponseContent(HttpWebResponse response)
            {
                if (response == null)
                {
                    throw new ArgumentNullException("response");
                }
                Stream dataStream = null;
                StreamReader reader = null;
                string responseFromServer = null;

                try
                {
                    // Get the stream containing content returned by the server.
                    dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    reader = new StreamReader(dataStream);
                    // Read the content.
                    responseFromServer = reader.ReadToEnd();
                    // Cleanup the streams and the response.
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    if (dataStream != null)
                    {
                        dataStream.Close();
                    }
                    response.Close();
                }
                LastResponse = responseFromServer;
                return responseFromServer;
            }

            public HttpWebResponse SendPOSTRequest(string uri, string content, string login, string password, bool allowAutoRedirect)
            {
                HttpWebRequest request = GeneratePOSTRequest(uri, content, login, password, allowAutoRedirect);
                return GetResponse(request);
            }

            public HttpWebResponse SendGETRequest(string uri, string login, string password, bool allowAutoRedirect)
            {
                HttpWebRequest request = GenerateGETRequest(uri, login, password, allowAutoRedirect);
                return GetResponse(request);
            }

            public HttpWebResponse SendRequest(string uri, string content, string method, string login, string password, bool allowAutoRedirect)
            {
                HttpWebRequest request = GenerateRequest(uri, content, method, login, password, allowAutoRedirect);
                return GetResponse(request);
            }

            public HttpWebRequest GenerateGETRequest(string uri, string login, string password, bool allowAutoRedirect)
            {
                return GenerateRequest(uri, null, "GET", null, null, allowAutoRedirect);
            }

            public HttpWebRequest GeneratePOSTRequest(string uri, string content, string login, string password, bool allowAutoRedirect)
            {
                return GenerateRequest(uri, content, "POST", null, null, allowAutoRedirect);
            }

            internal HttpWebRequest GenerateRequest(string uri, string content, string method, string login, string password, bool allowAutoRedirect)
            {
                if (uri == null)
                {
                    throw new ArgumentNullException("uri");
                }
                // Create a request using a URL that can receive a post. 
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
                // Set the Method property of the request to POST.
                request.Method = method;
                // Set cookie container to maintain cookies
                request.CookieContainer = cookies;
                request.AllowAutoRedirect = false;
                // If login is empty use defaul credentials
                if (string.IsNullOrEmpty(login))
                {
                    request.Credentials = CredentialCache.DefaultNetworkCredentials;
                }
                else
                {
                    request.Credentials = new NetworkCredential(login, password);
                }
                if (method == "POST")
                {
                    // Convert POST data to a byte array.
                    byte[] byteArray = Encoding.UTF8.GetBytes(content);
                    // Set the ContentType property of the WebRequest.
                    request.ContentType = "application/x-www-form-urlencoded";
                    // Set the ContentLength property of the WebRequest.
                    request.ContentLength = byteArray.Length;
                    // Get the request stream.
                    Stream dataStream = request.GetRequestStream();
                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    // Close the Stream object.
                    dataStream.Close();
                }
                return request;
            }

            internal HttpWebResponse GetResponse(HttpWebRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    cookies.Add(response.Cookies);
                    // Print the properties of each cookie.
                    Console.WriteLine("\nCookies: ");
                    foreach (Cookie cook in cookies.GetCookies(request.RequestUri))
                    {
                        Console.WriteLine("Domain: {0}, String: {1}", cook.Domain, cook.ToString());
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Web exception occurred. Status code: {0}", ex.Status);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return response;
            }

        }
    

    public class DadosController : ApiController
    {
         public HttpResponseMessage GenerateDocument(int P_CDIDIOMA, string P_IMPRES_DETALHE)
        {
            try
            {
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

                if (P_CDIDIOMA == null || string.IsNullOrEmpty(P_IMPRES_DETALHE)) 
                    return null;

                var manager = new RequestManager();
                string hidden_run_parameters = "hidden_run_parameters=server=rep_bnsvher280_rep10g&report=%2Fhome%2Freport%2Foracle10g%2Freports%2Fhome%2Fcoml%2Fsist%2Far0148.rdf&destype=cache&desformat=pdf&userid=COML%2FCOML%40CORP1_qa&P_ARBRFART_ID=63312&PARAMFORM=YES&P_INDPERCCOMPOS=S&P_IMPRES_INSUMO=S&P_IMPRES_IMAGEM=S&P_IMPRES_MEDIDA=S";
                string _P_CDIDIOMA = "P_CDIDIOMA=" + P_CDIDIOMA;
                string _P_IMPRES_DETALHE = "P_IMPRES_DETALHE=" + P_IMPRES_DETALHE;
                string content = hidden_run_parameters + "&" + _P_CDIDIOMA + "&" + _P_IMPRES_DETALHE;
                string uri = "http://bnsvher280.hering.local:7777/reports/rwservlet?";

                var dados = manager.GetResponseContent(manager.SendPOSTRequest(uri, content, null, null, true));

                result.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(dados)));
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

         public IHttpActionResult GetReport(int P_CDIDIOMA, string P_IMPRES_DETALHE)
        {
            try
            {
                if (P_CDIDIOMA == null || string.IsNullOrEmpty(P_IMPRES_DETALHE))
                    return null;

                var manager = new RequestManager();
                string hidden_run_parameters = "hidden_run_parameters=server=rep_bnsvher280_rep10g&report=%2Fhome%2Freport%2Foracle10g%2Freports%2Fhome%2Fcoml%2Fsist%2Far0148.rdf&destype=cache&desformat=pdf&userid=COML%2FCOML%40CORP1_qa&P_ARBRFART_ID=63312&PARAMFORM=YES&P_INDPERCCOMPOS=S&P_IMPRES_INSUMO=S&P_IMPRES_IMAGEM=S&P_IMPRES_MEDIDA=S";
                string _P_CDIDIOMA = "P_CDIDIOMA=" + P_CDIDIOMA;
                string _P_IMPRES_DETALHE = "P_IMPRES_DETALHE=" + P_IMPRES_DETALHE;
                string content = hidden_run_parameters + "&" + _P_CDIDIOMA + "&" + _P_IMPRES_DETALHE;
                string uri = "http://bnsvher280.hering.local:7777/reports/rwservlet?";

                var dados = manager.GetResponseContent(manager.SendPOSTRequest(uri, content, null, null, true));

                var result = new StreamResult(new MemoryStream(Encoding.UTF8.GetBytes(dados)), "arquivo.pdf");
                return result == null ? (IHttpActionResult)NotFound() : result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
