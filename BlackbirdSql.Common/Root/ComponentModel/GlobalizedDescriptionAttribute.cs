#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.ComponentModel;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities
namespace BlackbirdSql.Common.ComponentModel;

public sealed class GlobalizedDescriptionAttribute : AbstractGlobalizedDescriptionAttribute
{
	public override System.Resources.ResourceManager ResMgr => ControlsResources.ResourceManager;

	public GlobalizedDescriptionAttribute(string resourceName) : base(resourceName)
	{
	}
}
