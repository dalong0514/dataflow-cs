using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace dataflow_cs.Utils.CADUtils
{
    public static class UtilsWeb
    {
        public static string Get(string serviceUrl)
        {
            try
            {
                //创建Web访问对象
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(new Uri(serviceUrl));
                //通过Web访问对象获取响应内容
                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                //通过响应内容流创建StreamReader对象，因为StreamReader更高级更快
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                //string returnXml = HttpUtility.UrlDecode(reader.ReadToEnd());//如果有编码问题就用这个方法
                string returnXml = reader.ReadToEnd();//利用StreamReader就可以从响应内容从头读到尾
                reader.Close();
                if (myResponse != null)
                    myResponse.Close();
                if (myRequest != null)
                    myRequest.Abort();
                return returnXml;
            }
            catch
            {
                return null;
            }

        }

        public static string Post(string serviceUrl, string data)
        {
            try
            {
                string returnXml = "";
                //创建Web访问对象
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(new Uri(serviceUrl));
                //把用户传过来的数据转成"UTF-8"的字节流
                byte[] buf = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(data);

                myRequest.Method = "POST";
                myRequest.ContentLength = buf.Length;
                myRequest.ContentType = "application/json";
                myRequest.KeepAlive = false;
                myRequest.ProtocolVersion = HttpVersion.Version10;
                myRequest.Timeout = 100000;
                myRequest.MediaType = "application/json";
                myRequest.Accept = "application/json";

                //发送请求
                Stream stream = myRequest.GetRequestStream();
                stream.Write(buf, 0, buf.Length);

                //获取接口返回值
                //通过Web访问对象获取响应内容
                HttpWebResponse myResponse = null;
                try
                {
                    myResponse = (HttpWebResponse)myRequest.GetResponse();
                    //通过响应内容流创建StreamReader对象，因为StreamReader更高级更快
                    StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                    //string returnXml = HttpUtility.UrlDecode(reader.ReadToEnd());//如果有编码问题就用这个方法
                    returnXml = reader.ReadToEnd();//利用StreamReader就可以从响应内容从头读到尾
                    reader.Close();

                }
                catch (Exception ex)
                {
                    string exc = ex.ToString();

                }
                finally
                {
                    stream.Close();
                    if (myResponse != null)
                        myResponse.Close();
                    if (myRequest != null)
                    {
                        myRequest.Abort();
                    }
                }


                return returnXml;
            }
            catch
            {
                return null;
            }


        }

        public static string DoPost(string serviceUrl, string data = "")
        {
            string json = string.Empty;
            if (string.IsNullOrEmpty(data))
                json = Get(serviceUrl);
            else
                json = Post(serviceUrl, data);

            if (string.IsNullOrEmpty(json))
                return null;

            JObject root = (JObject)JsonConvert.DeserializeObject(json);
            int code = (int)root["code"];
            if (code != 20000)
            {
                UtilsCADActive.Editor.WriteMessage((string)root["message"]);
                return null;
            }

            return root["data"].ToString();
        }


    }
}
