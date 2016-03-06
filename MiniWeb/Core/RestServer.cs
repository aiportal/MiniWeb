using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using bfbd.Common;

namespace bfbd.MiniWeb.Core
{
    /// <summary>
    /// 提供Restful模式的后台服务
    /// </summary>
    public class RestServer : HttpServerBase
    {
        public RestServer(string host, int port, string prefix): base(host, port, prefix) { }

        protected override void ProcessRequest(HttpListenerContext ctx)
        {
            var request = ctx.Request;
            var response = ctx.Response;
            try
            {
                var fpath = request.GetRelationPath(this.Prefix);   // 相对路径

                    // Web服务
                    var handler = MatchHandler(fpath);
                    if (handler != null)
                    {
                        handler.ProcessRequest(this.Prefix, request, response);
                        return;
                    }

                    // 找不到网页，404页面
                    response.SendErrorMessage(404, "file not found: " + request.Url);
            }
            catch (Exception ex)
            {
                if (ex is HttpListenerException)
                {
                    if ((ex as HttpListenerException).ErrorCode == 1229)
                        return;
                }

                Logger.Exception(ex);
                try
                {
                    if (SuppressClicentException)
                        response.SendJsonObject(new { success = false, message = ex.GetInnerMessage() }, false);
                    else
                        response.SendJsonObject(new { success = false, message = ex.ToString() }, false);

                    if (OnException != null)
                        OnException(request, ex);
                }
                catch (Exception) { }
            }
            finally
            {
                try { response.Close(); }
                catch (Exception) { }
            }
        }
    }
}
