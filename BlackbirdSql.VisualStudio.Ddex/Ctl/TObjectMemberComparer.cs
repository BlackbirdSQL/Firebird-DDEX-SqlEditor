// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										TObjectMemberComparer Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectMemberComparer"/> interface
/// </summary>
/// <remarks>
/// Implementation for debugging
/// </remarks>
// =========================================================================================================
public class TObjectMemberComparer : DataObjectMemberComparer
{
	/* For debug trace */
	public TObjectMemberComparer() : base()
	{
		// Tracer.Trace(GetType(), "TObjectMemberComparer.TObjectMemberComparer");
	}

	public TObjectMemberComparer(IVsDataConnection dataConnection) : base(dataConnection)
	{
		// Tracer.Trace(GetType(), "TObjectMemberComparer.TObjectMemberComparer(IVsDataConnection)");
	}

	public override int Compare(string typeName, string propertyName, object value1, object value2)
	{
		int result;

		// Tracer.Trace();

		try
		{
			// Table type hack
			if (typeName == "Table" && propertyName == "TABLE_TYPE")
			{
				// null means all tables
				if (value1 == null || value2 == null)
				{
					return 0;
				}

				if ((string)value1 == "SYSTEM_TABLE")
				{
					// Tracer.Trace("RENAMING value1 SYSTEM TABLE");
					value1 = "SYSTEM TABLE";
				}
				if ((string)value2 == "SYSTEM_TABLE")
				{
					// Tracer.Trace("RENAMING value2 SYSTEM TABLE");
					value2 = "SYSTEM TABLE";
				}
			}

			result = base.Compare(typeName, propertyName, value1, value2);
			// Tracer.Trace("typeName: " + typeName + " propertyName: " + propertyName + " value1: " + (value1 == null ? "null" : value1.ToString()) + " value2: " + (value2 == null ? "null" : value2.ToString()) + " result: " + result);
		}
		catch (Exception e)
		{
			Diag.StackException(String.Format("Compare({0},propertyName={1}, {2}, {3}) | {4} - {5}.",
				typeName, propertyName, value1, value2, e.Source, e.Message));
			throw;
		}


		return result;
	}
	

	public override int Compare(string typeName, object[] identifier, int identifierPart, object value)
	{
#nullable enable
		// base.Compare(typeName, identifier, identifierPart, value);

		int result;
		string? value1 = null, value2 = null;

		try
		{
			if (typeName == null)
			{
				ArgumentNullException ex = new(nameof(typeName), "Compare(string typeName, object[] identifier, int identifierPart, object value)");
				Diag.Dug(ex);
				throw ex;
			}
			if (identifier == null)
			{
				ArgumentNullException ex = new(nameof(identifier), "Compare(string typeName, object[] identifier, int identifierPart, object value)");
				Diag.Dug(ex);
				throw ex;
			}
			if (identifierPart < 0 || identifierPart >= identifier.Length)
			{
				ArgumentOutOfRangeException ex = new(nameof(identifierPart), "Compare(string typeName, object[] identifier, int identifierPart, object value)");
				Diag.Dug(ex);
				throw ex;
			}


			if (identifier[identifierPart] != null && !DBNull.Value.Equals(identifier[identifierPart]))
			{
				value1 = identifier[identifierPart] as string;

				if (value1 == null)
				{
					ArgumentException ex = new("Compare(string typeName, object[] identifier, int identifierPart, object value)", "identifier[" + identifierPart.ToString() + "]");
					Diag.Dug(ex);
					throw ex;
				}
			}
			if (value != null && !DBNull.Value.Equals(value))
			{
				value2 = value as string;

				if (value2 == null)
				{
					ArgumentException ex = new("Compare(string typeName, object[] identifier, int identifierPart, object value)", nameof(value));
					Diag.Dug(ex);
					throw ex;
				}
			}


			if (value1 == value2)
			{
				result = 0;
			}
			else if (value1 == null)
			{
				result = -1;
			}
			else if (value2 == null)
			{
				result = 1;
			}
			else
			{
				result = StringComparer.Ordinal.Compare(value1, value2);
			}

			// Tracer.Trace("typeName: " + typeName + " identifierPart: " + identifierPart + " value1: " + (value1 == null ? "null" : value1.ToString()) + " value2: " + (value2 == null ? "null" : value2.ToString()) + " result: " + result);

		}
		catch (Exception e)
		{
			Diag.StackException(String.Format("Compare({0},identifier={1},{2},{3}) | {4} - {5}.",
				typeName, identifier, identifierPart, value, e.Source, e.Message));
			throw;
		}


		return result > 0 ? 1 : (result < 0 ? -1 : 0);

#nullable restore
	}

}
