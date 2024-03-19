using ProtoBuf;

namespace OtpAuth.PowerShell.Model {

	[ProtoContract]
	public class OtpMigrationPayload {

		[ProtoMember(1)]
		public OtpMigrationParameters[] OtpParameters { get; set; }

		[ProtoMember(2)]
		public int Version { get; set; }

		[ProtoMember(3)]
		public int BatchSize { get; set; }

		[ProtoMember(4)]
		public int BatchIndex { get; set; }

		[ProtoMember(5)]
		public int BatchId { get; set; }
	}
}
