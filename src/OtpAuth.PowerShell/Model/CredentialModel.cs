using KeePassLib;
using KeePassLib.Security;
using System;

namespace OtpAuth.PowerShell.Model {

	public class CredentialModel {

		public const int DefaultPeriod = 30;

		public string Id { get; set; }

		public string Name { get; set; }

		public ProtectedString Secret { get; set; }

		public string Issuer { get; set; }

		public OtpAlgorithm Algorithm { get; set; }

		public OtpDigitCount Digits { get; set; }

		public OtpType Type { get; set; }

		public long Counter { get; set; }

		public int Period { get; set; }

		public DateTimeOffset Created { get; set; }

		public DateTimeOffset Updated { get; set; }

		public CredentialModel() { 
		
			Id = Guid.NewGuid().ToString();
		}

		public static CredentialModel FromParameters(OtpMigrationParameters parameters) {

			var now = DateTimeOffset.UtcNow;
			var secret = Convert.ToBase64String(parameters.Secret);

			return new CredentialModel {
				Name = parameters.Name ?? "(undefined)",
				Secret = new ProtectedString(true, secret),
				Issuer = parameters.Issuer ?? "(undefined)",
				Algorithm = parameters.Algorithm,
				Digits = parameters.Digits,
				Type = parameters.Type,
				Counter = parameters.Counter,
				Period = DefaultPeriod,
				Created = now,
				Updated = now
			};
		}

		public static CredentialModel FromAuthPayload(OtpAuthPayload payload) {

			var now = DateTimeOffset.UtcNow;
			var secret = Convert.ToBase64String(payload.Secret);

			return new CredentialModel {
				Name = payload.Name ?? "(undefined)",
				Secret = new ProtectedString(true, secret),
				Issuer = payload.Issuer ?? "(undefined)",
				Algorithm = payload.Algorithm,
				Digits = payload.Digits,
				Type = payload.Type,
				Counter = payload.Counter,
				Period = payload.Period,
				Created = now,
				Updated = now
			};
		}

		public OtpMigrationParameters ToParameters() {

			return new OtpMigrationParameters {
				Name = Name,
				Secret = Convert.FromBase64String(Secret.ReadString()),
				Issuer = Issuer,
				Algorithm = Algorithm,
				Digits = Digits,
				Type = Type,
				Counter = Counter
			};
		}

		public static CredentialModel FromKeePassEntry(PwEntry entry) {

			var result = new CredentialModel {
				Id = entry.Strings.ReadSafe("Id").Trim(),
				Name = entry.Strings.ReadSafe("Title").Trim(),
				Secret = entry.Strings.Get("Password"),
				Issuer = entry.Strings.ReadSafe("Issuer").Trim(),
				Algorithm = Enum.Parse<OtpAlgorithm>(entry.Strings.ReadSafe("Algorithm").Trim()),
				Digits = Enum.Parse<OtpDigitCount>(entry.Strings.ReadSafe("Digits").Trim()),
				Type = Enum.Parse<OtpType>(entry.Strings.ReadSafe("Type").Trim()),
				Counter = Convert.ToInt64(entry.Strings.ReadSafe("Counter").Trim()),
				Period = DefaultPeriod,
				Created = DateTimeOffset.Parse(entry.Strings.ReadSafe("Created").Trim()),
				Updated = DateTimeOffset.Parse(entry.Strings.ReadSafe("Updated").Trim())
			};

			if (entry.Strings.Exists("Period")) {
				result.Period = Convert.ToInt32(entry.Strings.ReadSafe("Period").Trim());
			}

			return result;
		}

		public PwEntry ToKeePassEntry() {

			var result = new PwEntry(true, true);

			result.Strings.Set("Id", new ProtectedString(true, Id));
			result.Strings.Set("Title", new ProtectedString(true, Name));
			result.Strings.Set("Password", Secret);
			result.Strings.Set("Issuer", new ProtectedString(true, Issuer));
			result.Strings.Set("Algorithm", new ProtectedString(true, Convert.ToString((int)Algorithm)));
			result.Strings.Set("Digits", new ProtectedString(true, Convert.ToString((int)Digits)));
			result.Strings.Set("Type", new ProtectedString(true, Convert.ToString((int)Type)));
			result.Strings.Set("Counter", new ProtectedString(true, Convert.ToString(Counter)));
			result.Strings.Set("Period", new ProtectedString(true, Convert.ToString(Period)));
			result.Strings.Set("Created", new ProtectedString(true, Created.ToString("O")));
			result.Strings.Set("Updated", new ProtectedString(true, Updated.ToString("O")));

			return result;
		}
	}
}
