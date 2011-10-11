using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE
	[Serializable]
#endif
	public class OAuthWebQueryInfo
	{
		public virtual string ConsumerKey { get; set; }
		public virtual string Token { get; set; }
		public virtual string Nonce { get; set; }
		public virtual string Timestamp { get; set; }
		public virtual string SignatureMethod { get; set; }
		public virtual string Signature { get; set; }
		public virtual string Version { get; set; }
		public virtual string Callback { get; set; }
		public virtual string Verifier { get; set; }
		public virtual string ClientMode { get; set; }
		public virtual string ClientUsername { get; set; }
		public virtual string ClientPassword { get; set; }
		public virtual string UserAgent { get; set; }
		public virtual string WebMethod { get; set; }
		public virtual OAuthParameterHandling ParameterHandling { get; set; }
		public virtual OAuthSignatureTreatment SignatureTreatment { get; set; }
		internal virtual string ConsumerSecret { get; set; }
		internal virtual string TokenSecret { get; set; }
	}
}