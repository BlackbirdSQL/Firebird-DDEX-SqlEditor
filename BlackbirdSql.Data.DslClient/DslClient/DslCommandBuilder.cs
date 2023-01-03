/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/master/license.txt.
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
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;

using BlackbirdSql.Common;



namespace BlackbirdSql.Data.DslClient;



public sealed class DslCommandBuilder : DbCommandBuilder
{
	#region Static Methods

	public static void DeriveParameters(DslCommand command)
	{
		Diag.Dug();

		if (command.CommandType != CommandType.StoredProcedure)
		{
			Diag.Dug(true, "DeriveParameters only supports CommandType.StoredProcedure.");
			throw new InvalidOperationException("DeriveParameters only supports CommandType.StoredProcedure.");
		}

		var spName = command.CommandText.Trim();
		var quotePrefix = "\"";
		var quoteSuffix = "\"";

		if (spName.StartsWith(quotePrefix) && spName.EndsWith(quoteSuffix))
		{
			spName = spName[1..^1];
		}
		else
		{
			spName = spName.ToUpperInvariant();
		}

		// var paramsText = string.Empty;

		command.Parameters.Clear();

		var dataTypes = command.Connection.GetSchema("DataTypes").DefaultView;

		var spSchema = command.Connection.GetSchema(
			"ProcedureParameters", new string[] { null, null, spName });

		// SP has zero params. or not exist
		// so check whether exists, else thow exception
		if (spSchema.Rows.Count == 0)
		{
			if (command.Connection.GetSchema("Procedures", new string[] { null, null, spName }).Rows.Count == 0)
				throw new InvalidOperationException("Stored procedure doesn't exist.");
		}

		foreach (DataRow row in spSchema.Rows)
		{
			dataTypes.RowFilter = string.Format(
				"TypeName = '{0}'",
				row["PARAMETER_DATA_TYPE"]);

			var parameter = command.Parameters.Add(
				"@" + row["PARAMETER_NAME"].ToString().Trim(),
				DslDbType.VarChar);

			parameter.FbDbType = (DslDbType)dataTypes[0]["ProviderDbType"];

			parameter.Direction = (ParameterDirection)row["PARAMETER_DIRECTION"];

			parameter.Size = Convert.ToInt32(row["PARAMETER_SIZE"], CultureInfo.InvariantCulture);

			if (parameter.FbDbType == DslDbType.Decimal ||
				parameter.FbDbType == DslDbType.Numeric)
			{
				if (row["NUMERIC_PRECISION"] != DBNull.Value)
				{
					parameter.Precision = Convert.ToByte(row["NUMERIC_PRECISION"], CultureInfo.InvariantCulture);
				}
				if (row["NUMERIC_SCALE"] != DBNull.Value)
				{
					parameter.Scale = Convert.ToByte(row["NUMERIC_SCALE"], CultureInfo.InvariantCulture);
				}
			}
		}
	}

	#endregion

	#region Fields

	private EventHandler<DslRowUpdatingEventArgs> _rowUpdatingHandler;

	#endregion

