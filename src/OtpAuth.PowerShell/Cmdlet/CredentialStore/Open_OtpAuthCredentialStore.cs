using System;
using System.Management.Automation;
using System.Net;

namespace OtpAuth.PowerShell.Cmdlet.CredentialStore {

	[Cmdlet(VerbsCommon.Open, "OtpAuthCredentialStore")]
	public class Open_OtpAuthCredentialStore : CmdletBase {

		[Parameter(Position = 0, Mandatory = false)]
		public string Password { get; set; }

		protected override void ProcessRecord() {

			if (!PsConfiguration.CredentialStore.Exists) {
				WriteError(
					new ErrorRecord(
						new Exception("Credential store is not initialized, use Initialize-OtpAuthCredentialStore instead."),
						String.Empty,
						ErrorCategory.NotSpecified,
						this));
				return;
			}

			if (PsConfiguration.CredentialStore.Opened) {
				WriteWarning("Credential store is already open.");
				return;
			}

			var password = Password;

			if (String.IsNullOrWhiteSpace(password)) {

				CommandRuntime.Host.UI.Write("Enter password: ");

				var secureString = CommandRuntime.Host.UI.ReadLineAsSecureString();
				password = (new NetworkCredential("", secureString)).Password;
			}

			PsConfiguration.CredentialStore.Open(password);
		}

	}

}
