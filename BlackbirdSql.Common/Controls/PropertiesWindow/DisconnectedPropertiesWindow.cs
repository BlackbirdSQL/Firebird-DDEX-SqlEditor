// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyWindow.PropertiesWindowWithoutConnection

using BlackbirdSql.Common.Ctl.ComponentModel;
using BlackbirdSql.Common.Properties;


namespace BlackbirdSql.Common.Controls.PropertiesWindow
{
	public class DisconnectedPropertiesWindow : AbstractPropertiesWindow
	{
		private static readonly string _NoConnection = ControlsResources.QueryWindowDisconnected;

		private static DisconnectedPropertiesWindow _Instance = null;

		public static DisconnectedPropertiesWindow Instance
		{
			get
			{
				return _Instance ??= new DisconnectedPropertiesWindow();
			}
		}

		[GlobalizedCategory("PropertyWindowCurrentConnectionParameters")]
		[GlobalizedDescription("PropertyWindowStatusDescription")]
		[GlobalizedDisplayName("PropertyWindowStatusDisplayName")]
		public string Status => _NoConnection;

		private DisconnectedPropertiesWindow()
		{
		}

		public override string GetClassName()
		{
			return AttributeResources.PropertyWindowQueryWindowOptions;
		}
	}
}
