// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using BlackbirdSql.Core.Ctl.Diagnostics;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services.SupportEntities;




namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										TConnectionUIProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionUIProperties"/> interface
/// </summary>
// =========================================================================================================
public class TConnectionUIProperties : TConnectionProperties
{

	public TConnectionUIProperties() : base()
	{
		// Tracer.Trace(GetType(), "TConnectionUIProperties.TConnectionUIProperties()");
	}

	public TConnectionUIProperties(IVsDataProvider site) : base(site)
	{
		// Tracer.Trace(GetType(), "TConnectionUIProperties.TConnectionUIProperties(IVsDataProvider)", "Site type: {0}", site.GetType());
	}
}
