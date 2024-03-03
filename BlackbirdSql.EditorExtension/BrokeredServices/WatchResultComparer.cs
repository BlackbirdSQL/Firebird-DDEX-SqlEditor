// Microsoft.VisualStudio.Shell.UI.Internal, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.PlatformUI.Packages.FileSystem.WatchResultComparer
using System;
using System.Collections.Generic;
using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio.RpcContracts.FileSystem;


namespace BlackbirdSql.BrokeredServices;

internal class WatchResultComparer : IEqualityComparer<WatchResult>
{
	public static WatchResultComparer Instance { get; } = new WatchResultComparer();


	private static StringComparer SchemeComparer { get; } = StringComparer.OrdinalIgnoreCase;


	private WatchResultComparer()
	{
	}

	public bool Equals(WatchResult x, WatchResult y)
	{
		if (x == null)
		{
			return y == null;
		}
		if (y == null)
		{
			return false;
		}
		if (!SchemeComparer.Equals(x.Scheme, y.Scheme))
		{
			return false;
		}
		if (x.Cookie != y.Cookie)
		{
			return false;
		}
		return x.IsDirectory == y.IsDirectory;
	}

	public int GetHashCode(WatchResult result)
	{
		if (result == null)
		{
			return 0;
		}
		int hashCode = SchemeComparer.GetHashCode(result.Scheme);
		hashCode = HashHelpers.CombineHashes(hashCode, result.Cookie.GetHashCode());
		return HashHelpers.CombineHashes(hashCode, result.IsDirectory.GetHashCode());
	}
}
