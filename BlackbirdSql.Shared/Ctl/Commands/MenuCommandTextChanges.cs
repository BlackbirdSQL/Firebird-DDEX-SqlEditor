// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.MenuCommandTextChanges

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Ctl.Commands;

[ComVisible(false)]


internal class MenuCommandTextChanges : MenuCommand, IBsMenuCommandTextChanges
{

	public MenuCommandTextChanges(EventHandler handler, CommandID command)
		: base(handler, command)
	{
	}


	private string _Text;


	public string Text
	{
		get { return _Text; }
		set { _Text = value; }
	}
}
