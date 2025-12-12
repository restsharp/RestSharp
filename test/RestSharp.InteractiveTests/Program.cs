using RestSharp.InteractiveTests;
// ReSharper disable HeuristicUnreachableCode

using var client = new TwitterClient("apikey", "apisecret");

await foreach (var tweet in client.SearchStream()) {
    Console.WriteLine(tweet);
}

return;

#pragma warning disable CS0162 // Unreachable code detected
var keys = new AuthenticationTests.TwitterKeys {
    ConsumerKey    = Prompt("Consumer key"),
    ConsumerSecret = Prompt("Consumer secret")
};

await AuthenticationTests.Can_Authenticate_With_OAuth_Async_With_Callback(keys);
#pragma warning restore CS0162 // Unreachable code detected

static string Prompt(string message) {
    Console.Write($"{message}: ");
    return Console.ReadLine();
}