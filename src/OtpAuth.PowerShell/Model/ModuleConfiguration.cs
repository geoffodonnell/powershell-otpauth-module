using Newtonsoft.Json;

namespace OtpAuth.PowerShell.Model {

	public class ModuleConfiguration {

		[JsonProperty("defaultAlgorithm")]
		public OtpAlgorithm DefaultAlgorithm { get; set; }

		[JsonProperty("credentialStoreConfiguration")]
		public CredentialStoreConfiguration CredentialStoreConfiguration { get; set; }
	}

}
