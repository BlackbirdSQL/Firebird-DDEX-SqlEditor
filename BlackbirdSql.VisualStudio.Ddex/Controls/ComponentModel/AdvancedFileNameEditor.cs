// System.Windows.Forms.Design, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System.Windows.Forms.Design.FileNameEditor
using BlackbirdSql.Core.Controls.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Properties;



namespace BlackbirdSql.VisualStudio.Ddex.Controls.ComponentModel;


/// <summary>
/// File name lookup dialog editor using ParametersAttribute and the local
/// Properties.AttributeResources.resx for parameterizing the dialog. 
/// Where arg[0]: Title resource, arg[1] = Filter resource and optionally
/// arg[2]: CheckFileExists [default: false]. 
/// </summary>
public class AdvancedFileNameEditor : AbstractFileNameEditor
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
