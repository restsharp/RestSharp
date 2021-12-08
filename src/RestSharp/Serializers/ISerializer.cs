namespace RestSharp.Serializers;

public interface ISerializer {
    string ContentType { get; set; }

    string? Serialize(object obj);
}