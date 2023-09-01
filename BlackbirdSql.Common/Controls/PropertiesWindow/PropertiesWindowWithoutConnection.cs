#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel;
// using Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities;

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyWindow

using BlackbirdSql.Common.ComponentModel;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Controls.PropertiesWindow
{
	public class PropertiesWindowWithoutConnection : AbstractPropertiesBase
	{
		private static readonly string _NoConnection = ControlsResources.QueryWindowDisconnected;

		private static PropertiesWindowWithoutConnection _Instance = null;

		public static PropertiesWindowWithoutConnection Instance
		{
			get
			{
				return _Instance ??= new PropertiesWindowWithoutConnection();
			}
		}

		[GlobalizedCategory("PropertyWindowCurrentConnectionParameters")]
		[GlobalizedDescription("PropertyWindowStatusDescription")]
		[GlobalizedDisplayName("PropertyWindowStatusDisplayName")]
		public string Status => _NoConnection;

		private PropertiesWindowWithoutConnection()
		{
		}

		public override string GetClassName()
		{
			return ControlsResources.PropertyWindowQueryWindowOptions;
		}
	}
}
