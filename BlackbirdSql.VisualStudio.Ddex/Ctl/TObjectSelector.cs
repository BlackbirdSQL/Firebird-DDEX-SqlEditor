// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys;
using BlackbirdSql.VisualStudio.Ddex.Model;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell.Interop;



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
		// Tracer.Trace(GetType(), "TObjectSelector.TObjectSelector()");
	}


	public TObjectSelector(IVsDataConnection connection) : base(connection)
	{
		// Tracer.Trace(GetType(), "TObjectSelector(IVsDataConnection)");
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
		// Tracer.Trace(GetType(), "SelectObjects()", "typeName: {0}", typeName);
		

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
			DbConnection connection = DbNative.CastToAssemblyConnection(lockedProviderObject);

			// VS glitch. Null if ado has picked up a project data model firebird assembly.
			if (connection == null)
			{
				// Tracer.Trace(GetType(), "SelectObjects()", "Glitch!!!!");
				connection = (DbConnection)DbNative.CreateDbConnection(Site.DecryptedConnectionString());
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
			Tracer.Warning(GetType(), "SelectObjects", "{0} error: {1}.", DbNative.DbEngineName, exf.Message);

			LinkageParser parser = LinkageParser.GetInstance((IDbConnection)lockedProviderObject);
			if (parser != null)
				LinkageParser.DisposeInstance((IDbConnection)lockedProviderObject, parser.Loaded);

			Site.Close();

			reader = new AdoDotNetTableReader(new DataTable());
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



	protected override DataTable GetSchema(DbConnection connection, string typeName, ref object[] restrictions, object[] parameters)
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

		DataTable schema = DslProviderSchemaFactory.GetSchema(connection, parameters[0].ToString(), restrictionArray);


		if (parameters.Length == 2 && parameters[1] is DictionaryEntry entry)
		{
			if (entry.Value is object[] array2)
			{
				IDictionary<string, object> mappings = GetMappings(array2);
				ApplyMappings(schema, mappings);
			}
		}

		return schema;
	}


	#endregion Method Implementations

}
