using OtpAuth.PowerShell.Model;
using ProtoBuf;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Web;
using ZXing;
using BarcodeReader = ZXing.SkiaSharp.BarcodeReader;

namespace OtpAuth.PowerShell.Cmdlet.Credential {

	[Cmdlet(VerbsData.Import, "OtpAuthCredential")]
	public class Import_OtpAuthCredential : CmdletBase {

		private static readonly StringComparison mCmp = StringComparison.InvariantCultureIgnoreCase;

		[Parameter(Mandatory = true, ValueFromPipeline = true)]
		public string Path { get; set; }

		protected override void ProcessRecord() {

			var path = GetFullPath(Path);
			var result = Decode(path);

			if (result == null) {
				WriteError(new ErrorRecord (
					new Exception("Failed to decode image."),
					String.Empty,
					ErrorCategory.NotSpecified,
					this));
			}

			var format = result.BarcodeFormat;

			if (format != BarcodeFormat.QR_CODE) {
				WriteError(new ErrorRecord(
					new Exception($"{format} is unsupported, expected {BarcodeFormat.QR_CODE}"),
					String.Empty,
					ErrorCategory.NotSpecified,
					this));
				return;
			}

			var uri = new Uri(result?.Text);
			var credentials = GetCredentialModels(uri);

			foreach (var entry in credentials) {
				WriteObject(entry);
			}
		}

		private static CredentialModel[] GetCredentialModels(Uri uri) {

			if (String.Equals(uri.Scheme, "otpauth-migration", mCmp)) {
				return ProcessMigrationUri(uri); 
			}

			if (String.Equals(uri.Scheme, "otpauth", mCmp)) {
				return ProcessOtpAuthUri(uri);
			}

			return null;
		}

		private static CredentialModel[] ProcessMigrationUri(Uri uri) {

			// SEE: https://alexbakker.me/post/parsing-google-auth-export-qr-code.html

			var args = HttpUtility.ParseQueryString(uri.Query);
			var data = args.Get("data")?.Replace(' ', '+');
			var payloadAsBytes = Convert.FromBase64String(data);

			OtpMigrationPayload payload = null;

			using (var stream = new MemoryStream(payloadAsBytes)) {
				payload = Serializer.Deserialize<OtpMigrationPayload>(stream);
			}

			return payload
				.OtpParameters
				.Select(CredentialModel.FromParameters)
				.ToArray();
		}

		private static CredentialModel[] ProcessOtpAuthUri(Uri uri) {
						
			var payload = OtpAuthPayload.FromUri(uri);
			var result = CredentialModel.FromAuthPayload(payload);

			return [result];
		}

		private static Result Decode(string fullPath) {

			var reader = new BarcodeReader();

			reader.Options.PossibleFormats = [ BarcodeFormat.QR_CODE ];
			reader.Options.Hints.Add(DecodeHintType.TRY_HARDER, true);

			var bitmap = SKBitmap.Decode(fullPath);
			var result = reader.Decode(bitmap);

			if (result == null) {
				var info = bitmap.Info.WithSize(new SKSizeI {
					Width = bitmap.Width * 2,
					Height = bitmap.Height * 2
				});

				bitmap = bitmap.Resize(info, SKFilterQuality.High);
			}

			if (bitmap == null) {
				return null;
			}

			return reader.Decode(bitmap);
		}
	}
}