using System;
using UnityEngine;

namespace Kit
{
	public static class BuildManager
	{
		public enum VersionPart
		{
			Major,
			Minor,
			Build,
			Revision
		}

		public static Version Version;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		public static void Initialize()
		{
			Version = new Version(Application.version);
		}

		public static bool MatchVersion(Version version)
		{
			return Version.Equals(version);
		}

		public static bool MatchVersion(string version)
		{
			return VersionString == version;
		}

		public static bool MatchVersion(int major, int minor)
		{
			return Version.Major == major && Version.Minor == minor;
		}

		public static bool MatchVersion(int major, int minor, int build)
		{
			return Version.Major == major && Version.Minor == minor && Version.Build == build;
		}

		public static bool MatchVersion(int major, int minor, int build, int revision)
		{
			return Version.Major == major && Version.Minor == minor && Version.Build == build && Version.Revision == revision;
		}

		public static string VersionString => Version.ToString();

		public static int GetVersionPart(VersionPart part)
		{
			switch (part)
			{
				case VersionPart.Major:
					return Version.Major;

				case VersionPart.Minor:
					return Version.Minor;

				case VersionPart.Build:
					return Version.Build;

				case VersionPart.Revision:
					return Version.Revision;

				default:
					return -1;
			}
		}
	}
}