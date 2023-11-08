// Microsoft.SqlServer.Tools.Extensibility, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Tools.Extensibility.ExtensionDescriptor<T,TMetadata>

using System;
using System.Reflection;
using BlackbirdSql.Core.Ctl.Interfaces;

namespace BlackbirdSql.Core.Model;

internal class ExtensionDescriptor<T, TMetadata> where TMetadata : IBStandardMetadata
{
	private readonly Lazy<T, TMetadata> _lazyExport;

	public T Extension => _lazyExport.Value;

	public TMetadata Metadata => _lazyExport.Metadata;

	public string ExtensionId => Metadata.Id;

	public string DisplayMember => Metadata.DisplayMember;

	public string ExtensionAssemblyName => Extension.GetType().Module.Name;

	public ExtensionDescriptor(Lazy<T, TMetadata> lazyExport)
	{
		_lazyExport = lazyExport ?? throw new ArgumentNullException("lazyExport");
	}

	public Type GetExtensionType()
	{
		return Extension.GetType();
	}

	public Assembly GetExtensionAssembly()
	{
		return Extension.GetType().Assembly;
	}

	/*
	public bool HasVersionInfo()
	{
		return !string.IsNullOrWhiteSpace(Metadata.Version);
	}
	*/

	/*
	public bool TryGetExtensionVersion(out Version version)
	{
		return Version.TryParse(Metadata.Version, out version);
	}
	*/

	/*
	public bool IsExtensionVersionCompatible(Version expectedMinVersion)
	{
		if (expectedMinVersion == null)
		{
			ArgumentNullException ex = new("expectedMinVersion");
			Diag.Dug(ex);
			throw ex;
		}

		if (HasVersionInfo() && TryGetExtensionVersion(out var version))
		{
			if (version.Major == expectedMinVersion.Major)
			{
				return version.Minor >= expectedMinVersion.Minor;
			}

			return false;
		}

		return false;
	}
	*/
}
