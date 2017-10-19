using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;


public class THttpListener
{
    HttpListener _listerner;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prefixes">格式 http://*/test/ </param>
    /// <param name="authent"></param>
    public THttpListener(string[] prefixes, AuthenticationSchemes authent = AuthenticationSchemes.Anonymous)
    {
        _listerner = new HttpListener();
        _listerner.AuthenticationSchemes = authent;//指定身份验证 Anonymous匿名访问 

        foreach (var item in prefixes)
        {
            _listerner.Prefixes.Add(item);
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " : HttpListener：" + item);
        }
    }

    public delegate void ResponseEventArges(HttpListenerContext ctx);
    public event ResponseEventArges ResponseEvent;
    AsyncCallback _ac = null;

    public void Start()
    {
        if (!_listerner.IsListening)
        {
            _listerner.Start();
            _ac = new AsyncCallback(GetContextAsyncCallback);
            _listerner.BeginGetContext(_ac, null);
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " : Start");
        }
    }

    /// <summary>
    /// 停止监听服务
    /// </summary>
    public void Stop()
    {
        _listerner.Stop();
    }

    /// <summary>
    /// 收到监听请求回调
    /// </summary>
    /// <param name="ia"></param>
    public void GetContextAsyncCallback(IAsyncResult ia)
    {
        try
        {
            if (ia.IsCompleted)
            {
                var ctx = _listerner.EndGetContext(ia);
                ctx.Response.StatusCode = 200;
                if (ResponseEvent != null)
                {
                    ResponseEvent.BeginInvoke(ctx, null, null);
                }
                else
                {
                    System.IO.BinaryWriter br = new System.IO.BinaryWriter(ctx.Response.OutputStream, new UTF8Encoding());
                    br.Write("error: 服务器未处理");
                    ctx.Response.Close();
                    br.Close();
                    br.Dispose();
                }
            }
            _listerner.BeginGetContext(_ac, null);
        }
        catch (Exception exception)
        { 
        }
    }

    public Dictionary<string, string> getData(System.Net.HttpListenerContext ctx)
    {
        var request = ctx.Request;
        if (request.HttpMethod == "GET")
        {
            return getData(ctx, DataType.Get);
        }
        else
        {
            return getData(ctx, DataType.Post);
        }
    }

    public Dictionary<string, string> getData(System.Net.HttpListenerContext ctx, DataType type)
    {
        var rets = new Dictionary<string, string>();
        var request = ctx.Request;
        switch (type)
        {
            case DataType.Post:
                if (request.HttpMethod == "POST")
                {
                    string rawData;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        rawData = reader.ReadToEnd();
                    }
                    string[] rawParams = rawData.Split('&');
                    foreach (string param in rawParams)
                    {
                        string[] kvPair = param.Split('=');
                        string key = kvPair[0];
                        string value = HttpUtility.UrlDecode(kvPair[1]);
                        rets[key] = value;
                    }
                }
                break;
            case DataType.Get:
                if (request.HttpMethod == "GET")
                {
                    string[] keys = request.QueryString.AllKeys;
                    foreach (string key in keys)
                    {
                        rets[key] = request.QueryString[key];
                    }
                }
                break;
        }
        return rets;
    }

    /// <summary>
    /// 数据提交方式
    /// </summary>
    public enum DataType
    {
        Post,
        Get,
    }
}
