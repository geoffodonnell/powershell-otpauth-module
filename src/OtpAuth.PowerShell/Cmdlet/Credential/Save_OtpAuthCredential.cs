using OtpAuth.PowerShell.Model;
using System;
using System.Management.Automation;

namespace OtpAuth.PowerShell.Cmdlet.Credential {

	[Cmdlet(VerbsData.Save, "OtpAuthCredential")]
	public class Save_OtpAuthCredential : CmdletBase {

		[Parameter(
			Position = 0,
			Mandatory = true,
			ValueFromPipeline = true)]
		public CredentialModel Credential { get; set; }

		protected override void ProcessRecord() {

			if (!PsConfiguration.CredentialStore.Exists) {
				WriteError(new ErrorRecord(
					new Exception("Credential store is not initialized, use Initialize-OtpAuthCredentialStore and retry this action."),
					String.Empty,
					ErrorCategory.NotSpecified,
					this));
				return;
			}

			if (!PsConfiguration.CredentialStore.Opened) {
				WriteError(new ErrorRecord(
					new Exception("Credential store is not opened, use Open-OtpAuthCredentialStore and retry this action."),
					String.Empty,
					ErrorCategory.NotSpecified,
					this));
				return;
			}

			PsConfiguration.CredentialStore.Save(Credential);
		}

	}

}
