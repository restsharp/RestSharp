using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace RestSharp.IntegrationTests.Helpers
{
    public static class Handlers
    {
        /// <summary>
        /// Echoes the request input back to the output.
        /// </summary>
        //public static void Echo(HttpListenerContext context)
        //{
        //    context.Request.InputStream.CopyTo(context.Response.OutputStream);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static Action<HttpListenerContext> EchoBasicAuthCredentialsValue()
        {
            return ctx =>
            {
                var header = ctx.Request.Headers["Authorization"];

                var parts = Encoding.ASCII.GetString(Convert.FromBase64String(header.Substring("Basic ".Length))).Split(':');
                ctx.Response.OutputStream.WriteStringUtf8(string.Join("|", parts));
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static Action<HttpListenerContext> EchoCookieRequestValue(string cookieName)
        {
            return ctx =>
            {
                string value = ctx.Request.Cookies[cookieName].Value;
                ctx.Response.OutputStream.WriteStringUtf8(value);
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static Action<HttpListenerContext> EchoCookieResponseValue(string cookieName, string cookieValue)
        {
            return ctx =>
            {
                ctx.Response.Cookies.Add(new Cookie(cookieName, cookieValue));
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static Action<HttpListenerContext> EchoQuerystringValue(string parameterName)
        {
            return ctx =>
            {
                string value = ctx.Request.QueryString[parameterName];
                ctx.Response.OutputStream.WriteStringUtf8(value);
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static Action<HttpListenerContext> EchoMediaTypeValue(string mediaType)
        {
            return ctx =>
            {
                ctx.Response.ContentType = mediaType;
                ctx.Response.OutputStream.WriteStringUtf8("TEST");
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public static Action<HttpListenerContext> EchoSegmentValue(string pathSegment)
        {
            return ctx =>
            {
                string value = ctx.Request.Url.AbsolutePath.Contains(pathSegment).ToString();
                ctx.Response.OutputStream.WriteStringUtf8(value);
            };
        }
        
        
        /// <summary>
        /// Echoes the given value back to the output.
        /// </summary>
        public static Action<HttpListenerContext> EchoValue(string value)
        {
            return ctx =>
            {
                System.Diagnostics.Debug.WriteLine("SimpleServer is echoing: '" + value + "'");
                ctx.Response.OutputStream.WriteStringUtf8(value);
            };
        }


        /// <summary>
        /// Echoes the given value back to the output.
        /// </summary>
        public static Action<HttpListenerContext> EchoPostObject()
        {
            return ctx =>
            {
                var reader = new StreamReader(ctx.Request.InputStream);
                string input = reader.ReadToEnd();

                var name = JsonConvert.DeserializeObject<Name>(input);

                var output = JsonConvert.SerializeObject(name);

                ctx.Response.ContentType = "application/json";
                ctx.Response.ContentLength64 = output.Length;
                ctx.Response.OutputStream.WriteStringUtf8(output);
            };
        }

        class RequestData
        {
            public HttpWebRequest WebRequest;
            public HttpListenerContext Context;

            public RequestData(HttpWebRequest request, HttpListenerContext context)
            {
                WebRequest = request;
                Context = context;
            }
        }

        /// <summary>
        /// Echoes the given value back to the output.
        /// </summary>
        public static Action<HttpListenerContext> EchoFileObject(byte[] source)
        {
            return ctx =>
            {
                var parse = new HttpMultipartParser.MultipartFormDataParser(ctx.Request.InputStream, System.Text.UTF8Encoding.Default);
                var part = parse.Files[0];
                
                byte[] bytes = new byte[part.Data.Length];
                part.Data.Read(bytes, 0, (int)part.Data.Length);

                var result = source.SequenceEqual(bytes);

                ctx.Response.OutputStream.WriteStringUtf8(result.ToString());
            };
        }

        /// <summary>
        /// Echoes the given value back to the output.
        /// </summary>
        public static Action<HttpListenerContext> EchoMultiPartObject(byte[] source)
        {
            return ctx =>
            {
                var parse = new HttpMultipartParser.MultipartFormDataParser(ctx.Request.InputStream, System.Text.UTF8Encoding.Default);
                var part = parse.Files[0];

                byte[] bytes = new byte[part.Data.Length];
                part.Data.Read(bytes, 0, (int)part.Data.Length);

                var result = source.SequenceEqual(bytes);

                ctx.Response.OutputStream.WriteStringUtf8(result.ToString());
            };
        }

        /// <summary>
        /// Response to a request like this:  http://localhost:8080/assets/koala.jpg
        /// by streaming the file located at "assets\koala.jpg" back to the client.
        /// </summary>
        public static void FileHandler(HttpListenerContext context)
        {
            var pathToFile = Path.Combine(context.Request.Url.Segments.Select(s => s.Replace("/", "")).ToArray());

            using(var reader = new StreamReader(pathToFile))
                reader.BaseStream.CopyTo(context.Response.OutputStream);
        }

        /// <summary>
        /// T should be a class that implements methods whose names match the urls being called, and take one parameter, an HttpListenerContext.
        /// e.g.
        /// urls exercised:  "http://localhost:8080/error"  and "http://localhost:8080/get_list"
        /// 
        /// class MyHandler
        /// {
        ///   void error(HttpListenerContext ctx)
        ///   {
        ///     // do something interesting here
        ///   }
        ///
        ///   void get_list(HttpListenerContext ctx)
        ///   {
        ///     // do something interesting here
        ///   }
        /// }
        /// </summary>
        public static Action<HttpListenerContext> Generic<T>() where T : new()
        {
            return ctx =>
            {
                var methodName = ctx.Request.Url.Segments.Last();
                var method = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                if(method.IsStatic)
                {
                    method.Invoke(null, new object[] { ctx });
                }
                else
                {
                    method.Invoke(new T(), new object[] { ctx });
                }
            };
        }
    }
}