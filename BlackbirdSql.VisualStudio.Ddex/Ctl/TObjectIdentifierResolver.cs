// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.VisualStudio.Ddex.Model;

using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TObjectIdentifierResolver Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectIdentifierResolver"/> interface
/// </summary>
// =========================================================================================================
public class TObjectIdentifierResolver : DataObjectIdentifierResolver
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TObjectIdentifierConverter
	// ---------------------------------------------------------------------------------


	public TObjectIdentifierResolver() : base()
	{
		Tracer.Trace(GetType(), "TObjectIdentifierResolver.TObjectIdentifierResolver");
	}

	public TObjectIdentifierResolver(IVsDataConnection connection) : base(connection)
	{
		Tracer.Trace(GetType(), "TObjectIdentifierResolver.TObjectIdentifierResolver(IVsDataConnection)");
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TObjectIdentifierResolver
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Implementation of IVsDataObjectIdentifierResolver.ContractIdentifier
	/// Contracts an identifier for a data object with the specified type and complete
	/// identifier for use as a label or title.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override object[] ContractIdentifier(string typeName, object[] fullIdentifier)
	{
		Tracer.Trace(GetType(), "TObjectIdentifierResolver.ContractIdentifier", "typeName: {0}", typeName);


		if (typeName == null)
		{
			ArgumentNullException ex = new("typeName");
			Diag.Dug(ex);
			throw ex;
		}


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
		Tracer.Trace(GetType(), "TObjectIdentifierResolver.ExpandIdentifier", "typeName: {0}", typeName);

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
