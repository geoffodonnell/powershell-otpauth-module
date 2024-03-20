using OtpAuth.PowerShell.Model;
using ProtoBuf;
using SkiaSharp;
using System;
using System.IO;
using System.Management.Automation;
using System.Web;
using ZXing;
using BarcodeReader = ZXing.SkiaSharp.BarcodeReader;

namespace OtpAuth.PowerShell.Cmdlet.Credential {

	[Cmdlet(VerbsData.Import, "OtpAuthCredential")]
	public class Import_OtpAuthCredential : CmdletBase {

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
			var args = HttpUtility.ParseQueryString(uri.Query);
			var payload = args.Get("data").Replace(' ', '+');
			var payloadAsBytes = Convert.FromBase64String(payload);

			OtpMigrationPayload model = null;

			using (var stream = new MemoryStream(payloadAsBytes)) {
				model = Serializer.Deserialize<OtpMigrationPayload>(stream);
			}

			foreach (var entry in model.OtpParameters) {
				WriteObject(CredentialModel.FromParameters(entry));
			}
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
