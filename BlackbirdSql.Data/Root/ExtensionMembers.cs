// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Text.RegularExpressions;
using FirebirdSql.Data.FirebirdClient;

namespace BlackbirdSql;

// =========================================================================================================
//											ExtensionMembers Class
//
/// <summary>
/// Central class for package external class extension methods. 
/// </summary>
// =========================================================================================================
public static partial class ExtensionMembers
{
	/// <summary>
	/// Adds a parameter suffixed with an index to DbCommand.Parameters.
	/// </summary>
	/// <returns>The index of the parameter in DbCommand.Parameters else -1.</returns>
	public static int AddParameter(this FbCommand @this, string name, int index, object value)
	{
		// Catalog, Schema and TableType are no real restrictions
		// if (!name.EndsWith("Catalog") && !name.EndsWith("Schema") && name != "TableType")
		// {
		var pname = string.Format("@p{0}", index++);

		FbParameter parm = @this.Parameters.Add(pname, FbDbType.VarChar, 255);
		parm.Value = value;

		return index;
		// }

		// return -1;
	}


	/// <summary>
	/// Parses and converts a server version string to it's Version format.
	/// </summary>
	public static Version GetVersion(this FbConnection @this)
	{
		string stringVersion = @this.ServerVersion;

		var m = Regex.Match(stringVersion, @"\w{2}-\w(\d+\.\d+\.\d+\.\d+)");
		if (!m.Success)
			return null;
		return new Version(m.Groups[1].Value);
	}

}
