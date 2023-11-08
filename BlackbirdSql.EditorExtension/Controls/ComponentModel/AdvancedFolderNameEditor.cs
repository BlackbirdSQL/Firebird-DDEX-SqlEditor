// System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// System.Diagnostics.Design.WorkingDirectoryEditor
using System.Security.Permissions;
using BlackbirdSql.Core.Controls.ComponentModel;
using BlackbirdSql.EditorExtension.Properties;


namespace BlackbirdSql.EditorExtension.Controls.ComponentModel;

/// <summary>
/// Folder name dialog editor using ParametersAttribute for parameterizing the dialog.
/// </summary>
[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
public class AdvancedFolderNameEditor : AbstractFolderNameEditor
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
