//
// Plagiarized from Community.VisualStudio.Toolkit extension
//
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

using BlackbirdSql.Core.Controls.Events;
using BlackbirdSql.Core.Controls.Interfaces;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.Core.Properties;

using Microsoft.VisualStudio.Shell;

using Control = System.Windows.Forms.Control;
using TextBox = System.Windows.Forms.TextBox;


namespace BlackbirdSql.Core.Controls.Config;

// =========================================================================================================
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
// =========================================================================================================
[ComVisible(true)]
public abstract class AbstractSettingsPage<T> : AbstruseSettingsPage where T : AbstractSettingsModel<T>, new()
{

	// ---------------------------------------------------------------------------------
	#region Variables - AbstractSettingsPage
	// ---------------------------------------------------------------------------------


	protected AbstractSettingsModel<T> _Model;


	#endregion Variables




	// =========================================================================================================
	#region Property Accessors - AbstractSettingsPage
	// =========================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override object AutomationObject => _Model;


	#endregion Property Accessors




	// =========================================================================================================
	#region Constructors / Destructors - AbstractSettingsPage
	// =========================================================================================================


	public AbstractSettingsPage()
	{
		// Tracer.Trace(GetType(), ".ctor");
	}


	#endregion Constructors / Destructors




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
				{
					// Diag.Trace($"Getting default for {property.Name}: {defaultAttr.Value}.");
					return defaultAttr.Value;
				}
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
			_Model.Load();
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
