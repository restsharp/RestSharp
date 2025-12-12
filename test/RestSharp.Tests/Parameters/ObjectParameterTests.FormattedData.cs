namespace RestSharp.Tests.Parameters;

public partial class ObjectParameterTests {
    sealed record FormattedData<TDateTime>([property: RequestProperty(Format = "hh:mm tt")] TDateTime FormattedParameter) where TDateTime : notnull;
}
