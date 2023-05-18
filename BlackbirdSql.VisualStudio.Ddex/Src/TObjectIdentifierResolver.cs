// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Schema;



namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TObjectIdentifierResolver Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectIdentifierResolver"/> interface
/// </summary>
// =========================================================================================================
internal class TObjectIdentifierResolver : DataObjectIdentifierResolver
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TObjectIdentifierConverter
	// ---------------------------------------------------------------------------------


	public TObjectIdentifierResolver() : base()
	{
		// Diag.Trace();
	}

	public TObjectIdentifierResolver(IVsDataConnection connection) : base(connection)
	{
		// Diag.Trace();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TObjectIdentifierResolver
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Contracts an identifier for a data object with the specified type and complete
	/// identifier.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override object[] ContractIdentifier(string typeName, object[] fullIdentifier)
	{
		if (typeName == null)
		{
			Diag.Dug(true, "Null argument: typeName");
			ArgumentNullException ex = new("typeName");
			Diag.Dug(ex);
			throw ex;
		}

		/*
		string str = "";

		if (fullIdentifier != null)
		{
			foreach (object item in fullIdentifier)
			{
				str += (item != null ? item.ToString() : "null") + ", ";
			}
		}
		// Diag.Trace(String.Format("typeName: {0} Identifiers: {1}", typeName, str));
		*/

		if (typeName == DslObjectTypes.Root)
		{
			return base.ContractIdentifier(typeName, fullIdentifier);
		}


		int length = DslObjectTypes.GetIdentifierLength(typeName);
		if (length == -1)
		{
			NotSupportedException ex = new();
			Diag.Dug(ex);
			throw ex;
		}
		object[] identifier = new object[length];

		fullIdentifier?.CopyTo(identifier, length - fullIdentifier.Length);

		if (identifier.Length > 0)
		{
			identifier[0] = null;
		}

		if (identifier.Length > 1)
		{
			identifier[1] = null;
		}

		// Diag.Trace("typeName: " + typeName + " Dsl length: " + length + " Supplied length: " + fullIdentifier.Length + " Copy length:" + (length - fullIdentifier.Length));
		return identifier;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Expands an identifier for a data object with the specified type and partial identifier.
	/// </summary>
	/// <param name="typeName"></param>
	/// <param name="partialIdentifier"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="NotSupportedException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	// ---------------------------------------------------------------------------------
	public override object[] ExpandIdentifier(string typeName, object[] partialIdentifier)
	{
		// Diag.Trace();
		// Diag.Trace(String.Format("ExpandIdentifier({0},...)", typeName));

		if (typeName == null)
		{
			ArgumentNullException ex = new("typeName");
			Diag.Dug(ex);
			throw ex;
		}

		int length = DslObjectTypes.GetIdentifierLength(typeName);
		if (length == -1)
		{
			NotSupportedException ex = new();
			Diag.Dug(ex);
			throw ex;
		}
		// Create an identifier array of the correct full length based on
		// the object type
		object[] identifier = new object[length];

		// If the input identifier is not null, copy it to the full
		// identifier array.  If the input identifier's length is less
		// than the full length we assume the more specific parts are
		// specified and thus copy into the rightmost portion of the
		// full identifier array.
		if (partialIdentifier != null)
		{
			if (partialIdentifier.Length > length)
			{
				InvalidOperationException ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			partialIdentifier.CopyTo(identifier, length - partialIdentifier.Length);
		}

		if (length > 0)
		{
			identifier[0] = null;
		}

		if (length > 1)
		{
			identifier[1] = null;
		}

		return identifier;
	}


	#endregion Method Implementations

}
