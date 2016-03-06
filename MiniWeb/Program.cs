using System;
using System.Windows.Forms;
using bfbd.Common;
using bfbd.MiniWeb.Weixin;

namespace bfbd.MiniWeb
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Application.ThreadException += (o, ev) => { Logger.Exception(ev.Exception); };
            //AppDomain.CurrentDomain.UnhandledException += (o, ev) => { Logger.Exception(ev.ExceptionObject as Exception); };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Weixin.WeixinApp());
        }
    }
}
