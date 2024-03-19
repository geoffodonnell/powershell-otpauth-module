using ProtoBuf;

namespace OtpAuth.PowerShell.Model {

	[ProtoContract]
	public class OtpMigrationParameters {

		[ProtoMember(1)]
		public byte[] Secret { get; set; }

		[ProtoMember(2)]
		public string Name { get; set; }

		[ProtoMember(3)]
		public string Issuer { get; set; }

		[ProtoMember(4)]
		public OtpAlgorithm Algorithm { get; set; }

		[ProtoMember(5)]
		public OtpDigitCount Digits { get; set; }

		[ProtoMember(6)]
		public OtpType Type { get; set; }

		[ProtoMember(7)]
		public long Counter { get; set; }
	}
}
