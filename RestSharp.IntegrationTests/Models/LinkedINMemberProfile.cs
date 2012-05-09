namespace RestSharp.IntegrationTests.Models
{
	/// <summary>
	/// Model for used by the LinkedIN integration tests. <see cref="oAuth1Tests.Can_Retrieve_Member_Profile_Field_Field_Selector_From_LinkedIN"/>.
	/// </summary>
	public class LinkedINMemberProfile
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}