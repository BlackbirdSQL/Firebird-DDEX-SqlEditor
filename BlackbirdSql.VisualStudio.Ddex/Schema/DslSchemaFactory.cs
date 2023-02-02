/*
 *	This is an override of the FirebirdClient Schema
 *	We're maintaining the same structure so that it's easy to overload any GetSchema's that may need it.
 *	We still use the original Firebird metadata manifest pulled from the Firebird assembly
 *	
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex.Schema;


// Error suppression
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]


internal sealed class DslSchemaFactory
{
	#region Static Members

	private static readonly string ResourceName = "FirebirdSql.Data.Schema.FbMetaData.xml";

	#endregion

	#region Constructors

	private DslSchemaFactory()
	{ }

	#endregion

	#region Methods

	// Schema factory to handle custom collections
	public static DataTable GetSchema(FbConnection connection, string collectionName, string[] restrictions)
	{
		Diag.Trace();
		switch (collectionName)
		{
			case "Columns":
			case "ViewColumns":
			case "IndexColumns":
			case "FunctionArguments":
				break;
			default:
				return connection.GetSchema(collectionName, restrictions);
		}


		var filter = string.Format("CollectionName = '{0}'", collectionName);
		var ds = new DataSet();

		Assembly assembly = typeof(FirebirdClientFactory).Assembly;
		if (assembly == null)
		{
			DllNotFoundException ex = new(typeof(FirebirdClientFactory).Name + " class assembly not found");
			Diag.Dug(ex);
			throw ex;
		}

		var xmlStream = assembly.GetManifestResourceStream(ResourceName);
		if (xmlStream == null)
		{
			NullReferenceException ex = new NullReferenceException("Resource not found: " + ResourceName);
			Diag.Dug(ex);
			throw ex;
		}

		var oldCulture = Thread.CurrentThread.CurrentCulture;

		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			// ReadXml contains error: http://connect.microsoft.com/VisualStudio/feedback/Validation.aspx?FeedbackID=95116
			// that's the reason for temporarily changing culture
			ds.ReadXml(xmlStream);
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		var collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

		if (collection.Length != 1)
		{
			throw new NotSupportedException("Unsupported collection name.");
		}

		if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
		{
			throw new InvalidOperationException("The number of specified restrictions is not valid.");
		}

		if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
		{
			throw new InvalidOperationException("Incorrect restriction definition.");
		}

		switch (collection[0]["PopulationMechanism"].ToString())
		{
			case "PrepareCollection":
				return PrepareCollection(connection, collectionName, restrictions);

			case "DataTable":
				return ds.Tables[collection[0]["PopulationString"].ToString()].Copy();

			case "SQLCommand":
				return SqlCommandSchema(connection, collectionName, restrictions);

			default:
				throw new NotSupportedException("Unsupported population mechanism");
		}
	}

	#endregion

	#region Private Methods

	private static DataTable PrepareCollection(FbConnection connection, string collectionName, string[] restrictions)
	{
		DslSchema returnSchema = collectionName.ToUpperInvariant() switch
		{
			"COLUMNS" => new DslColumns(),
			"VIEWCOLUMNS" => new DslViewColumns(),
			"INDEXCOLUMNS" => new DslIndexColumns(),
			"FUNCTIONARGUMENTS" => new DslFunctionArguments(),
			_ => ((Func<DslSchema>)(() =>
				{
					NotSupportedException exbb = new("The specified metadata collection is not supported.");
					Diag.Dug(exbb);
					throw exbb;
				}))(),
		};
		return returnSchema.GetSchema(connection, collectionName, restrictions);
	}



	private static Task<DataTable> PrepareCollectionAsync(FbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		DslSchema returnSchema = collectionName.ToUpperInvariant() switch
		{
			"COLUMNS" => new DslColumns(),
			"VIEWCOLUMNS" => new DslViewColumns(),
			"INDEXCOLUMNS" => new DslIndexColumns(),
			"FUNCTIONARGUMENTS" => new DslFunctionArguments(),
			_ => ((Func<DslSchema>)(() =>
				{
					NotSupportedException exbb = new("The specified metadata collection is not supported.");
					Diag.Dug(exbb);
					throw exbb;
				}))(),
		};
		return returnSchema.GetSchemaAsync(connection, collectionName, restrictions, cancellationToken);
	}

	private static DataTable SqlCommandSchema(FbConnection connection, string collectionName, string[] restrictions)
	{
		throw new NotImplementedException();
	}
	private static Task<DataTable> SqlCommandSchemaAsync(FbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	#endregion
}
