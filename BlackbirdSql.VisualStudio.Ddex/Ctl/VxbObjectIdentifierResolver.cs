// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbObjectIdentifierResolver Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectIdentifierResolver"/> interface
/// </summary>
// =========================================================================================================
public class VxbObjectIdentifierResolver : DataObjectIdentifierResolver
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbObjectIdentifierConverter
	// ---------------------------------------------------------------------------------


	public VxbObjectIdentifierResolver() : base()
	{
		// Evs.Trace(typeof(VxbObjectIdentifierResolver), ".ctor");
	}

	public VxbObjectIdentifierResolver(IVsDataConnection connection) : base(connection)
	{
		// Evs.Trace(typeof(VxbObjectIdentifierResolver), ".ctor(IVsDataConnection)");
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - VxbObjectIdentifierResolver
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
		// Evs.Trace(GetType(), "VxbObjectIdentifierResolver.ContractIdentifier", "typeName: {0}", typeName);


		if (typeName == null)
		{
			ArgumentNullException ex = new(nameof(typeName));
			Diag.Ex(ex);
			throw ex;
		}


		if (typeName == NativeDb.RootObjectTypeName)
		{
			return base.ContractIdentifier(typeName, fullIdentifier);
		}


		int length = NativeDb.GetObjectTypeIdentifierLength(typeName);
		if (length == -1)
		{
			NotSupportedException ex = new();
			Diag.Ex(ex);
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

		// Evs.Trace("typeName: " + typeName + " Dsl length: " + length + " Supplied length: " + fullIdentifier.Length + " Copy length:" + (length - fullIdentifier.Length));
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
		// Evs.Trace(GetType(), "VxbObjectIdentifierResolver.ExpandIdentifier", "typeName: {0}", typeName);

		if (typeName == null)
		{
			ArgumentNullException ex = new(nameof(typeName));
			Diag.Ex(ex);
			throw ex;
		}

		int length = NativeDb.GetObjectTypeIdentifierLength(typeName);
		if (length == -1)
		{
			NotSupportedException ex = new();
			Diag.Ex(ex);
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
				Diag.Ex(ex);
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
