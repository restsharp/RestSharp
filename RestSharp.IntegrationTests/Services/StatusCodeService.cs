using Kayak.Framework;

namespace RestSharp.IntegrationTests.Services
{
	public class StatusCodeService : KayakService
	{
		[Path("/StatusCode/{statusCode}")]
		public void Root(int statusCode) {
			Response.StatusCode = statusCode;
		}
	}
}