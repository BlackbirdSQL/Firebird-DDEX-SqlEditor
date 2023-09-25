// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.VisualStudio.Ddex.Model;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//											TObjectSelector Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSelector"/> enumerator interface
/// </summary>
// =========================================================================================================
public class TObjectSelector : AdoDotNetObjectSelector
{

	// ---------------------------------------------------------------------------------
	#region Variables - TObjectSelector
	// ---------------------------------------------------------------------------------


	#endregion Variables





	// =========================================================================================================
	#region Constructors / Destructors - TObjectSelector
	// =========================================================================================================


	public TObjectSelector()
	{
		Tracer.Trace(GetType(), "TObjectSelector.TObjectSelector");
	}


	public TObjectSelector(IVsDataConnection connection) : base(connection)
	{
		Tracer.Trace(GetType(), "TObjectSelector.TObjectSelector(IVsDataConnection)");
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TObjectSelector
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Data object enumeration
	/// </summary>
	/// <remarks>
	/// Also intercepts enumerations from the SE for <see cref="AbstractCommandProvider"/> and
	/// sets <see cref="CommandProperties.CommandNodeSystemType"/> to the correct node system object type
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected override IVsDataReader SelectObjects(string typeName, object[] restrictions,
		string[] properties, object[] parameters)
	{
		Tracer.Trace(GetType(), "TObjectSelector.SelectObjects", "typeName: {0}", typeName);

		try
		{
			if (typeName == null)
				throw new ArgumentNullException("typeName");

			if (parameters == null || parameters.Length < 1 || parameters.Length > 2 || parameters[0] is not string)
				throw new ArgumentNullException(Resources.ExceptionInvalidParameters);

			if (Site == null)
				throw new InvalidOperationException(Resources.ExceptionSiteIsNull);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}


		object lockedProviderObject = Site.GetLockedProviderObject();


		if (lockedProviderObject == null)
		{
			NotImplementedException ex = new("Site.GetLockedProviderObject()");
			Diag.Dug(ex);
			throw ex;
		}

		IVsDataReader reader;
		DataTable schema;

		try
		{
			if (lockedProviderObject is not FbConnection connection)
				throw new NotImplementedException("(DbConnection)Site.GetLockedProviderObject()");

			Site.EnsureConnected();


			schema = GetSchema(connection, typeName, restrictions, parameters);

			reader = new AdoDotNetTableReader(schema);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;

		}
		finally
		{
			Site.UnlockProviderObject();
		}

		return reader;
	}



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
		Tracer.Trace(GetType(), "TObjectSelector.GetSupportedRestrictions", "typeName: {0}", typeName);

		IList<string> list;

		try
		{
			list = base.GetSupportedRestrictions(typeName, parameters);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}


		// Table type hack
		if (typeName == "Table" || typeName == "SystemTable")
		{
			IList<string> array = new string[list.Count + 1];

			for (int i = 0; i < list.Count; i++)
				array[i] = list[i];

			array[list.Count] = "TABLE_TYPE";
			list = array;
		}
		return list;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Override for debugging
	/// </summary>
	/// <param name="typeName"></param>
	/// <param name="parameters"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	protected override IList<string> GetRequiredRestrictions(string typeName, object[] parameters)
	{
		Tracer.Trace(GetType(), "TObjectSelector.GetRequiredRestrictions", "typeName: {0}", typeName);

		IList<string> list;

		try
		{
			list = base.GetRequiredRestrictions(typeName, parameters);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}


		return list;
	}


	#endregion Method Implementations



	private DataTable GetSchema(DbConnection connection, string typeName, object[] restrictions, object[] parameters)
	{
		Tracer.Trace(GetType(), "TObjectSelector.GetSchema", "typeName: {0}", typeName);

		if (CommandProperties.CommandNodeSystemType != CommandProperties.EnNodeSystemType.None
			&& typeName == "Table" && parameters != null && parameters.Length > 0 && (string)parameters[0] == "Tables"
			&& (restrictions == null || restrictions.Length < 3 || (restrictions.Length > 2 && restrictions[2] == null)))
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
				case CommandProperties.EnNodeSystemType.User:
					restrictions[3] = "TABLE";
					break;
				case CommandProperties.EnNodeSystemType.System:
					restrictions[3] = "SYSTEM TABLE";
					break;
				default:
					restrictions[3] = null;
					break;
			}
		}
		
		CommandProperties.CommandNodeSystemType = CommandProperties.EnNodeSystemType.None;



		string[] array = null;

		if (restrictions != null)
		{
			array = new string[restrictions.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = restrictions[i]?.ToString();
			}
		}

		Tracer.Trace(GetType(), "TObjectSelector.GetSchema", "Ensuring connection typeName: {0}", typeName);

		Site.EnsureConnected();


		Tracer.Trace(GetType(), "TObjectSelector.GetSchema", "Calling GetSchema params.ToString, typeName: {0}", typeName);

		DataTable schema = DslProviderSchemaFactory.GetSchema((FbConnection)connection, parameters[0].ToString(), array);

		Tracer.Trace(GetType(), "TObjectSelector.GetSchema", "Returned from GetSchema params.ToString, typeName: {0}", typeName);


		if (parameters.Length == 2 && parameters[1] is DictionaryEntry entry)
		{
			if (entry.Value is object[] array2)
			{
				// Diag.Trace();
				IDictionary<string, object> mappings = GetMappings(array2);
				ApplyMappings(schema, mappings);
			}
		}

		return schema;
	}


}
