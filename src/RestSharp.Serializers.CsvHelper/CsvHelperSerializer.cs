using CsvHelper;
using CsvHelper.Configuration;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace RestSharp.Serializers.CsvHelper {
    public class CsvHelperSerializer : IDeserializer, IRestSerializer, ISerializer {
        private const string TextCsvContentType = "text/csv";

        private readonly CsvConfiguration _configuration;

        public ISerializer Serializer {
            get {
                return this;
            }
        }

        public IDeserializer Deserializer {
            get {
                return this;
            }
        }

        public string[] AcceptedContentTypes {
            get {
                return new string[] { TextCsvContentType, "application/x-download" };
            }
        }

        public SupportsContentType SupportsContentType {
            get {
                return x => Array.IndexOf(AcceptedContentTypes, x) != -1 || x.Contains("csv");
            }
        }

        public DataFormat DataFormat {
            get {
                return DataFormat.None;
            }
        }

        public string ContentType { get; set; } = TextCsvContentType;

        public CsvHelperSerializer() {
            _configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
        }

        public CsvHelperSerializer(CsvConfiguration configuration) {
            _configuration = configuration;
        }

        public T? Deserialize<T>(RestResponse response) {
            try {
                if (response.Content == null) {
                    throw new InvalidOperationException(message: "Response content is null");
                }
                else {
                    using (StringReader stringReader = new StringReader(response.Content))
                    using (CsvReader csvReader = new CsvReader(stringReader, CultureInfo.CurrentCulture)) {
                        Type? @interface = typeof(T).GetInterface("IEnumerable`1");

                        if (@interface == null) {
                            csvReader.Read();

                            return csvReader.GetRecord<T>();
                        }
                        else {
                            Type itemType = @interface.GenericTypeArguments[0];
                            T result;

                            try {
                                result = Activator.CreateInstance<T>();
                            }
                            catch (MissingMethodException) {
                                throw new InvalidOperationException(message: "The type must contain a public, parameterless constructor.");
                            }

                            MethodInfo? method = typeof(T).GetMethod(name: "Add");

                            if (method == null) {
                                throw new InvalidOperationException(message: "If the type implements IEnumerable<T>, then it must contain a public \"Add(T)\" method.");
                            }
                            else {
                                foreach (object record in csvReader.GetRecords(itemType)) {
                                    method.Invoke(result, new object[]
                                    {
                                        record
                                    });
                                }
                            }

                            return result;
                        }
                    }
                }
            }
            catch (Exception exception) {
                throw new DeserializationException(response, exception);
            }
        }

        public string? Serialize(Parameter parameter) {
            return Serialize(parameter.Value);
        }

        public string? Serialize(object? obj) {
            if (obj == null) {
                return null;
            }
            else {
                using (StringWriter stringWriter = new StringWriter())
                using (CsvWriter csvWriter = new CsvWriter(stringWriter, _configuration)) {
                    if (obj is IEnumerable records) {
                        IEnumerator enumerator = records.GetEnumerator();

                        if (enumerator.MoveNext()) {
                            csvWriter.WriteHeader(enumerator.Current.GetType());
                            csvWriter.NextRecord();
                            csvWriter.WriteRecords(records);
                        }

                        if (enumerator is IDisposable disposable) {
                            disposable.Dispose();
                        }
                    }
                    else {
                        csvWriter.WriteHeader(obj.GetType());
                        csvWriter.NextRecord();
                        csvWriter.WriteRecord(obj);
                        csvWriter.NextRecord();
                    }

                    return stringWriter.ToString();
                }
            }
        }
    }
}