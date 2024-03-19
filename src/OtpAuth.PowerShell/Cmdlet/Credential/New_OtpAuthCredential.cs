using OtpAuth.PowerShell.Model;
using ProtoBuf;
using SkiaSharp;
using System;
using System.IO;
using System.Management.Automation;
using BarcodeReader = ZXing.SkiaSharp.BarcodeReader;

namespace OtpAuth.PowerShell.Cmdlet.Credential {

	[Cmdlet(VerbsCommon.New, "OtpAuthCredential")]
	public class New_OtpAuthCredential : CmdletBase {

		[Parameter(Mandatory = false, ValueFromPipeline = false)]
		public string Path { get; set; }

		protected override void ProcessRecord() {

			var reader = new BarcodeReader();
			var bitmap = SKBitmap.Decode(GetFullPath(Path));
			var result = reader.Decode(bitmap);
			var format = result?.BarcodeFormat;

			if (!format.HasValue || format != ZXing.BarcodeFormat.QR_CODE) {
				WriteVerbose($"{format} is unsupported, expected {ZXing.BarcodeFormat.QR_CODE}");
				return;
			}

			var uri = new Uri(result?.Text);
			var args = System.Web.HttpUtility.ParseQueryString(uri.Query);
			var payload = args.Get("data");
			var payloadAsBytes = Convert.FromBase64String(payload);

			OtpMigrationPayload model = null;

			using (var stream = new MemoryStream(payloadAsBytes)) {
				model = Serializer.Deserialize<OtpMigrationPayload>(stream);
			}

			foreach (var entry in model.OtpParameters) {
				WriteObject(CredentialModel.FromParameters(entry));
			}
		}

	}

}
