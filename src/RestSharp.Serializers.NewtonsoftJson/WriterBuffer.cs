using System.Globalization;
using System.Text;

namespace RestSharp.Serializers.NewtonsoftJson; 

sealed class WriterBuffer : IDisposable {
    readonly StringWriter   _stringWriter;
    readonly JsonTextWriter _jsonTextWriter;

    public WriterBuffer(JsonSerializer jsonSerializer) {
        _stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);

        _jsonTextWriter = new JsonTextWriter(_stringWriter) {
            Formatting = jsonSerializer.Formatting, CloseOutput = false
        };
    }

    public JsonTextWriter GetJsonTextWriter() => _jsonTextWriter;
        
    public StringWriter GetStringWriter() => _stringWriter;
        
    public void Dispose() => _stringWriter.GetStringBuilder().Clear();
}