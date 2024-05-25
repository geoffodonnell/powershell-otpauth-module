using OtpAuth.PowerShell.Model;
using OtpNet;
using System;
using System.Collections;
using System.IO;
using System.Management.Automation;

namespace OtpAuth.PowerShell.Cmdlet.Code {

	[Cmdlet(VerbsCommon.Get, "OtpAuthCode")]
	public class Get_OtpAuthCode : CmdletBase {

		[Parameter(Mandatory = false, ValueFromPipeline = true)]
		public CredentialModel Credential { get; set; }

		[Parameter(Mandatory = false, ValueFromPipeline = false)]
		public SwitchParameter WithLabel { get; set; }

		protected override void ProcessRecord() {

			var size = GetSize(Credential);
			var mode = GetMode(Credential);
			var period = Credential.Period != 0 ? Credential.Period : CredentialModel.DefaultPeriod;
			var keyAsB64 = Credential.Secret.ReadChars();
			var key = Convert.FromBase64CharArray(keyAsB64, 0, keyAsB64.Length);

			if (Credential.Type == Model.OtpType.HOTP) {

				var hotp = new Hotp(key, mode, size);
				var code = hotp.ComputeHOTP(Credential.Counter);

				WriteObject(GetResult(code));

			} else if (Credential.Type == Model.OtpType.TOTP) {

				var totp = new Totp(key, period, mode, size);
				var code = totp.ComputeTotp();

				WriteObject(GetResult(code));
			}

			Array.Clear(key);
			Array.Clear(keyAsB64);
		}

		private object GetResult(string code) {

			if (WithLabel) {
				return new PSObject(
					new Hashtable {
						{ $"{Credential.Name} - {Credential.Issuer}", code }
					});
			}

			return code;
		}

		private static int GetSize(CredentialModel credential) {

            if ( credential.Digits == OtpDigitCount.Six) {
				return 6;
			}

			if (credential.Digits == OtpDigitCount.Eight) {
				return 8;
			}

			throw new InvalidDataException("Unknown digit count.");
        }

		private static OtpHashMode GetMode(CredentialModel credential) {

			if (credential.Algorithm == OtpAlgorithm.SHA1) {
				return OtpHashMode.Sha1;
			}

			if (credential.Algorithm == OtpAlgorithm.SHA256) {
				return OtpHashMode.Sha256;
			}

			if (credential.Algorithm == OtpAlgorithm.SHA512) {
				return OtpHashMode.Sha512;
			}

			throw new InvalidDataException("Unknown or unsupported algorithm.");
		}

	}

}
