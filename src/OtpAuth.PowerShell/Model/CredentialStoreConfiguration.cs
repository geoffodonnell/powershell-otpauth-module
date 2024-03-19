using Newtonsoft.Json;

namespace OtpAuth.PowerShell.Model {

	public class CredentialStoreConfiguration {

		[JsonProperty("path")]
		public string Path { get; set; }

		[JsonProperty("groupName")]
		public string GroupName { get; set; }
	}

}
