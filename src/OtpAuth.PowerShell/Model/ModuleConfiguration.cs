using Newtonsoft.Json;

namespace OtpAuth.PowerShell.Model {

	public class ModuleConfiguration {

		[JsonProperty("version")]
		public int Version { get; set; }

		[JsonProperty("defaultAlgorithm")]
		public OtpAlgorithm DefaultAlgorithm { get; set; }

		[JsonProperty("credentialStoreConfiguration")]
		public CredentialStoreConfiguration CredentialStoreConfiguration { get; set; }
	
		public ModuleConfiguration() {

			Version = 1;
		}
	}

}
