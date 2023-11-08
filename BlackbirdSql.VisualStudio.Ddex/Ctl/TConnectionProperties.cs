// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Model;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TConnectionProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionProperties"/> interface
/// </summary>
// =========================================================================================================
public class TConnectionProperties : AdoDotNetConnectionProperties
{
	// Sanity checker.
	private readonly bool _Ctor = false;
	protected IVsDataProvider _InstanceSite = null;

	// ---------------------------------------------------------------------------------
	#region Property Accessors - TConnectionProperties
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Determines if the connection properties object is sufficiently complete (inclusive of password)
	/// to establish a database connection
	/// </summary>
	public override bool IsComplete
	{
		get
		{
			// Tracer.Trace(GetType(), "IsComplete:getter");

			foreach (Describer describer in CorePropertySet.Describers)
			{
				if (!describer.IsMandatory)
					continue;

				// Diag.Trace($"Getting display name for property: {describer.Name} Csb: {ConnectionStringBuilder}");

				string displayName = describer.DisplayName;

				if (displayName == null)
					continue;

				// Diag.Trace($"DisplayName: {displayName}.");

				if (!base.TryGetValue(displayName, out object value) || string.IsNullOrEmpty((string)value))
				{
					return false;
				}
			}

			foreach (Describer describer in ModelPropertySet.Describers)
			{
				if (!describer.IsMandatory)
					continue;

				// Diag.Trace($"Getting display name for property: {describer.Name}");

				string displayName = describer.DisplayName;

				if (displayName == null)
					continue;

				// Diag.Trace($"DisplayName: {displayName}.");

				if (!base.TryGetValue(displayName, out object value) || string.IsNullOrEmpty((string)value))
				{
					return false;
				}
			}


			return true;
		}
	}


	#endregion Property Accessors





	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TConnectionProperties
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// TConnectionProperties .ctor
	/// </summary>
	public TConnectionProperties() : base()
	{
		// Tracer.Trace(GetType(), "TConnectionProperties.TConnectionProperties()");
	}

	public TConnectionProperties(IVsDataProvider site) : base()
	{
		// Tracer.Trace(GetType(), "TConnectionProperties.TConnectionProperties(IVsDataProvider)");

		_InstanceSite = site;

		_Ctor = true;
		Site = site;
		_Ctor = false;
	}


	#endregion Constructors / Destructors




	public override void Reset()
	{
		// Tracer.Trace(GetType(), "Reset()");

		base.Reset();
	}



	protected override void OnSiteChanged(EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnSiteChanged()");

		if (!_Ctor)
		{
			base.OnSiteChanged(e);
			_InstanceSite = Site;
		}
	}

}
