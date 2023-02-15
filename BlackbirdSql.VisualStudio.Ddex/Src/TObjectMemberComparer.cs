//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using Microsoft.VisualStudio.Debugger.Interop;



namespace BlackbirdSql.VisualStudio.Ddex;


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
internal sealed class TObjectMemberComparer : DataObjectMemberComparer
{
	/* For debug trace */
	public TObjectMemberComparer() : base()
	{
		// Diag.Trace();
	}

	public TObjectMemberComparer(IVsDataConnection dataConnection) : base(dataConnection)
	{
		// Diag.Trace();
	}

	public override int Compare(string typeName, string propertyName, object value1, object value2)
	{
		int result;

		// Diag.Trace();

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
				Diag.Trace("RENAMING value1 SYSTEM TABLE");
				value1 = "SYSTEM TABLE";
			}
			if ((string)value2 == "SYSTEM_TABLE")
			{
				Diag.Trace("RENAMING value2 SYSTEM TABLE");
				value2 = "SYSTEM TABLE";
			}
		}

		try
		{
			result = base.Compare(typeName, propertyName, value1, value2);
			Diag.Trace("typeName: " + typeName + " propertyName: " + propertyName + " value1: " + (value1 == null ? "null" : value1.ToString()) + " value2: " + (value2 == null ? "null" : value2.ToString()) + " result: " + result);
		}
		catch (Exception e)
		{
			Diag.Dug(true, String.Format("Compare({0},propertyName={1}, {2}, {3}) | {4} - {5}.",
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
				throw new ArgumentNullException(nameof(typeName), "Compare(string typeName, object[] identifier, int identifierPart, object value)");
			}
			if (identifier == null)
			{
				throw new ArgumentNullException(nameof(identifier), "Compare(string typeName, object[] identifier, int identifierPart, object value)");
			}
			if (identifierPart < 0 || identifierPart >= identifier.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(identifierPart), "Compare(string typeName, object[] identifier, int identifierPart, object value)");
			}


			if (identifier[identifierPart] != null && !DBNull.Value.Equals(identifier[identifierPart]))
			{
				value1 = identifier[identifierPart] as string;

				if (value1 == null)
				{
					throw new ArgumentException("Compare(string typeName, object[] identifier, int identifierPart, object value)", "identifier[" + identifierPart.ToString() + "]");
				}
			}
			if (value != null && !DBNull.Value.Equals(value))
			{
				value2 = value as string;

				if (value2 == null)
				{
					throw new ArgumentException("Compare(string typeName, object[] identifier, int identifierPart, object value)", nameof(value));
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

			Diag.Trace("typeName: " + typeName + " identifierPart: " + identifierPart + " value1: " + (value1 == null ? "null" : value1.ToString()) + " value2: " + (value2 == null ? "null" : value2.ToString()) + " result: " + result);

		}
		catch (Exception e)
		{
			Diag.Dug(true, String.Format("Compare({0},identifier={1},{2},{3}) | {4} - {5}.",
				typeName, identifier, identifierPart, value, e.Source, e.Message));
			throw;
		}


		return result > 0 ? 1 : (result < 0 ? -1 : 0);

#nullable restore
	}

}
