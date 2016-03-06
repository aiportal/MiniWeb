using System;
using System.Net;
using System.Diagnostics.Contracts;
using bfbd.Common;
using System.Configuration;

namespace bfbd.MiniWeb.Core
{
    /// <summary>
    /// 封装 HTTP Server 基础功能
    /// </summary>
    public abstract class HttpServerBase
    {
        /// <summary>
        /// HTTP监听域名配置项
        /// </summary>
        public virtual string Host_Config { get; } = "http.host";

        /// <summary>
        /// HTTP监听端口配置项
        /// </summary>
        public virtual string Port_Config { get; } = "http.port";

        #region Implement

        private HttpListener _listener;

        public HttpServerBase() : this(null) { }

        public HttpServerBase(string prefix)
        {
            Contract.Requires(prefix == null || prefix.Trim('/') == prefix);

            this.Host = ConfigurationManager.AppSettings[Host_Config] ?? "localhost";
            this.Port = int.Parse(ConfigurationManager.AppSettings[Port_Config] ?? "80");
            this.Prefix = prefix;
        }

        public HttpServerBase(string host, int port, string prefix)
        {
            Contract.Requires(prefix == null || prefix.Trim('/') == prefix);

            this.Host = host;
            this.Port = port;
            this.Prefix = prefix;
        }

        public virtual void Start()
        {
            try
            {
                var prefix = string.IsNullOrEmpty(this.Prefix) ? null : this.Prefix + "/";
                _listener = new HttpListener();
                _listener.IgnoreWriteExceptions = true;
                _listener.Prefixes.Add(string.Format("http://{0}:{1}/{2}", Host, Port, prefix));
                _listener.Start();
                _listener.BeginGetContext(this.Listener_Request, _listener);
            }
            catch (Exception ex) { Logger.Exception(ex); throw; }
        }

        public virtual void Stop()
        {
            if (_listener != null)
            {
                try
                {
                    _listener.Stop();
                    _listener = null;
                }
                catch (Exception ex) { Logger.Exception(ex); throw; }
            }
        }

        /// <summary>
        /// 以阻塞方式运行
        /// </summary>
        public void RunSync()
        {
            try
            {
                _listener = new System.Net.HttpListener();
                _listener.Prefixes.Add(string.Format("http://{0}:{1}/{2}/", Host, Port, Prefix));
                _listener.Start();

                while (true)
                {
                    HttpListenerContext ctx = _listener.GetContext();
                    if (ctx != null)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(p =>
                            ProcessRequest(ctx)
                        );
                    }
                }
            }
            catch (Exception ex) { Logger.Exception(ex); throw; }
        }

        private void Listener_Request(IAsyncResult ar)
        {
            HttpListenerContext ctx = null;
            try
            {
                HttpListener listener = ar.AsyncState as HttpListener;
                ctx = listener.EndGetContext(ar);
                if (listener.IsListening)
                    listener.BeginGetContext(this.Listener_Request, listener);
            }
            catch (Exception ex) { Logger.Exception(ex); }

            if (ctx != null)
            {
                try
                {
                    ProcessRequest(ctx);
                }
                catch (Exception) { }
            }
        }

        #endregion

        /// <summary>
        /// 处理Http请求
        /// </summary>
        protected abstract void ProcessRequest(HttpListenerContext ctx);

        /// <summary>
        /// 主机头
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// 监听端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 网址前缀
        /// </summary>
        public string Prefix { get; }
    }
}
