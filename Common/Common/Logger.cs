using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace bfbd.Common
{
    /// <summary>
    /// 通用日志类
    /// </summary>
    /// <remarks>
    ///   <system.diagnostics>
    ///     <switches>
    ///       <add name = "Logger" value="4" />
    ///     </switches>
    ///     <trace autoflush = "true" indentsize="4">
    ///       <listeners>
    ///         <add name = "Logger" type="System.Diagnostics.TextWriterTraceListener" initializeData="..\trace.log" />
    ///         <remove name = "Default" />
    ///       </listeners >
    ///     </trace >
    ///   </system.diagnostics >
    /// </remarks>
    public static class Logger
    {
        private static TraceSwitch _ts = new TraceSwitch("Logger", "Description for Logger");

        public static void Error(string msg) { Trace.WriteLineIf(_ts.TraceError, msg + " at " + GetFunctionFullName(2)); }
        public static void Warning(string msg) { Trace.WriteLineIf(_ts.TraceWarning, msg + " at " + GetFunctionFullName(2)); }
        public static void Info(string msg) { Trace.WriteLineIf(_ts.TraceInfo, msg + " at " + GetFunctionFullName(2)); }
        public static void Verbose(string msg) { Trace.WriteLineIf(_ts.TraceVerbose, msg + " at " + GetFunctionFullName(2)); }

        public static void Enter() { Trace.WriteLineIf(_ts.TraceVerbose, "Enter " + GetFunctionFullName(2)); }
        public static void Exit() { Trace.WriteLineIf(_ts.TraceVerbose, "Exit " + GetFunctionFullName(2)); }

        public static void Exception(Exception ex)
        {
            Trace.WriteLineIf(true, string.Format("Exception at {0}, {1}, {2}\r\n{3}", GetFunctionFullName(2), ex.Message, ex.Source, ex.StackTrace));
        }

        #region Inner Implement

        /// <summary>
        /// 获取被调用的函数名称
        /// </summary>
        /// <param name="skipFrames">嵌套的函数层数</param>
        /// <returns></returns>
        private static string GetFunctionFullName(int skipFrames)
        {
            string str = string.Empty;
            try
            {
                StackFrame frame = new StackFrame(skipFrames);
                var method = frame.GetMethod();
                str = string.Format("{0}::{1}", method.Name, method.DeclaringType.Name);
            }
            catch (Exception) { }
            return str;
        }

       #endregion
    }
}
