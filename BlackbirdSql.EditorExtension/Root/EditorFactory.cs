// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorFactoryWithoutEncoding

using System.Runtime.InteropServices;
using BlackbirdSql.Core;



namespace BlackbirdSql.EditorExtension;

[Guid(SystemData.C_EditorFactoryGuid)]
// [ProvideMenuResource("Menus.ctmenu", 1)]


// =========================================================================================================
//
//												EditorFactory Class
//
/// <summary>
/// Editor Factory without encoding.
/// </summary>
// =========================================================================================================
public sealed class EditorFactory : AbstractEditorFactory
{

	public EditorFactory() : base(encoded: false)
	{
	}

}
