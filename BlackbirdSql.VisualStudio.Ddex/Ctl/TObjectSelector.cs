// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.VisualStudio.Ddex.Properties;
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
public class TObjectSelector : TObjectSelectorTable
{

	// ---------------------------------------------------------------------------------
	#region Fields - TObjectSelector
	// ---------------------------------------------------------------------------------


	#endregion Fields





	// =========================================================================================================
	#region Constructors / Destructors - TObjectSelector
	// =========================================================================================================


	public TObjectSelector() : base()
	{
		// Tracer.Trace(typeof(TObjectSelector), ".ctor");
	}


	public TObjectSelector(IVsDataConnection connection) : base(connection)
	{
		// Tracer.Trace(typeof(TObjectSelector), ".ctor(IVsDataConnection)");
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
		// Tracer.Trace(GetType(), "SelectObjects()", "typeName: {0}, ConnectionSource: {1}.", typeName, RctManager.ConnectionSource);
		

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

		// There is only one place the Site can come from. However it is possible and does happen
		// that IVsDataExplorerConnection.Connection and Site do not reference the same object.
		// So we reference LinkageParser instances and RctEventSinks with the
		// IVsDataExplorerConnection root nodes. The references to the nodes is robust.
		// This is far less volatile than using IVsDataConnection.



		object lockedProviderObject = Site.GetLockedProviderObject();


		if (lockedProviderObject == null)
		{
			NotImplementedException ex = new("Site.GetLockedProviderObject()");
			Diag.Dug(ex);
			throw ex;
		}

		IVsDataReader reader;
		DataTable schema;

		bool connectionCreated = false;
		DbConnection connection = null;

		try
		{
			connection = NativeDb.CastToNativeConnection(lockedProviderObject);

			// VS glitch. Null if ado has picked up a project data model firebird assembly.
			if (connection == null)
			{
				// Tracer.Trace(GetType(), "SelectObjects()", "Glitch!!!!");
				connectionCreated = true;
				connection = (DbConnection)NativeDb.CreateDbConnection(Site.DecryptedConnectionString());
				connection.Open();
			}
			else
			{
				Site.EnsureConnected();
			}

			schema = GetSchema(connection, typeName, ref restrictions, parameters);

			// Tracer.Trace(GetType(), "SelectObjects()", "Typename: {0}, Count: {1}.", typeName, schema.Rows.Count);

			reader = new AdoDotNetTableReader(schema);
		}
		catch (DbException exf)
		{
			// It's most likely the connection has timed out. Do a hard reset on all linkages.

			Tracer.Warning(GetType(), "SelectObjects", "{0} error: {1}.", NativeDb.DbEngineName, exf.Message);

			Site.Close();

			reader = new AdoDotNetTableReader(new DataTable());
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"CollectionName: {parameters[0]}");
			throw;
		}
		finally
		{
			Site.UnlockProviderObject();

			if (connectionCreated)
			{
				if (connection.State == ConnectionState.Open)
					connection.Close();
				connection.Dispose();
			}
		}

		// Tracer.Trace(GetType(), "SelectObjects()", "Completed typeName: {0}", typeName);

		return reader;
	}



	protected override DataTable GetSchema(IDbConnection connection, string typeName, ref object[] restrictions, object[] parameters)
	{
		// Tracer.Trace(GetType(), "GetSchema()", "typeName: {0}", typeName);

		if (typeName == "Table")
			base.GetSchema(connection, typeName, ref restrictions, parameters);


		string[] restrictionArray = null;
		string restriction;

		if (restrictions != null)
		{
			restrictionArray = new string[restrictions.Length];

			for (int i = 0; i < restrictionArray.Length; i++)
			{
				restriction = restrictions[i] == DBNull.Value ? null : restrictions[i]?.ToString();
				restrictionArray[i] = restriction;
			}
		}

		string collectionName = parameters[0].ToString();
		DataTable schema = connection.GetSchemaEx(collectionName, restrictionArray);

		// Not used.
		if (parameters.Length > 1 && parameters[1] is DictionaryEntry entry
			&& entry.Value is object[] array)
		{
			IDictionary<string, object> mappings = GetMappings(array);
			ApplyMappings(schema, mappings);
		}
		

		return schema;
	}


	#endregion Method Implementations

}
