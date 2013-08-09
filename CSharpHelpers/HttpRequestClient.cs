using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CSharpHelpers
{
    public class HttpRequestClient
    {
        public HttpRequestClient(string url)
        {
            this.Address = new Uri(url);
        }

        #region Properties

        public Uri Address { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends a JSON or XML request
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        public void SendRequest(string data, Action<string> callback)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            this.SendRequest(bytes, response => callback(Encoding.UTF8.GetString(response)));
        }

        /// <summary>
        /// Sends a byte stream request
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        public void SendRequest(byte[] data, Action<byte[]> callback)
        {
            var request = (HttpWebRequest)WebRequest.Create(this.Address.AbsoluteUri);
            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();

            this.SendRequest(request, response =>
                {
                    var responseData = new MemoryStream();

                    using (var responseStream = response.GetResponseStream())
                    {
                        responseStream.CopyTo(responseData);
                    }

                    callback(responseData.GetBuffer());
                });
        }

        /// <summary>
        /// Send request and get response async
        /// </summary>
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

        #endregion
    }
}
