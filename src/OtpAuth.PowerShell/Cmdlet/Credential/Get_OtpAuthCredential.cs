using System;
using System.Linq;
using System.Management.Automation;

namespace OtpAuth.PowerShell.Cmdlet.Credential {

	[Cmdlet(VerbsCommon.Get, "OtpAuthCredential", DefaultParameterSetName = "Default")]
	public class Get_OtpAuthCredential : CmdletBase {

		[Parameter(
			ParameterSetName = "Id",
			Mandatory = false,
			ValueFromPipeline = false)]
		public string Id { get; set; }

		[Parameter(
			ParameterSetName = "Name",
			Mandatory = false,
			ValueFromPipeline = false)]
		public string Name { get; set; }

		[Parameter(
			ParameterSetName = "Issuer",
			Mandatory = false,
			ValueFromPipeline = false)]
		public string Issuer { get; set; }

		[Parameter(
			ParameterSetName = "GetAll",
			Mandatory = false,
			ValueFromPipeline = false)]
		public SwitchParameter GetAll { get; set; }

		protected override void ProcessRecord() {

			if (!PsConfiguration.CredentialStore.Exists) {
				WriteError(new ErrorRecord(
					new Exception("Credential store is not initialized, use Initialize-OtpAuthCredentialStore and retry this action."),
					string.Empty,
					ErrorCategory.NotSpecified,
					this));
				return;
			}

			if (!PsConfiguration.CredentialStore.Opened) {
				WriteError(new ErrorRecord(
					new Exception("Credential store is not opened, use Open-OtpAuthCredentialStore and retry this action."),
					string.Empty,
					ErrorCategory.NotSpecified,
					this));
				return;
			}

			var credentials = PsConfiguration
				.CredentialStore
				.GetCredentials();

			if (GetAll.IsPresent && (bool)GetAll) {
				foreach (var credential in credentials) {
					WriteObject(credential);
				}
			} else if (!String.IsNullOrWhiteSpace(Id)) {
				var accountById = credentials
					.FirstOrDefault(s => string.Equals(s.Id, Id, StringComparison.OrdinalIgnoreCase));

				WriteObject(accountById);
			} else if (!String.IsNullOrWhiteSpace(Name)) {
				var accountByName = credentials
					.FirstOrDefault(s => String.Equals(s.Name, Name, StringComparison.OrdinalIgnoreCase));

				WriteObject(accountByName);
			} else if (!String.IsNullOrWhiteSpace(Issuer)) {
				var accountByIssuer = credentials
					.FirstOrDefault(s => String.Equals(s.Issuer, Issuer, StringComparison.OrdinalIgnoreCase));

				WriteObject(accountByIssuer);
			} else {
				foreach (var credential in credentials) {
					WriteObject(credential);
				}
			}
		}

	}

}
