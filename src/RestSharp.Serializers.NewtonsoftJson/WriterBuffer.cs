//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

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