using OtpAuth.PowerShell.Model;

namespace OtpAuth.PowerShell {

	public static class PsConstant {

		public static ModuleConfiguration DefaultModuleConfiguration = new ModuleConfiguration {
			DefaultAlgorithm = OtpAlgorithm.SHA1,
			CredentialStoreConfiguration = new CredentialStoreConfiguration {
				Path = "credentials.kdbx",
				GroupName = "OtpAuth"
			}
		};
	}
}
