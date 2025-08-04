//
// Original code plagiarized from Community.VisualStudio.Toolkit extension
//
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Model.Config;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Core.Controls.Config;


// =============================================================================================================
//										AbstractSettingsPage Class
//
/// <summary>
/// VS Options DialogPage base class.
/// Disclosure: This class exposes some PropertyGridView members with hidden access modifiers using
/// the Visual Studio's Reflection library, so that we can implement a few standard or comparable windows
/// functionality features like single-click check boxes, radio buttons and cardinal synonyms into the
/// DialogPage property grid.
/// Common cardinal synonyms include current culture min[imum], max[imum], unlimited, default etc.
/// </summary>
// =============================================================================================================
[ComVisible(true)]
public abstract class AbstractSettingsPage<T> : AbstruseSettingsPage where T : AbstractSettingsModel<T>, new()
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSettingsPage
	// ---------------------------------------------------------------------------------


	public AbstractSettingsPage() : base()
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractSettingsPage
	// =========================================================================================================


	protected AbstractSettingsModel<T> _Model;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractSettingsPage
	// =========================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override object AutomationObject => _Model;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AbstractSettingsPage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the default value of a property.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override object GetDefaultPropertyValue(PropertyDescriptor property)
	{
		lock (_LockObject)
		{
			foreach (Attribute customAttribute in property.PropertyType.GetCustomAttributes())
			{
				if (customAttribute is DefaultValueAttribute defaultAttr)
					return defaultAttr.Value;
			}
			return null;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads the settings from live storage. TBC!!!
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void LoadSettings()
	{
		lock (_LockObject)
			_Model.Load();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads the settings from storage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void LoadSettingsFromStorage()
	{
		lock (_LockObject)
		{
			_Model.Load();
		}
	}


	public override void LoadSettingsFromXml(IVsSettingsReader reader)
	{
		base.LoadSettingsFromXml(reader);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Override of the DialogPage ResetSettings method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void ResetSettings()
	{
		lock (_LockObject)
			_Model.LoadDefaults();


		base.ResetSettings();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Saves settings to live storage. TBC!!!
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void SaveSettings()
	{
		lock (_LockObject)
			_Model.Save();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Saves settings to internal storage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void SaveSettingsToStorage()
	{
		lock (_LockObject)
			_Model.Save();
	}


	#endregion Methods




	// =========================================================================================================
	#region Event Handling - AbstractSettingsPage
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Automation model reset setting event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnResetSettings(object sender, EventArgs e)
	{
		ResetSettings();
	}


	#endregion Event Handling


}
