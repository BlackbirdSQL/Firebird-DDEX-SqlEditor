// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabbedEditorToolbarHandlerManager

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Ctl;


public class CommandMapper
{
	private readonly Dictionary<Type, Dictionary<CommandID, IBsCommandHandler>> _Mappings = [];



	public void AddMapping(Type tabbedWindowPaneType, IBsCommandHandler commandHandler)
	{
		if (!_Mappings.ContainsKey(tabbedWindowPaneType))
		{
			_Mappings.Add(tabbedWindowPaneType, []);
		}
		Dictionary<CommandID, IBsCommandHandler > dictionary = _Mappings[tabbedWindowPaneType];
		CommandID cmdId = commandHandler.CmdId;

		if (dictionary.ContainsKey(cmdId))
			_ = dictionary[cmdId];
		else
			dictionary.Add(cmdId, commandHandler);
	}



	public bool TryGetCommandHandler(Type tabbedWindowPaneType, CommandID cmdId, out IBsCommandHandler commandHandler)
	{
		commandHandler = null;
		bool result = false;

		if (_Mappings.TryGetValue(tabbedWindowPaneType, out Dictionary<CommandID, IBsCommandHandler> dictionary))
		{
			result = dictionary.TryGetValue(cmdId, out commandHandler);
		}

		return result;
	}


}
