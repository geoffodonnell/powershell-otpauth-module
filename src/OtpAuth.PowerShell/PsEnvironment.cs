using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OtpAuth.PowerShell {

	internal static class PsEnvironment {

		private static readonly object mLock = new object();

		public static JsonSerializer JsonSerializer { get; private set; }

		private static bool mIsInitialized;

		static PsEnvironment() {

			mIsInitialized = false;
			JsonSerializer = JsonSerializer.CreateDefault();
		}

		public static void SafeInitialize() {

			if (mIsInitialized) {
				return;
			}

			lock (mLock) {
				if (mIsInitialized) {
					return;
				}

				Initialize();
				mIsInitialized = true;
			}
		}

		public static T Deserialize<T>(string json) {

			var token = JToken
				.Parse(json);

			return token.ToObject<T>(JsonSerializer);
		}

		public static string Serialize<T>(T value) {

			var token = JToken
				.FromObject(value, JsonSerializer);

			return token.ToString();
		}

		private static void Initialize() {

			PsConfiguration.Initialize();
		}

	}

}
