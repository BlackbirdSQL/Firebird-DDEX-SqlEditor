// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.VisualStudio.Ddex.Interfaces;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


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
public sealed class TProviderObjectFactory : DataProviderObjectFactory, IBProviderObjectFactory
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TProviderObjectFactory
	// ---------------------------------------------------------------------------------

	public TProviderObjectFactory() : base()
	{
		// Tracer.Trace(typeof(TProviderObjectFactory), ".ctor");
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
		// Tracer.Trace(GetType(), "CreateObject", "objType: {0}", objType.Name);

		/* Uncomment this and change PackageSupportedObjects._UseFactoryOnly to true to debug implementations
		 * Don't forget to do the same for TConnectionSupport if you do.
		 * 
		if (objType == typeof(IVsDataConnectionSupport))
		{
			// Tracer.Trace();
			return new TConnectionSupport();
		}
		else if (objType == typeof(IVsDataConnectionUIControl))
		{
			// Tracer.Trace();
			return new TConnectionUIControl();
		}
		else if (objType == typeof(IVsDataConnectionPromptDialog))
		{
			// Tracer.Trace();
			return new TConnectionPromptDialog();
		}
		else if (objType == typeof(IVsDataConnectionProperties))
		{
			// Tracer.Trace();
			return new TConnectionProperties();
		}
		else if (objType == typeof(IVsDataConnectionUIProperties))
		{
			// Tracer.Trace();
			return new TConnectionUIProperties();
		}
		else if (objType == typeof(IVsDataObjectIdentifierResolver))
		{
			// Tracer.Trace();
			return new TObjectIdentifierResolver((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataObjectSupport))
		{
			// Tracer.Trace();
			return new TObjectSupport((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataSourceInformation))
		{
			// Tracer.Trace();
			return new TSourceInformation();
		}
		else if (objType == typeof(IVsDataViewSupport))
		{
			// Tracer.Trace();
			return new DataViewSupport("BlackbirdSql.VisualStudio.Ddex.Ctl.TViewSupport", typeof(ProviderObjectFactory).Assembly);
			// return new TViewSupport();
		}
		else if (objType == typeof(IVsDataConnectionEquivalencyComparer))
		{
			// Tracer.Trace();
			return new TConnectionEquivalencyComparer();
		}
		*/


		// Tracer.Trace(objType.FullName + " is not supported");

		return null;
	}


	#endregion Method Implementations

}
