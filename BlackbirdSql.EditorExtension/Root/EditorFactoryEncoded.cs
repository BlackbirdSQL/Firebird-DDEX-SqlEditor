// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorFactoryWithEncoding

using System.Runtime.InteropServices;
using BlackbirdSql.Core;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.EditorExtension;

[Guid(SystemData.C_EditorEncodedFactoryGuid)]
[ProvideMenuResource("Menus.ctmenu", 1)]


// =========================================================================================================
//
//											EditorFactoryEncoded Class
//
/// <summary>
/// Editor Factory with encoding.
/// </summary>
// =========================================================================================================
public sealed class EditorFactoryEncoded : AbstractEditorFactory
{

	public EditorFactoryEncoded() : base(encoded: true)
	{
	}

}
