using Newtonsoft.Json;
using System.IO;
using System.Management.Automation;
using System.Threading;

namespace OtpAuth.PowerShell.Cmdlet {

	public abstract class CmdletBase : PSCmdlet {

		protected CancellationTokenSource CancellationTokenSource { get; private set; }

		protected CancellationToken CancellationToken => CancellationTokenSource.Token;

		protected CmdletBase() { 

			PsEnvironment.SafeInitialize();

			CancellationTokenSource = new CancellationTokenSource();
		}

		protected override void StopProcessing() {

			CancellationTokenSource.Cancel();

			base.StopProcessing();
		}

		public virtual T ReadJsonFile<T>(string path) {

			var fullPath = GetFullPath(path);
			var serializer = PsEnvironment.JsonSerializer;

			T value = default(T);

			using (var stream = File.OpenRead(fullPath))
			using (var reader = new StreamReader(stream))
			using (var jsonReader = new JsonTextReader(reader)) {
				value = serializer.Deserialize<T>(jsonReader);
			}

			return value;
		}

		public virtual void WriteJsonFile<T>(string path, T value) {

			var fullPath = GetFullPath(path);
			var serializer = PsEnvironment.JsonSerializer;

			using (var stream = File.OpenWrite(fullPath))
			using (var writer = new StreamWriter(stream))
			using (var jsonWriter = new JsonTextWriter(writer)) {
				serializer.Serialize(jsonWriter, value);
			}
		}

		protected virtual string GetModulePath() {

			return MyInvocation.MyCommand.Module.ModuleBase;
		}

		protected virtual string GetFullPath(string fullOrRelativePath) {

			if (Path.IsPathRooted(fullOrRelativePath)) {
				return Path.GetFullPath(fullOrRelativePath);
			}

			var basePath = SessionState.Path.CurrentFileSystemLocation.Path;
			var result = Path.Combine(basePath, fullOrRelativePath);

			return Path.GetFullPath(result);
		}

	}

}
