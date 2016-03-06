using System;
using System.Windows.Forms;
using System.Drawing;
using System.Configuration;
using System.IO;
using bfbd.Common;

namespace bfbd.MiniWeb.App
{
    public class ApplicationBase : System.Windows.Forms.ApplicationContext
    {
        public ApplicationBase()
        {
            this.InitNotifyIcon();
        }

        #region NotifyIcon

        /// <summary>
        /// Web.Config->appSettings: app.icon, app.title, app.desc
        /// </summary>
        public NotifyIcon NotifyIcon { get; } = new NotifyIcon() { Visible = false };

        /// <summary>
        /// 初始化任务栏图标
        /// </summary>
        protected void InitNotifyIcon()
        {
            try
            {
                // 任务栏图标
                var appIcon = Properties.Resources.app_icon;
                if (ConfigurationManager.AppSettings["app.icon"] != null)
                {
                    var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        ConfigurationManager.AppSettings["app.icon"]);
                    if (File.Exists(iconPath))
                        appIcon = Icon.ExtractAssociatedIcon(iconPath);
                }
                var appName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
                if (ConfigurationManager.AppSettings["app.title"] != null)
                {
                    appName = ConfigurationManager.AppSettings["app.title"];
                }

                var ni = this.NotifyIcon;
                ni.Icon = appIcon;
                ni.Text = appName;
                ni.BalloonTipIcon = ToolTipIcon.Info;
                ni.BalloonTipTitle = this.NotifyIcon.Text;
                ni.BalloonTipText = ConfigurationManager.AppSettings["app.desc"];
                ni.Visible = true;

                // 单击时显示提示窗
                ni.MouseClick += (o, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        ((NotifyIcon)o).ShowBalloonTip(5000);
                };

                // 右键时显示控制菜单
                var menuExit = new MenuItem("退出程序", (o, e) =>
                {
                    if (MessageBox.Show("确认要退出么？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        Application.Exit();
                });
                var menuRestart = new MenuItem("重新启动", (o, e) => Application.Restart());

                ni.ContextMenu = new ContextMenu(
                    new MenuItem[] { menuExit, menuRestart }
                );

                // 程序退出时隐藏图标
                Application.ApplicationExit += (o, ev) =>
                {
                    this.NotifyIcon.Visible = false;
                };
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        #endregion
    }
}
