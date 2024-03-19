using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using OtpAuth.PowerShell.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OtpAuth.PowerShell {

	public class CredentialStore {

		public string Location => mFilePath;

		public string GroupName => mGroupName;

		public bool Exists { get {

				// PwDatabase uses this static instance for file operations, 
				// by default that results in writing database files to disk,
				// however for testing the file content is held in memory.
				return IOConnection.m_FilesProvider.IsFileExist(mFilePath);
			}
		}

		public bool Opened => mDatabase != null;

		private readonly IStatusLogger mStatusLogger;
		private readonly string mFilePath;
		private readonly string mGroupName;
		private PwDatabase mDatabase;

		public CredentialStore(string filePath, string groupName) {

			mStatusLogger = new NullStatusLogger();
			mFilePath = filePath;
			mGroupName = groupName;
			mDatabase = null;
		}

		public void Open(string password) {

			if (String.IsNullOrEmpty(password)) {
				throw new ArgumentNullException("password");
			}

			if (mDatabase != null) {
				throw new InvalidOperationException(
					$"CredentialStore is already open, call {nameof(Close)} first.");
			}

			try {
				mDatabase = new PwDatabase();

				var connection = IOConnectionInfo.FromPath(mFilePath);
				var kcpPassword = new KcpPassword(password);
				var key = new CompositeKey();
				
				key.AddUserKey(kcpPassword);

				if (Exists) {
					mDatabase.Open(connection, key, mStatusLogger);
				} else {
					mDatabase.New(connection, key);
					mDatabase.Save(mStatusLogger);
				}
			} catch (Exception) {
				mDatabase = null;
				throw;
			}
		}

		public virtual void Close() {

			mDatabase?.Close();
			mDatabase = null;
		}

		public virtual IEnumerable<CredentialModel> GetCredentials() {

			var group = GetOrCreateGroup(mGroupName);

			if (group == null) {
				return new List<CredentialModel>();
			}

			return group
				.Entries
				.Select(CredentialModel.FromKeePassEntry)
				.ToList();
		}

		public virtual void Save(CredentialModel credential) {

			if (mDatabase == null) {
				return;
			}

			var group = GetOrCreateGroup(mGroupName);

			if (group == null) {
				throw new Exception($"An error occured while retrieving group '{mGroupName}'");
			}

			foreach (var entry in group.Entries) {

				var id = entry.Strings.ReadSafe("Id")?.Trim();

				if (String.Equals(credential.Id.Trim(), id, StringComparison.OrdinalIgnoreCase)) {
					entry.Strings.Set("Title", new ProtectedString(true, credential.Name));
					entry.Strings.Set("Issuer", new ProtectedString(true, credential.Issuer));
					entry.Strings.Set("Updated", new ProtectedString(true, DateTimeOffset.UtcNow.ToString("O")));
					mDatabase.Save(mStatusLogger);
					return;
				}
			}

			foreach (var entry in group.Entries) {

				var name = entry.Strings.ReadSafe("Title")?.Trim();

				if (String.Equals(credential.Name.Trim(), name, StringComparison.OrdinalIgnoreCase)) {
					throw new Exception($"Credential already exists, name: '{name}'");
				}
			}

			group.Entries.Add(credential.ToKeePassEntry());

			mDatabase.Save(mStatusLogger);
		}

		public virtual void Remove(CredentialModel credential) {

			if (mDatabase == null) {
				return;
			}

			var group = GetOrCreateGroup(mGroupName);

			if (group == null) {
				throw new Exception($"An error occured while retrieving '{mGroupName}'");
			}

			foreach (var entry in group.Entries) {

				var id = entry.Strings.ReadSafe("Id")?.Trim();

				if (String.Equals(credential.Id, id, StringComparison.OrdinalIgnoreCase)) {

					group.Entries.Remove(entry);
					mDatabase.Save(mStatusLogger);
					break;
				}
			}
		}

		protected virtual PwGroup GetOrCreateGroup(string name) {

			var group = mDatabase
				.RootGroup
				.Groups
				.FirstOrDefault(s => String.Equals(s.Name, name, StringComparison.Ordinal));

			if (group != null) {
				return group;
			}

			mDatabase.RootGroup.AddGroup(
				new PwGroup(true, true, name, PwIcon.Folder), true);
			mDatabase.Save(mStatusLogger);

			return mDatabase
				.RootGroup
				.Groups
				.FirstOrDefault(s => String.Equals(s.Name, name, StringComparison.Ordinal));
		}

	}

}
