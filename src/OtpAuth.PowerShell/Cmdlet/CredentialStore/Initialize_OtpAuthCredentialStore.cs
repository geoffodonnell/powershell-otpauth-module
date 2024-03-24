using System;
using System.Management.Automation;
using System.Net;

namespace OtpAuth.PowerShell.Cmdlet.CredentialStore {

	[Cmdlet(VerbsData.Initialize, "OtpAuthCredentialStore")]
	public class Initialize_OtpAuthCredentialStore : CmdletBase {

		[Parameter(Position = 0, Mandatory = false)]
		public string Password { get; set; }

		protected override void ProcessRecord() {

			if (PsConfiguration.CredentialStore.Exists) {
				WriteError(new ErrorRecord(
						new Exception("Credential store is already initialized, use Open-OtpAuthCredentialStore instead."),
						String.Empty, 
						ErrorCategory.NotSpecified,
						this));
				return;
			}

			var password = String.IsNullOrWhiteSpace(Password)
				? PromptForPassword()
				: Password;

			if (String.IsNullOrWhiteSpace(password)) {
				return;
			}

			PsConfiguration.CredentialStore.Open(password);

			CommandRuntime.Host.UI.WriteLine("");
			CommandRuntime.Host.UI.WriteLine($"Created credential store: '{PsConfiguration.CredentialStore.Location}'");
			CommandRuntime.Host.UI.WriteLine("");
		}

        protected string PromptForPassword() {

			CommandRuntime.Host.UI.WriteLine("Set password for the new credential store.");
			CommandRuntime.Host.UI.Write("Enter password: ");

			var cred1 = CommandRuntime.Host.UI.ReadLineAsSecureString();

			CommandRuntime.Host.UI.Write("Confirm password: ");

			var cred2 = CommandRuntime.Host.UI.ReadLineAsSecureString();

			var pw1 = (new NetworkCredential("", cred1)).Password;
			var pw2 = (new NetworkCredential("", cred2)).Password;

			if (!String.Equals(pw1, pw2, StringComparison.Ordinal)) {
				WriteError(new ErrorRecord(
					new Exception("Passwords do not match, use Initialize-OtpAuthCredentialStore to try again."),
					String.Empty,
					ErrorCategory.NotSpecified,
					this));
				return null;
			}

			return pw1;
        }

	}

}
