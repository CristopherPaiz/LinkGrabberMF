using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LinkGrab.Constants;
using LinkGrab.Services;

namespace LinkGrab.Tests
{
    // A local stand-in for JD2's API, so tests never touch the real one.
    public sealed class FakeJd : IDisposable
    {
        readonly HttpListener listener = new HttpListener();
        readonly Dictionary<string, string> answers = new Dictionary<string, string>
        {
            { JdRoutes.GrabberLinks, "[]" },
            { JdRoutes.DownloadLinks, "[]" },
            { JdRoutes.GrabberPackages, "[]" },
            { JdRoutes.DownloadPackages, "[]" },
            { JdRoutes.IsCollecting, "false" },
            { JdRoutes.State, "\"STOPPED_STATE\"" },
            { JdRoutes.Speed, "0" },
            { JdRoutes.AddLinks, "null" },
            { JdRoutes.GrabberMove, "null" },
            { JdRoutes.GrabberRemove, "null" }
        };
        readonly string previousBase = JDownloaderClient.Base;
        readonly List<string> calls = new List<string>();

        public string LastQuery { get; private set; }

        // What LinkGrabber holds once links were added, as JD2 would show after crawling them.
        public string GrabberAfterAdd { get; set; }

        // Every request except reads, as "route?arguments".
        public List<string> Changes { get { lock (calls) return new List<string>(calls); } }

        public FakeJd()
        {
            string address = "http://localhost:" + FreePort();
            listener.Prefixes.Add(address + "/");
            listener.Start();
            JDownloaderClient.Base = address;
            new Thread(Serve) { IsBackground = true }.Start();
        }

        public FakeJd Answer(string route, string json)
        {
            answers[route] = json;
            return this;
        }

        static int FreePort()
        {
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            int port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
            return port;
        }

        void Serve()
        {
            try
            {
                while (true) Reply(listener.GetContext());
            }
            catch (HttpListenerException)
            {
                // The listener was closed: the test is over.
            }
            catch (ObjectDisposedException)
            {
                // Same, when it closed between two requests.
            }
        }

        void Reply(HttpListenerContext context)
        {
            string route = context.Request.Url.AbsolutePath;
            string query = Uri.UnescapeDataString(context.Request.Url.Query.TrimStart('?'));
            if (route == JdRoutes.AddLinks)
            {
                LastQuery = query;
                if (GrabberAfterAdd != null) answers[JdRoutes.GrabberLinks] = GrabberAfterAdd;
            }
            if (route == JdRoutes.AddLinks || route == JdRoutes.GrabberMove || route == JdRoutes.GrabberRemove)
            {
                lock (calls) calls.Add(route + "?" + query);
            }

            string answer;
            if (answers.TryGetValue(route, out answer))
            {
                byte[] body = Encoding.UTF8.GetBytes("{\"data\":" + answer + "}");
                context.Response.OutputStream.Write(body, 0, body.Length);
            }
            else
            {
                context.Response.StatusCode = 404;
            }
            context.Response.Close();
        }

        public void Dispose()
        {
            JDownloaderClient.Base = previousBase;
            listener.Close();
        }
    }
}
