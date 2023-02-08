//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.IO;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TObjectSelectorRoot Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSelector"/> enumerator interface for the root node
/// </summary>
// =========================================================================================================
class TObjectSelectorRoot : AdoDotNetRootObjectSelector
{

	/// <summary>
	/// Enumerates the root object properties
	/// </summary>
	/// <param name="typeName"></param>
	/// <param name="restrictions"></param>
	/// <param name="properties"></param>
	/// <param name="parameters"></param>
	/// <returns>A data reader of the root object</returns>
	protected override IVsDataReader SelectObjects(string typeName, object[] restrictions, string[] properties, object[] parameters)
	{

		IVsDataReader reader;

		try
		{
			DbConnection connection = (DbConnection)Site.GetLockedProviderObject();

			DataTable schema = new DataTable
			{
				Locale = System.Globalization.CultureInfo.CurrentCulture,
				Columns =
				{
					{ "Server", typeof(string) },
					{ "Database", typeof(string) }
				},
				Rows =
				{
					new object[]
					{
						connection.DataSource,
						Path.GetFileNameWithoutExtension(connection.Database)
					}
				}
			};

			if (parameters?.Length > 1)
			{
				ApplyMappings(schema, GetMappings((object[])((DictionaryEntry)parameters[1]).Value));
			}

			reader = new AdoDotNetTableReader(schema);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw (ex);
		}
		finally
		{
			Site.UnlockProviderObject();
		}

		return reader;
	}

}
