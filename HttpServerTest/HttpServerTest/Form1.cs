using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using FirstFloor.ModernUI.App.Tools.DOS;
using FirstFloor.ModernUI.App.Tools.WindowsManager;

namespace HttpServerTest
{
    public partial class Form1 : Form
    {
        #region HttpServer相关变量
        public string Ip = Dns.GetHostEntry(Dns.GetHostName())
.AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork"))
.ToString();

        public int Port = 9091;

        public string EnvironmentUserName = Environment.UserName;

        private THttpListener _httpListener;
        #endregion

        public Form1()
        {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
 
            InitializeComponent();
            label1.Text = "";
             
            textBoxIP.Text = Ip;
            textBoxPort.Text = Port.ToString();

            linkLabel1.Text = string.Format("http://{0}:{1}/ODOO/Test", Ip, Port);
            linkLabel2.Text = string.Format("http://{0}:{1}/v1/ODOO/", Ip, Port);
            linkLabel4.Text = string.Format("http://{0}:{1}/v2/ODOO/", Ip, Port);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        #region 启动和停止HttpServer按钮事件
        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            label1.Text = "HttpServer已启动";

            Ip = textBoxIP.Text;
            Port = Convert.ToInt32(textBoxPort.Text.Trim());
            linkLabel1.Text = string.Format("http://{0}:{1}/ODOO/Test", Ip, Port);
            linkLabel2.Text = string.Format("http://{0}:{1}/v1/ODOO/", Ip, Port);
            linkLabel4.Text = string.Format("http://{0}:{1}/v2/ODOO/", Ip, Port);

            //添加防火墙例外端口，供客户端访问
            INetFwManger.NetFwAddPorts("ODOO ", Port, "TCP");

            #region 执行dos命令

            DosCommandOperation dosCommandOperation = new DosCommandOperation();
            string dosRet = dosCommandOperation.Execute(string.Format("netsh http add urlacl url=http://{0}:{1}/ user={2}", Ip, Port, EnvironmentUserName));
            #endregion

            #region 启动HttpServer 
            //string ip = Method.GetLocalIP()[1];
            string[] strUrl = new string[] { string.Format("http://{0}:{1}/ODOO/", Ip, Port), string.Format("http://{0}:{1}/v1/ODOO/", Ip, Port), string.Format("http://{0}:{1}/v2/ODOO/", Ip, Port) };
            _httpListener = new THttpListener(strUrl);
            _httpListener.ResponseEvent += _HttpListener_ResponseEvent;
            _httpListener.Start();
            #endregion

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            label1.Text = "HttpServer已停止";

            _httpListener.Stop();
        }
        #endregion

        #region HttpServer相关

        void _HttpListener_ResponseEvent(System.Net.HttpListenerContext ctx)
        {
            Dictionary<string, string> requestParameter = _httpListener.getData(ctx);

            //测试页面
            if (ctx.Request.Url.AbsolutePath.ToUpper().Contains("ODOO/TEST"))
            { 
                ResponseWrite("<html><head><title>ODOO</title></head><body><div><h1>这是一个测试页面</h1><p>如果能打开这个页面，那么HttpServer已经启动成功了。</body></html>", ctx.Response,"text/html");
            }
            else
            {
                ResponseWrite("这是响应的消息", ctx.Response);
            }
        }

        /// <summary>
        /// http响应
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="response"></param>
        /// <param name="type"></param>
        public void ResponseWrite(string msg, System.Net.HttpListenerResponse response, string type = "text/plain")
        {
            try
            {
                //使用Writer输出http响应代码
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(response.OutputStream, new UTF8Encoding()))
                {
                    response.ContentType = type + ";charset=utf-8";
                    writer.WriteLine(msg);
                    writer.Close();
                    response.Close();
                }
            }
            catch (Exception exception)
            {
            }
        }

        #endregion

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel linkLabel = (LinkLabel)sender;
            //调用系统默认的浏览器   
            System.Diagnostics.Process.Start("explorer.exe", linkLabel.Text);
        }
    }
}
