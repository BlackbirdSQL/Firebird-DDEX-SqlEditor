// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Sys;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//											TObjectSelectorTable Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSelector"/> enumerator interface
/// </summary>
// =========================================================================================================
public class TObjectSelectorTable : AdoDotNetObjectSelector
{

	// ---------------------------------------------------------------------------------
	#region Fields - TObjectSelector
	// ---------------------------------------------------------------------------------


	#endregion Fields





	// =========================================================================================================
	#region Constructors / Destructors - TObjectSelectorTable
	// =========================================================================================================


	public TObjectSelectorTable() : base()
	{
	}


	public TObjectSelectorTable(IVsDataConnection connection) : base(connection)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TObjectSelectorTable
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Override included for TABLE_TYPE hack
	/// </summary>
	/// <param name="typeName"></param>
	/// <param name="parameters"></param>
	/// <returns>The list of supported reestrictions</returns>
	// ---------------------------------------------------------------------------------
	protected override IList<string> GetSupportedRestrictions(string typeName, object[] parameters)
	{
		// Tracer.Trace(GetType(), "GetSupportedRestrictions()", "typeName: {0}", typeName);

		IList<string> list = base.GetSupportedRestrictions(typeName, parameters);

		if (typeName != "Table")
			return list;


		// Table type hack
		IList<string> array = new string[list.Count + 1];

		for (int i = 0; i < list.Count; i++)
			array[i] = list[i];

		array[list.Count] = "TABLE_TYPE";
		list = array;

		return list;
	}



	protected virtual DataTable GetSchema(DbConnection connection, string typeName, ref object[] restrictions, object[] parameters)
	{
		// Tracer.Trace(GetType(), "GetSchema()", "typeName: {0}", typeName);

		if (parameters == null || parameters.Length == 0 || Cmd.IsNullValue(parameters[0]) || (string)parameters[0] != "Tables")
		{
			CommandProperties.CommandNodeSystemType = EnNodeSystemType.Undefined;
			return null;
		}

		if (restrictions == null || restrictions.Length < 4 || (restrictions.Length > 3 && Cmd.IsNullValue(restrictions[3])))
		{
			if (restrictions == null || restrictions.Length < 4)
			{
				object[] objs = new object[4];

				for (int i = 0; restrictions != null && i < restrictions.Length; i++)
					objs[i] = restrictions[i];

				restrictions = objs;
			}

			switch (CommandProperties.CommandNodeSystemType)
			{
				case EnNodeSystemType.User:
					restrictions[3] = "TABLE";
					break;
				case EnNodeSystemType.System:
					restrictions[3] = "SYSTEM TABLE";
					break;
				default:
					restrictions[3] = null;
					break;
			}
		}
		
		CommandProperties.CommandNodeSystemType = EnNodeSystemType.Undefined;

		return null;
	}


	#endregion Method Implementations

}
