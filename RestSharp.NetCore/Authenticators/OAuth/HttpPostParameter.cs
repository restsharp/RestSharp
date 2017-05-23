using System.IO;

namespace RestSharp.Authenticators.OAuth
{
    internal class HttpPostParameter : WebParameter
    {
        public HttpPostParameter(string name, string value)
            : base(name, value) { }

        public virtual HttpPostParameterType Type { get; private set; }

        public virtual string FileName { get; private set; }

        public virtual string FilePath { get; private set; }

        public virtual Stream FileStream { get; set; }

        public virtual string ContentType { get; private set; }

        public static HttpPostParameter CreateFile(string name, string fileName, string filePath, string contentType)
        {
            HttpPostParameter parameter = new HttpPostParameter(name, string.Empty)
                                          {
                                              Type = HttpPostParameterType.File,
                                              FileName = fileName,
                                              FilePath = filePath,
                                              ContentType = contentType,
                                          };

            return parameter;
        }

        public static HttpPostParameter CreateFile(string name, string fileName, Stream fileStream, string contentType)
        {
            HttpPostParameter parameter = new HttpPostParameter(name, string.Empty)
                                          {
                                              Type = HttpPostParameterType.File,
                                              FileName = fileName,
                                              FileStream = fileStream,
                                              ContentType = contentType,
                                          };

            return parameter;
        }
    }
}
