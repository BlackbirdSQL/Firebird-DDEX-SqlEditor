// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyWindow.PropertiesWindowWithoutConnection

using BlackbirdSql.Shared.Ctl.ComponentModel;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Controls.PropertiesWindow;


internal class DisconnectedPropertiesWindow : AbstractPropertiesWindow
{
	private static readonly string _NoConnection = ControlsResources.PropertiesWindow_Disconnected;

	private static DisconnectedPropertiesWindow _Instance = null;

	internal static DisconnectedPropertiesWindow Instance
	{
		get
		{
			return _Instance ??= new DisconnectedPropertiesWindow();
		}
	}

	[GlobalizedCategory("PropertyWindowCurrentConnectionParameters")]
	[GlobalizedDescription("PropertyWindowStatusDescription")]
	[GlobalizedDisplayName("PropertyWindowStatusDisplayName")]
	internal string Status => _NoConnection;

	private DisconnectedPropertiesWindow()
	{
	}

	public override string GetClassName()
	{
		return AttributeResources.PropertyWindowQueryWindowOptions;
	}
}
