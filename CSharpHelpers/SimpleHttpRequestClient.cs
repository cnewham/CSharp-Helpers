using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CSharpHelpers
{
    /// <summary>
    /// Creates a simple http GET or POST request and returns the response asynchronously. Optimized for .NET 4.0
    /// </summary>
    public class SimpleHttpRequestClient
    {
        public const string Post = "POST";
        public const string Get = "GET";

        public const string HtmlContentType = "text/html";
        public const string XmlContentType = "text/xml";
        public const string JsonContentType = "application/json";
        public const string BinContentType = "application/octet-stream";

        private string _method = Get;

        public SimpleHttpRequestClient(string serverUrl)
        {
            this.ServerUrl = new Uri(serverUrl);
            this.QueryString = new Dictionary<string, string>();
        }

        #region Properties

        /// <summary>
        /// Server URI
        /// </summary>
        public Uri ServerUrl { get; set; }

        /// <summary>
        /// Request method. Default GET
        /// </summary>
        public string Method
        {
            get { return _method; }
            set { _method = value; }
        }

        /// <summary>
        /// Query string key value pair
        /// </summary>
        public Dictionary<string,string> QueryString { get; set; } 

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends a request with no body
        /// </summary>
        /// <param name="callback"></param>
        public void SendRequest(Action<string> callback)
        {
            this.Method = Get;
            this.SendRequest(new byte[0], HtmlContentType, response => callback(Encoding.UTF8.GetString(response)));
        }

        /// <summary>
        /// Sends a JSON request
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        /// <param name="contentType"></param>
        public void SendRequest(string data, string contentType, Action<string> callback)
        {
            this.Method = Post;

            byte[] content = Encoding.UTF8.GetBytes(data);
            this.SendRequest(content, contentType, response => callback(Encoding.UTF8.GetString(response)));
        }

        /// <summary>
        /// Sends a byte stream request
        /// </summary>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        /// <param name="contentType"></param>
        public void SendRequest(byte[] content, string contentType, Action<byte[]> callback)
        {
            var request = (HttpWebRequest)WebRequest.Create(this.BuildRequestUrl());

            if (content.Length > 0)
                this.Method = Post;

            request.Method = this.Method;
            request.ContentLength = content.Length;
            request.ContentType = contentType;

            if (this.Method == Post)
            {
                Stream stream = request.GetRequestStream();
                stream.Write(content, 0, content.Length);
                stream.Close();                
            }

            this.SendRequest(request, response =>
                {
                    var responseData = new MemoryStream();

                    using (var responseStream = response.GetResponseStream())
                    {
                        responseStream.CopyTo(responseData);
                    }

                    response.Close();
                    callback(responseData.ToArray());
                });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Send request and get response async. 
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/202481/how-to-use-httpwebrequest-net-asynchronously</remarks>
        /// <param name="request"></param>
        /// <param name="responseAction"></param>
        private void SendRequest(HttpWebRequest request, Action<HttpWebResponse> responseAction)
        {
            Action wrapperAction = () => request.BeginGetResponse((iar) =>
                {
                    var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
                    responseAction(response);
                }, request);

            wrapperAction.BeginInvoke((iar) =>
                {
                    var action = (Action)iar.AsyncState;
                    action.EndInvoke(iar);
                }, wrapperAction);
        }

        /// <summary>
        /// Builds request url with query string parameters
        /// </summary>
        /// <returns></returns>
        private string BuildRequestUrl()
        {
            var builder = new StringBuilder(this.ServerUrl.ToString());

            if (this.QueryString.Count > 0)
            {
                builder.Append("?");

                foreach (var item in this.QueryString)
                    builder.AppendFormat("{0}={1}&", item.Key, item.Value);

                builder.Remove(builder.Length - 1, 1);
            }

            return builder.ToString();
        }

        #endregion
    }
}
