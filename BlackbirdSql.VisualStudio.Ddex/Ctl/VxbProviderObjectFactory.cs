// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.VisualStudio.Ddex.Interfaces;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbProviderObjectFactory Class
//
/// <summary>
/// Implementation of <see cref="IVsDataProviderObjectFactory"/> interface
/// </summary>
/// <remarks>
/// For debugging only set <see cref="PackageSupportedObjects._UseFactoryOnly"/> to true to
/// utilize <see cref="CreateObject"/>
/// </remarks>
// =========================================================================================================
public sealed class VxbProviderObjectFactory : DataProviderObjectFactory, IBsProviderObjectFactory
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbProviderObjectFactory
	// ---------------------------------------------------------------------------------

	public VxbProviderObjectFactory() : base()
	{
		// Evs.Trace(typeof(VxbProviderObjectFactory), ".ctor");
	}



	#endregion Constructors / Destructors



	// =========================================================================================================
	#region Method Implementations - VxbProviderObjectFactory
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
		Evs.Trace(GetType(), "CreateObject", $"objType: {objType.Name}");

		/* Uncomment this and change PackageSupportedObjects._UseFactoryOnly to true to debug implementations
		 * Don't forget to do the same for VxbConnectionSupport if you do.
		 * 
		if (objType == typeof(IVsDataConnectionSupport))
		{
			// Evs.Trace();
			return new VxbConnectionSupport();
		}
		else if (objType == typeof(IVsDataConnectionUIControl))
		{
			// Evs.Trace();
			return new VxbConnectionUIControl();
		}
		else if (objType == typeof(IVsDataConnectionPromptDialog))
		{
			// Evs.Trace();
			return new VxbConnectionPromptDialog();
		}
		else if (objType == typeof(IVsDataConnectionProperties))
		{
			// Evs.Trace();
			return new VxbConnectionProperties();
		}
		else if (objType == typeof(IVsDataConnectionUIProperties))
		{
			// Evs.Trace();
			return new VxbConnectionUIProperties();
		}
		else if (objType == typeof(IVsDataObjectIdentifierResolver))
		{
			// Evs.Trace();
			return new VxbObjectIdentifierResolver((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataObjectSupport))
		{
			// Evs.Trace();
			return new VxbObjectSupport((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataSourceInformation))
		{
			// Evs.Trace();
			return new VxbSourceInformation();
		}
		else if (objType == typeof(IVsDataViewSupport))
		{
			// Evs.Trace();
			return new DataViewSupport("BlackbirdSql.VisualStudio.Ddex.Ctl.VxbViewSupport", typeof(ProviderObjectFactory).Assembly);
			// return new VxbViewSupport();
		}
		else if (objType == typeof(IVsDataConnectionEquivalencyComparer))
		{
			// Evs.Trace();
			return new VxbEquivalencyComparer();
		}
		*/


		// Evs.Trace(objType.FullName + " is not supported");

		return null;
	}


	#endregion Method Implementations

}
