using OtpAuth.PowerShell.Model;
using ProtoBuf;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using ZXing;
using ZXing.Common;
using BarcodeWriter = ZXing.SkiaSharp.BarcodeWriter;

namespace OtpAuth.PowerShell.Cmdlet.Credential {

	[Cmdlet(VerbsData.Export, "OtpAuthCredential")]
	public class Export_OtpAuthCredential : CmdletBase {

		private const int DefaultHeight = 900;
		private const int DefaultWidth = 900;

		[Parameter(Mandatory = true, ValueFromPipeline = true)]
		public CredentialModel Credential { get; set; }

		[Parameter(Mandatory = true, ValueFromPipeline = false)]
		public string Path { get; set; }

		[Parameter(Mandatory = false, ValueFromPipeline = false)]
		public int Height { get; set; } = DefaultHeight;

		[Parameter(Mandatory = false, ValueFromPipeline = false)]
		public int Width { get; set; } = DefaultWidth;

		protected List<CredentialModel> Records;

		protected override void BeginProcessing() {
			WriteVerbose("Export-OtpAuthCredential BeginProcessing");

			Records = new List<CredentialModel>();
		}

		protected override void ProcessRecord() {
			WriteVerbose($"Export-OtpAuthCredential ProcessRecord: Total records = {Records.Count}");

			Records.Add(Credential);
		}

		protected override void EndProcessing() {
			WriteVerbose($"Export-OtpAuthCredential EndProcessing: Total records = {Records.Count}");

			var writer = new BarcodeWriter() {
				Format = BarcodeFormat.QR_CODE,
				Options = new EncodingOptions {
					Height = Height,
					Width = Width
				}
			};

			var path = GetFullPath(Path);
			var payload = CreatePayload();
			var bytes = SerializePayload(payload);
			var bytesAsB64 = Convert.ToBase64String(bytes);
			var uri = $"otpauth-migration://offline?data={bytesAsB64}";

			WriteVerbose($"uri length = {uri.Length}");

			WriteQrCode(writer, uri, path);
		}

		protected OtpMigrationPayload CreatePayload() {

			int batchId = 0;
			int batchIndex = 0;
			int batchSize = Records.Count >= 1 ? 1 : 0;
			int version = Records.Count >= 1 ? 1 : 0;

			var result = new OtpMigrationPayload {
				BatchId = batchId,
				BatchIndex = batchIndex,
				BatchSize = batchSize,
				Version = version,
				OtpParameters = Records.Select(s => s.ToParameters()).ToArray()
			};

			return result;
		}

		protected static byte[] SerializePayload(OtpMigrationPayload payload) {

			byte[] result = null;

			using (var stream = new MemoryStream()) {
				Serializer.Serialize(stream, payload);
				result = stream.ToArray();
			}

			return result;
		}

		protected static void WriteQrCode(BarcodeWriter writer, string uri, string path) {

			using (var bitmap = writer.Write(uri))
			using (var destination = new SKFileWStream(path)) {
				bitmap.Encode(destination, SKEncodedImageFormat.Png, 100);
			}
		}

	}

}
