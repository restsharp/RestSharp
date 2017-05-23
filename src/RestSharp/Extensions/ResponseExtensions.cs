namespace RestSharp.Extensions
{
    public static class ResponseExtensions
    {
        public static IRestResponse<T> ToAsyncResponse<T>(this IRestResponse response)
        {
            return new RestResponse<T>
                   {
                       ContentEncoding = response.ContentEncoding,
                       ContentLength = response.ContentLength,
                       ContentType = response.ContentType,
                       Cookies = response.Cookies,
                       ErrorException = response.ErrorException,
                       ErrorMessage = response.ErrorMessage,
                       Headers = response.Headers,
                       RawBytes = response.RawBytes,
                       ResponseStatus = response.ResponseStatus,
                       ResponseUri = response.ResponseUri,
                       Server = response.Server,
                       StatusCode = response.StatusCode,
                       StatusDescription = response.StatusDescription
                   };
        }
    }
}
