using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GameOfSounds
{
    class RestCommand
    {
        const string ServerUrl = "http://goodsky.azurewebsites.net/GameOfSounds.svc/";

        // Stored as the Async WebRequest state
        struct RestCommandState
        {
            public string command;
            public HttpWebRequest webRequest;
            public byte[] requestData;
        }

        // Start a REST API Call
        public void PingRestAPI()
        {
            HttpWebRequest req = WebRequest.Create(new Uri(ServerUrl + "EchoWithPost")) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] formData = UTF8Encoding.UTF8.GetBytes("This is a ping from my Windows Phone!");
            req.ContentLength = formData.Length;

            RestCommandState commandState;
            commandState.command = "Ping";
            commandState.webRequest = req;
            commandState.requestData = formData;

            req.BeginGetRequestStream(RequestStreamCallback, commandState);

            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                GamePage.gamePage.DebugTextbox.Text = "Sent Request Stream Request");
        }

        // Handle Request Stream writing
        public void RequestStreamCallback(IAsyncResult ar)
        {
            // Make sure the state is correct
            if (!(ar.AsyncState is RestCommandState))
            {
                throw new Exception("Request Stream callback failed.");
            }

            // Get the web request
            RestCommandState reqState = (RestCommandState)ar.AsyncState;
            HttpWebRequest req = reqState.webRequest;
            byte[] data = reqState.requestData;

            // Write out request
            Stream requestStream = req.EndGetRequestStream(ar);
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            
            // Start the async result call
            req.BeginGetResponse(ResponseStreamCallback, reqState);

            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                GamePage.gamePage.DebugTextbox.Text = "Sent Request");
        }

        // Handle HTTP response
        public void ResponseStreamCallback(IAsyncResult ar)
        {
            // Make sure the state is correct
            if (!(ar.AsyncState is RestCommandState))
            {
                throw new Exception("Response Stream callback failed.");
            }
            RestCommandState reqState = (RestCommandState)ar.AsyncState;

            HttpWebRequest responseRequest = reqState.webRequest;
            HttpWebResponse response = (HttpWebResponse)responseRequest.EndGetResponse(ar);

            string responseMsg = new StreamReader(response.GetResponseStream()).ReadToEnd();

            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                GamePage.gamePage.DebugTextbox.Text =  responseMsg);
        }
    }
}
