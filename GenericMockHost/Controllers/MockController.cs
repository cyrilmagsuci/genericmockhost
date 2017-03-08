//  Copyright(c) 2017 Michael Cyril D.Magsuci
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
//  associated documentation files (the "Software"), to deal in the Software without restriction, including
//  without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
//  sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
//  subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all copies or substantial
//  portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//  INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
//  PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
//  THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;

namespace GenericMockHost.Controllers
{

   // [Route("api")]
    public class MockController : ApiController
    {

        [System.Web.Http.HttpGet, System.Web.Http.HttpPost, System.Web.Http.HttpPut, System.Web.Http.HttpDelete]
        public HttpResponseMessage HandleAny(HttpRequestMessage request)
        {

            //http://localhost:4978/biyahekoapi/domesticair.asmx


            string soapAction = request.Headers.Contains("SOAPAction")
                ? request.Headers.FirstOrDefault(a => a.Key == "SOAPAction").Value.FirstOrDefault()
                : string.Empty;

            if (string.IsNullOrWhiteSpace(soapAction))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {

                string soapActionName = (new Uri(soapAction.Replace("\"", ""))).Segments.Last();

                var segments = request.RequestUri.Segments.Select(a => a.Replace("/", ""));

                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Responses", string.Join("\\", segments.Skip(1)), soapActionName + ".xml");

                string content = File.ReadAllText(filePath);
                string pstContentFileName = soapActionName + ".PstrFinalOutPut";

                string pstFinalOutputMarker = "{{" + pstContentFileName + "}}";

                if (content.Contains(pstFinalOutputMarker))
                {
                    filePath = Path.Combine((new FileInfo(filePath)).DirectoryName, pstContentFileName + ".xml") ;
                    string pstrOutputContent = File.ReadAllText(filePath);
                    content = content.Replace(pstFinalOutputMarker, XmlEscape(pstrOutputContent));
                }

                var response = Request.CreateResponse(HttpStatusCode.OK);

                response.Content = new StringContent(content, Encoding.UTF8, "text/xml");

                return response;
            }

        }

        public static string XmlEscape(string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        public static string XmlUnescape(string escaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerXml = escaped;
            return node.InnerText;
        }
    }
}
