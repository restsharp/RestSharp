namespace RestSharp.Tests.Integrated;

#pragma warning disable xUnit1031 // Blocking calls in tests are intentional — we are testing sync-over-async deadlock safety

public sealed class SyncRequestTests(WireMockTestServer server) : IClassFixture<WireMockTestServer> {
    [Fact]
    public void Sync_execute_should_not_deadlock() {
        // Regression test for https://github.com/restsharp/RestSharp/issues/2083
        // Sync methods (ExecuteGet) could deadlock when await calls inside the pipeline
        // did not use ConfigureAwait(false), causing continuations to try to marshal
        // back to a captured SynchronizationContext.

        using var client  = new RestClient(server.Url!);
        var       request = new RestRequest("success");

        RestResponse? response = null;

        var completed = Task.Run(() => {
            response = client.ExecuteGet(request);
        }).Wait(TimeSpan.FromSeconds(10));

        completed.Should().BeTrue("sync ExecuteGet should complete without deadlocking");
        response.Should().NotBeNull();
        response!.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public void Sync_execute_with_deserialization_should_not_deadlock() {
        using var client  = new RestClient(server.Url!);
        var       request = new RestRequest("success");

        RestResponse<SuccessResponse>? response = null;

        var completed = Task.Run(() => {
            response = client.ExecuteGet<SuccessResponse>(request);
        }).Wait(TimeSpan.FromSeconds(10));

        completed.Should().BeTrue("sync ExecuteGet<T> should complete without deadlocking");
        response.Should().NotBeNull();
        response!.IsSuccessStatusCode.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public void Sync_execute_from_multiple_threads_should_not_deadlock() {
        using var client = new RestClient(server.Url!);
        const int threadCount = 5;

        var completed = Parallel.For(0, threadCount, _ => {
            var request  = new RestRequest("success");
            var response = client.ExecuteGet(request);
            response.IsSuccessStatusCode.Should().BeTrue();
        });

        completed.IsCompleted.Should().BeTrue();
    }
}
