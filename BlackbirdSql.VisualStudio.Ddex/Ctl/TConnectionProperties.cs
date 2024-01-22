// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Runtime.InteropServices;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TConnectionProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionProperties"/> interface.
/// </summary>
// =========================================================================================================
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread")]
public class TConnectionProperties : TAbstractConnectionProperties, IBDataConnectionProperties
{


	// ---------------------------------------------------------------------------------
	#region Fields - TConnectionProperties
	// ---------------------------------------------------------------------------------


	#endregion Fields





	// =================================================================================
	#region Property Accessors - TConnectionProperties
	// =================================================================================


	public CsbAgent Csa => ConnectionStringBuilder;

	public EnConnectionSource ConnectionSource
	{
		get
		{
			try
			{
				Diag.ThrowIfNotOnUIThread();

				string objectKind = Core.Controller.Instance.Dte.ActiveWindow.ObjectKind;
				string objectType = Core.Controller.Instance.Dte.ActiveWindow.Object.GetType().FullName;
				string appGuid = VSConstants.CLSID.VsTextBuffer_string;
				string seGuid = VSConstants.StandardToolWindows.ServerExplorer.ToString("B", CultureInfo.InvariantCulture);

				if (objectKind.Equals(seGuid, StringComparison.InvariantCultureIgnoreCase))
				{
					return EnConnectionSource.ServerExplorer;
				}
				else if (objectKind.Equals(appGuid, StringComparison.InvariantCultureIgnoreCase)
					&& objectType.Equals("System.ComponentModel.Design.DesignerHost",
						StringComparison.InvariantCultureIgnoreCase))
				{
					return EnConnectionSource.Application;
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			return EnConnectionSource.Session;
		}
	}

	/// <summary>
	/// Determines if the connection properties object is sufficiently complete (inclusive of password)
	/// to establish a database connection
	/// </summary>
	public override bool IsComplete
	{
		get
		{
			IEnumerable<Describer> describers = ConnectionSource == EnConnectionSource.Application
				? CsbAgent.PublicMandatoryKeys : CsbAgent.MandatoryKeys;

			foreach (Describer describer in describers)
			{
				if (!base.TryGetValue(describer.Key, out object value) || string.IsNullOrEmpty((string)value))
				{
					return false;
				}
			}


			return true;
		}
	}


	#endregion Property Accessors





	// =================================================================================
	#region Constructors / Destructors - TConnectionProperties
	// =================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// TConnectionProperties .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public TConnectionProperties() : base()
	{
		// Tracer.Trace(GetType(), "TConnectionProperties.TConnectionProperties()");
	}


	#endregion Constructors / Destructors




	// =================================================================================
	#region Methods - TConnectionProperties
	// =================================================================================

	protected override PropertyDescriptorCollection GetCsbAttributesProperties(Attribute[] attributes)
	{
		return TypeDescriptor.GetProperties(ConnectionStringBuilder, attributes);
	}

	protected override PropertyDescriptorCollection GetCsbProperties(DbConnectionStringBuilder csb)
	{
		return TypeDescriptor.GetProperties(csb);
	}


	#endregion Methods

}