	#region Properties

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string QuotePrefix
	{
		get { return base.QuotePrefix; }
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.QuotePrefix = value;
			}
			else
			{
				base.QuotePrefix = "\"";
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string QuoteSuffix
	{
		get { return base.QuoteSuffix; }
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.QuoteSuffix = value;
			}
			else
			{
				base.QuoteSuffix = "\"";
			}
		}
	}

	[DefaultValue(null)]
	public new DslDataAdapter DataAdapter
	{
		get { return (DslDataAdapter)base.DataAdapter; }
		set { base.DataAdapter = value; }
	}

	#endregion

	#region Constructors

	public DslCommandBuilder()
		: this(null)
	{
	}

	public DslCommandBuilder(DslDataAdapter adapter)
		: base()
	{
		DataAdapter = adapter;
		QuotePrefix = "\"";
		QuoteSuffix = "\"";
		ConflictOption = ConflictOption.OverwriteChanges;
	}

	#endregion

	#region DbCommandBuilder methods

	public new DslCommand GetInsertCommand()
	{
		return base.GetInsertCommand() as DslCommand;
	}

	public new DslCommand GetInsertCommand(bool useColumnsForParameterNames)
	{
		return base.GetInsertCommand(useColumnsForParameterNames) as DslCommand;
	}

	public new DslCommand GetUpdateCommand()
	{
		return base.GetUpdateCommand() as DslCommand;
	}

	public new DslCommand GetUpdateCommand(bool useColumnsForParameterNames)
	{
		return base.GetUpdateCommand(useColumnsForParameterNames) as DslCommand;
	}

	public new DslCommand GetDeleteCommand()
	{
		return base.GetDeleteCommand() as DslCommand;
	}

	public new DslCommand GetDeleteCommand(bool useColumnsForParameterNames)
	{
		return base.GetDeleteCommand(useColumnsForParameterNames) as DslCommand;
	}

	public override string QuoteIdentifier(string unquotedIdentifier)
	{
		if (unquotedIdentifier == null)
		{
			Diag.Dug(true, "Unquoted identifier parameter cannot be null");
			throw new ArgumentNullException(nameof(unquotedIdentifier), "Unquoted identifier parameter cannot be null");
		}

		return string.Format("{0}{1}{2}", QuotePrefix, unquotedIdentifier, QuoteSuffix);
	}

	public override string UnquoteIdentifier(string quotedIdentifier)
	{
		if (quotedIdentifier == null)
		{
			Diag.Dug(true, "Quoted identifier parameter cannot be null");
			throw new ArgumentNullException(nameof(quotedIdentifier), "Quoted identifier parameter cannot be null");
		}

		var unquotedIdentifier = quotedIdentifier.Trim();

		if (unquotedIdentifier.StartsWith(QuotePrefix))
		{
			unquotedIdentifier = unquotedIdentifier.Remove(0, 1);
		}
		if (unquotedIdentifier.EndsWith(QuoteSuffix))
		{
			unquotedIdentifier = unquotedIdentifier.Remove(unquotedIdentifier.Length - 1, 1);
		}

		return unquotedIdentifier;
	}

	#endregion

	#region Protected DbCommandBuilder methods

	protected override void ApplyParameterInfo(DbParameter p, DataRow row, StatementType statementType, bool whereClause)
	{
		var parameter = (DslParameter)p;

		parameter.Size = int.Parse(row["ColumnSize"].ToString());
		if (row["NumericPrecision"] != DBNull.Value)
		{
			parameter.Precision = byte.Parse(row["NumericPrecision"].ToString());
		}
		if (row["NumericScale"] != DBNull.Value)
		{
			parameter.Scale = byte.Parse(row["NumericScale"].ToString());
		}
		parameter.FbDbType = (DslDbType)row["ProviderType"];
	}

	protected override string GetParameterName(int parameterOrdinal)
	{
		return string.Format("@p{0}", parameterOrdinal);
	}

	protected override string GetParameterName(string parameterName)
	{
		return string.Format("@{0}", parameterName);
	}

	protected override string GetParameterPlaceholder(int parameterOrdinal)
	{
		return GetParameterName(parameterOrdinal);
	}

	protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
	{
#pragma warning disable IDE0038 // Use pattern matching
		if (adapter is DslDataAdapter)
		{
			_rowUpdatingHandler = new EventHandler<DslRowUpdatingEventArgs>(RowUpdatingHandler);
			((DslDataAdapter)adapter).RowUpdating += _rowUpdatingHandler;
		}
		else
		{
			Diag.Dug(true, $"Argument needs to be a {nameof(DslDataAdapter)}.", nameof(adapter));
			throw new ArgumentException($"Argument needs to be a {nameof(DslDataAdapter)}.", nameof(adapter));
		}
#pragma warning restore IDE0038 // Use pattern matching
	}

	#endregion

	#region Event Handlers

	private void RowUpdatingHandler(object sender, DslRowUpdatingEventArgs e)
	{
		base.RowUpdatingHandler(e);
	}

	#endregion
}
