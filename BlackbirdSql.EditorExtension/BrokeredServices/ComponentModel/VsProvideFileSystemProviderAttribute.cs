// Microsoft.VisualStudio.Shell.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Shell.ProvideFileSystemProviderAttribute

using System;
using Microsoft;
using Microsoft.VisualStudio.Shell.ServiceBroker;


namespace BlackbirdSql.BrokeredServices.ComponentModel;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class VsProvideFileSystemProviderAttribute : ProvideBrokeredServiceAttribute
{
	public static class RegValueNames
	{
		public const string Scheme = "FileSystemUriScheme";

		public const string UIContextGuid = "FileSystemProviderUIContextGuid";

		public const string IsDisplayInfoProvider = "FileSystemProviderIsDisplayInfoProvider";

		public const string IsRemoteProvider = "FileSystemProviderIsRemoteProvider";
	}

	[Obsolete("Use RegValueNames.Schemeinstead")]
	public const string SchemeRegValue = "FileSystemUriScheme";

	public string Scheme { get; }

	public Guid UIContextGuid { get; set; }

	public bool IsDisplayInfoProvider { get; set; }

	public bool IsRemoteProvider { get; set; } = true;


	public VsProvideFileSystemProviderAttribute(string scheme, string name)
		: this(scheme, name, null)
	{
	}

	public VsProvideFileSystemProviderAttribute(string scheme, string name, string version)
		: base(name, version)
	{
		Requires.NotNullOrWhiteSpace(scheme, "scheme");
		if (scheme.Contains(" "))
		{
			throw new ArgumentException("The scheme must not contain whitespace", "scheme");
		}
		if (scheme.EndsWith(Uri.SchemeDelimiter, StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("The scheme must not include the trailing schema delimiter \"" + Uri.SchemeDelimiter + "\".", "scheme");
		}

		Scheme = scheme;
		UIContextGuid = Guid.Empty;
	}

	protected override void SetRegistryValues(RegistrationContext context, Key key)
	{
		base.SetRegistryValues(context, key);
		key.SetValue("FileSystemUriScheme", Scheme);
		key.SetValue("FileSystemProviderIsDisplayInfoProvider", IsDisplayInfoProvider);
		key.SetValue("FileSystemProviderIsRemoteProvider", IsRemoteProvider);
		if (UIContextGuid != Guid.Empty)
		{
			key.SetValue("FileSystemProviderUIContextGuid", UIContextGuid.ToString());
		}
	}
}
