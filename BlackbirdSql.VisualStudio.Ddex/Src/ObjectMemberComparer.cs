using System;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common;
using Microsoft.VisualStudio.Debugger.Interop;

namespace BlackbirdSql.VisualStudio.Ddex;

internal sealed class ObjectMemberComparer : DataObjectMemberComparer
{

	public ObjectMemberComparer(IVsDataConnection dataConnection)
	{
	}

	// This method can come out after debugging
	public override int Compare(string typeName, string propertyName, object value1, object value2)
	{
		int result;

		if (typeName == "Table" && propertyName == "TABLE_TYPE" && value1 != null && value2 != null
			&& (string)value1 == "SYSTEM_TABLE" && (string)value2 == "SYSTEM TABLE")
		{
			value2 = "SYSTEM_TABLE";
		}

		try
		{
			result = base.Compare(typeName, propertyName, value1, value2);
		}
		catch (Exception e)
		{
			Diag.Dug(true, String.Format("Compare({0},propertyName={1}, {2}, {3}) | {4} - {5}.",
				typeName, propertyName, value1, value2, e.Source, e.Message));
			throw;
		}

		// Diag.Trace("typeName: " + typeName + " propertyName: " + propertyName + " values: " + value1 + ":" + value2 + " result: " + result);

		return result;
	}
	

	public override int Compare(string typeName, object[] identifier, int identifierPart, object value)
	{
#nullable enable

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


		}
		catch (Exception e)
		{
			Diag.Dug(true, String.Format("Compare({0},identifier={1},{2},{3}) | {4} - {5}.",
				typeName, identifier, identifierPart, value, e.Source, e.Message));
			throw;
		}

		// Diag.Trace(typeName + ":" + value1 + ":" + value2 + " result: " + result);

		return result > 0 ? 1 : (result < 0 ? -1 : 0);

#nullable restore
	}



}
