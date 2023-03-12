using CsvHelper;
using CsvHelper.Configuration;
using System.Collections;
using System.Globalization;

namespace RestSharp.Serializers.CsvHelper; 

public class CsvHelperSerializer : IDeserializer, IRestSerializer, ISerializer {
    const string TextCsvContentType = "text/csv";

    readonly CsvConfiguration _configuration;

    public ISerializer Serializer => this;

    public IDeserializer Deserializer => this;

    public string[] AcceptedContentTypes => new[] { TextCsvContentType, "application/x-download" };

    public SupportsContentType SupportsContentType => x => Array.IndexOf(AcceptedContentTypes, x) != -1 || x.Value.Contains("csv");

    public DataFormat DataFormat => DataFormat.None;

    public ContentType ContentType { get; set; } = TextCsvContentType;

    public CsvHelperSerializer() => _configuration = new CsvConfiguration(CultureInfo.InvariantCulture);

    public CsvHelperSerializer(CsvConfiguration configuration) => _configuration = configuration;

    public T? Deserialize<T>(RestResponse response) {
        try {
            if (response.Content == null)
                throw new InvalidOperationException(message: "Response content is null");

            using var stringReader = new StringReader(response.Content);

            using var csvReader = new CsvReader(stringReader, _configuration);

            var @interface = typeof(T).GetInterface("IEnumerable`1");

            if (@interface == null) {
                csvReader.Read();

                return csvReader.GetRecord<T>();
            }

            var itemType = @interface.GenericTypeArguments[0];
            T   result;

            try {
                result = Activator.CreateInstance<T>();
            }
            catch (MissingMethodException) {
                throw new InvalidOperationException(message: "The type must contain a public, parameterless constructor.");
            }

            var method = typeof(T).GetMethod(name: "Add");

            if (method == null) {
                throw new InvalidOperationException(
                    message: "If the type implements IEnumerable<T>, then it must contain a public \"Add(T)\" method."
                );
            }

            foreach (var record in csvReader.GetRecords(itemType)) {
                method.Invoke(result, new[] { record });
            }

            return result;
        }
        catch (Exception exception) {
            throw new DeserializationException(response, exception);
        }
    }

    public string? Serialize(Parameter parameter) => Serialize(parameter.Value);

    public string? Serialize(object? obj) {
        if (obj == null) {
            return null;
        }

        using var stringWriter = new StringWriter();

        using var csvWriter = new CsvWriter(stringWriter, _configuration);

        if (obj is IEnumerable records) {
            // ReSharper disable once PossibleMultipleEnumeration
            var enumerator = records.GetEnumerator();

            if (enumerator.MoveNext() && enumerator.Current != null) {
                csvWriter.WriteHeader(enumerator.Current.GetType());
                csvWriter.NextRecord();
                // ReSharper disable once PossibleMultipleEnumeration
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
