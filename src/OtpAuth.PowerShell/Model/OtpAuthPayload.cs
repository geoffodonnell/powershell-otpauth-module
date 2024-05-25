using OtpNet;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace OtpAuth.PowerShell.Model {
	public class OtpAuthPayload {

		public byte[] Secret { get; set; }

		public string Name { get; set; }

		public string Issuer { get; set; }

		public OtpAlgorithm Algorithm { get; set; }

		public OtpDigitCount Digits { get; set; }

		public OtpType Type { get; set; }

		public long Counter { get; set; }

		public int Period { get; set; }

		public static OtpAuthPayload FromUri(Uri uri) {

			//SEE: https://docs.yubico.com/yesdk/users-manual/application-oath/uri-string-format.html

			var type = String.Equals(uri.Host, "totp", StringComparison.InvariantCultureIgnoreCase) ? OtpType.TOTP : OtpType.HOTP;
			var issuerAndAccountName = uri.LocalPath.Split(':');
			var issuer = issuerAndAccountName?.FirstOrDefault()?.Substring(1);
			var accountName = issuerAndAccountName?.LastOrDefault();

			var args = HttpUtility.ParseQueryString(uri.Query);

			var secret = args.Get("secret");
			var digitsAsNumber = GetNumberOrDefaultValue(args, "digits", 6);
			var counter = GetNumberOrDefaultValue(args, "counter", 0);
			var period = GetNumberOrDefaultValue(args, "period", 30);

			var digits = digitsAsNumber == 6 ? OtpDigitCount.Six
				: digitsAsNumber == 7 ? OtpDigitCount.Seven
				: digitsAsNumber == 8 ? OtpDigitCount.Eight
				: OtpDigitCount.Unspecified;

			if (!Enum.TryParse<OtpAlgorithm>(args.Get("algorithm"), out var algorithm)) {
				algorithm = OtpAlgorithm.SHA1;
			}
			
			return new OtpAuthPayload {
				Secret = Base32Encoding.ToBytes(secret),
				Name = accountName,
				Issuer = issuer,
				Algorithm = algorithm,
				Digits = digits,
				Type = type,
				Counter = counter,
				Period = period
			};
		}

		private static int GetNumberOrDefaultValue(NameValueCollection args, string key, int defaultValue) {

			var value = args.Get(key);

			if (String.IsNullOrWhiteSpace(value)) {
				return defaultValue;
			}

			if (Int32.TryParse(value, out var result)) {
				return result;
			}

			return defaultValue;
		}
	}
}
