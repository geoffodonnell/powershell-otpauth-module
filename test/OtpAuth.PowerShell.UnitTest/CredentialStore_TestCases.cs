using KeePassLib.Security;
using KeePassLib.Serialization;
using OtpAuth.PowerShell.Model;
using OtpAuth.PowerShell.UnitTest.Services;
using System;
using System.IO;
using System.Linq;

namespace OtpAuth.PowerShell.UnitTest {

	[TestClass]
	public class CredentialStore_TestCases {

		public static readonly string FilePath = $".{Path.DirectorySeparatorChar}credentials.kdbx";
		public const string Password = "__Password__";
		public const string GroupName = "__GroupName__";

		[TestInitialize]
		public void Initialize() {
			IOConnection.m_FilesProvider = new UnitTestFilesProvider();
		}

		[TestCleanup]
		public void Cleanup() {
			
			if (IOConnection.m_FilesProvider is UnitTestFilesProvider provider) {
				provider.Close();
			}
		}

		[TestMethod]
		public void Initialize_CredentialStore() {

			var target = new CredentialStore(FilePath, GroupName);

			Assert.IsFalse(target.Exists);
			Assert.IsFalse(target.Opened);

			target.Open(Password);

			Assert.IsTrue(target.Opened);
			Assert.IsTrue(target.Exists);
		}

		[TestMethod]
		public void Add_Credential_To_CredentialStore() {

			var target = new CredentialStore(FilePath, GroupName);
			var credential = CreateCredential();

			target.Open(Password);

			Assert.IsTrue(target.Opened);

			target.Save(credential);

			var result = target.GetCredentials().FirstOrDefault(s => s.Id == credential.Id);

			Assert.IsNotNull(result);
			Assert.AreEqual(credential.Id, result.Id);
			Assert.AreEqual(credential.Name, result.Name);
			Assert.AreEqual(credential.Issuer, result.Issuer);
			Assert.AreEqual(credential.Secret.ReadString(), result.Secret.ReadString());
			Assert.AreEqual(credential.Algorithm, result.Algorithm);
			Assert.AreEqual(credential.Digits, result.Digits);
			Assert.AreEqual(credential.Type, result.Type);
			Assert.AreEqual(credential.Counter, result.Counter);
			Assert.AreEqual(credential.Created, result.Created);
			Assert.AreEqual(credential.Updated, result.Updated);
		}

		[TestMethod]
		public void Remove_Credential_From_CredentialStore() {

			var target = new CredentialStore(FilePath, GroupName);
			var credential = CreateCredential();

			target.Open(Password);

			Assert.IsTrue(target.Opened);

			target.Save(credential);

			var result = target.GetCredentials().FirstOrDefault(s => s.Id == credential.Id);
			
			Assert.IsNotNull(result);
			Assert.AreEqual(credential.Id, result.Id);

			target.Remove(result);

			var credentials = target.GetCredentials()?.ToList();

			Assert.IsNotNull(credentials);
			Assert.AreEqual(credentials.Count, 0);
		}

		private static CredentialModel CreateCredential() {

			var time = DateTimeOffset.UtcNow;
			var secret = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

			return new CredentialModel() {
				Id = Guid.NewGuid().ToString(),
				Name = Guid.NewGuid().ToString(),
				Issuer = Guid.NewGuid().ToString(),
				Secret = new ProtectedString(true, Convert.ToBase64String(secret)),
				Algorithm = OtpAlgorithm.SHA512,
				Digits = OtpDigitCount.Six,
				Type = OtpType.TOTP,
				Counter = 0,
				Created = time.AddHours(-2),
				Updated = time
			};
		}

	}

}