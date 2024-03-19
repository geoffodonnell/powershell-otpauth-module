using Newtonsoft.Json;
using OtpAuth.PowerShell.Model;
using System;
using System.IO;

namespace OtpAuth.PowerShell {

	internal static class PsConfiguration {

		public static readonly string ConfigurationFolderName = ".otpauth";
		public static readonly string ConfigurationFileName = "config.json";

		public static CredentialStore CredentialStore { get; set; }

		public static ModuleConfiguration ModuleConfiguration { get; set; }

		public static string ConfigurationDirectory { get; private set; }

		static PsConfiguration() {

			var localAppData = Environment.GetFolderPath(
				Environment.SpecialFolder.LocalApplicationData);

			ConfigurationDirectory = Path.Combine(localAppData, ConfigurationFolderName);
		}

		internal static void Initialize() {

			EnsureConfigurationDirectoryExists();
			EnsureConfigurationFileExists();
			InitializeModuleConfiguration();
			InitializeCredentialStore();
		}

		private static void EnsureConfigurationDirectoryExists() {

			if (!Directory.Exists(ConfigurationDirectory)) {
				Directory.CreateDirectory(ConfigurationDirectory);
			}
		}

		private static void EnsureConfigurationFileExists() {

			var filePath = Path.Combine(ConfigurationDirectory, ConfigurationFileName);

			if (!File.Exists(filePath)) {
				var defaultConfiguration = PsConstant.DefaultModuleConfiguration;

				File.AppendAllText(
					filePath, JsonConvert.SerializeObject(defaultConfiguration));
			}
		}

		private static void InitializeModuleConfiguration() {

			var filePath = Path.Combine(ConfigurationDirectory, ConfigurationFileName);
			var serializer = PsEnvironment.JsonSerializer;

			using (var file = File.OpenRead(filePath))
			using (var reader = new StreamReader(file))
			using (var json = new JsonTextReader(reader)) {

				ModuleConfiguration = serializer.Deserialize<ModuleConfiguration>(json);
			}
		}

		private static void InitializeCredentialStore() {

			var path = ModuleConfiguration?.CredentialStoreConfiguration?.Path;
			var fullPath = GetFullPathRelativeTo(ConfigurationDirectory, path);
			var groupName = ModuleConfiguration?.CredentialStoreConfiguration?.GroupName;

			CredentialStore = new CredentialStore(fullPath, groupName);
		}

		private static void SaveModuleConfiguration(ModuleConfiguration configuration) {

			var filePath = Path.Combine(ConfigurationDirectory, ConfigurationFileName);
			var serializer = PsEnvironment.JsonSerializer;

			using (var file = File.Open(filePath, FileMode.Truncate, FileAccess.ReadWrite))
			using (var writer = new StreamWriter(file))
			using (var json = new JsonTextWriter(writer)) {

				serializer.Serialize(json, configuration);
			}

			return;
		}

		private static string GetFullPathRelativeTo(string relativeTo, string path) {

			if (Path.IsPathRooted(path)) {
				return Path.GetFullPath(path);
			}

			var result = Path.Combine(relativeTo, path);

			return Path.GetFullPath(result);
		}

	}

}
