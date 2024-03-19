using System;
using System.Management.Automation;

namespace OtpAuth.PowerShell.Cmdlet.CredentialStore {

	[Cmdlet(VerbsCommon.Close, "OtpAuthCredentialStore")]
	public class Close_OtpAuthCredentialStore : CmdletBase {

		protected override void ProcessRecord() {

			if (!PsConfiguration.CredentialStore.Exists) {
				WriteError(new ErrorRecord(
					new Exception("Credential store is not initialized."),
					String.Empty,
					ErrorCategory.NotSpecified, 
					this));
				return;
			}

			if (!PsConfiguration.CredentialStore.Opened) {
				WriteError(new ErrorRecord(
					new Exception("Credential store is not opened."),
					String.Empty,
					ErrorCategory.NotSpecified,
					this));
				return;
			}

			PsConfiguration.CredentialStore.Close();
		}

	}

}
