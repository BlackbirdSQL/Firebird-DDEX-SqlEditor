// Microsoft.VisualStudio.LiveShare.VslsFileSystemProvider.VSCore, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.LiveShare.FileSystemProvider.CustomProvideFileSystemProviderAttribute
using System;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.ServiceBroker;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class CustomProvideFileSystemProviderAttribute : ProvideBrokeredServiceAttribute
{
	public static class RegValueNames
	{
		public const string Scheme = "FileSystemUriScheme";

		public const string UIContextGuid = "FileSystemProviderUIContextGuid";

		public const string IsDisplayInfoProvider = "FileSystemProviderIsDisplayInfoProvider";
	}

	[Obsolete("Use RegValueNames.Schemeinstead")]
	public const string SchemeRegValue = "FileSystemUriScheme";

	public string Scheme { get; }

	public string? UIContextGuid { get; set; }

	public bool IsDisplayInfoProvider { get; set; }

	public CustomProvideFileSystemProviderAttribute(string scheme, string name)
		: this(scheme, name, null)
	{
	}

	public CustomProvideFileSystemProviderAttribute(string scheme, string name, string? version)
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
	}

	protected override void SetRegistryValues(RegistrationContext context, Key key)
	{
		base.SetRegistryValues(context, key);
		key.SetValue("FileSystemUriScheme", Scheme);
		key.SetValue("FileSystemProviderIsDisplayInfoProvider", IsDisplayInfoProvider);
		if (!string.IsNullOrEmpty(UIContextGuid))
		{
			key.SetValue("FileSystemProviderUIContextGuid", UIContextGuid);
		}
	}
}
