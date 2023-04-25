//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//


using System;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;

using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Configuration;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TProviderObjectFactory Class
//
/// <summary>
/// Implementation of <see cref="IVsDataProviderObjectFactory"/> interface
/// </summary>
/// <remarks>
/// For debugging only set <see cref="PackageSupportedObjects._UseFactoryOnly"/> to true to
/// utilize <see cref="CreateObject"/>
/// </remarks>
// =========================================================================================================
public sealed class TProviderObjectFactory : DataProviderObjectFactory, IProviderObjectFactory
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TProviderObjectFactory
	// ---------------------------------------------------------------------------------

	public TProviderObjectFactory() : base()
	{
		// Diag.Trace();
	}

	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TProviderObjectFactory
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates an instance of the specified DDEX support entity implemented by this DDEX
	/// provider.
	/// </summary>
	/// <param name="objType">A type of DDEX support entity.</param>
	/// <returns>
	/// An instance of the specified DDEX support entity implemented by the DDEX provider.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public override object CreateObject(Type objType)
	{
		/* Uncomment this and change PackageSupportedObjects._UseFactoryOnly to true to debug implementations
		 * Don't forget to do the same for TConnectionSupport if you do.
		 * 
		if (objType == typeof(IVsDataConnectionSupport))
		{
			// Diag.Trace();
			return new TConnectionSupport();
		}
		else if (objType == typeof(IVsDataConnectionUIControl))
		{
			// Diag.Trace();
			return new TConnectionUIControl();
		}
		else if (objType == typeof(IVsDataConnectionPromptDialog))
		{
			// Diag.Trace();
			return new TConnectionPromptDialog();
		}
		else if (objType == typeof(IVsDataConnectionProperties))
		{
			// Diag.Trace();
			return new TConnectionProperties();
		}
		else if (objType == typeof(IVsDataConnectionUIProperties))
		{
			// Diag.Trace();
			return new TConnectionUIProperties();
		}
		else if (objType == typeof(IVsDataObjectIdentifierResolver))
		{
			// Diag.Trace();
			return new TObjectIdentifierResolver((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataObjectSupport))
		{
			// Diag.Trace();
			return new TObjectSupport((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataSourceInformation))
		{
			// Diag.Trace();
			return new TSourceInformation();
		}
		else if (objType == typeof(IVsDataViewSupport))
		{
			// Diag.Trace();
			return new DataViewSupport("BlackbirdSql.VisualStudio.Ddex.TViewSupport", typeof(ProviderObjectFactory).Assembly);
			// return new TViewSupport();
		}
		else if (objType == typeof(IVsDataConnectionEquivalencyComparer))
		{
			// Diag.Trace();
			return new TConnectionEquivalencyComparer();
		}
		*/

		// Diag.Trace(objType.FullName + " is not supported");

		return null;
	}


	#endregion Method Implementations

}
