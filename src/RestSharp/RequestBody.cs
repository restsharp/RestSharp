namespace RestSharp; 

public class RequestBody {
    public BodyElement[] Elements { get; }
    public bool          IsForm   { get; }

    public RequestBody(BodyElement[] elements, bool isForm) {
        Elements    = elements;
        IsForm = isForm;
    }

    public record BodyElement(string Name, string Content);
}