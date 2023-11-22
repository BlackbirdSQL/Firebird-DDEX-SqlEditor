// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TConnectionProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionProperties"/> interface.
/// </summary>
// =========================================================================================================
public class TConnectionProperties : TAbstractConnectionProperties
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
			foreach (Describer describer in CsbAgent.Describers.Mandatory)
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



	public override object this[string key]
	{
		get
		{
			// Tracer.Trace(GetType(), "get this[]", "key: {0}, get value: {1}, csb type: {2}", key, base[key], ConnectionStringBuilder.GetType().FullName);
			return base[key];
		}
		set
		{
			// Tracer.Trace(GetType(), "set this[]", "key: {0} set value: {1}, csb type: {2}", key, value, ConnectionStringBuilder.GetType().FullName);
			base[key] = value;
		}
	}

	/*
	public override string[] GetSynonyms(string key)
	{
		Tracer.Trace(GetType(), "GetSynonyms()", "key: {0}", key);
		return CsbAgent.Describers.GetSynonyms(key).ToArray();
	}
	*/

	public object GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(ConnectionStringBuilder, editorBaseType, noCustomTypeDesc: true);
	}

	public TypeConverter GetConverter()
	{
		return TypeDescriptor.GetConverter(ConnectionStringBuilder, noCustomTypeDesc: true);
	}

}
