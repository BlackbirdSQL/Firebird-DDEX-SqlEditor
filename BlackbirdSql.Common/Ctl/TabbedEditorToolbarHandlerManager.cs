// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabbedEditorToolbarHandlerManager
using System;
using System.Collections.Generic;
using BlackbirdSql.Common.Interfaces;



namespace BlackbirdSql.Common.Ctl;

public class TabbedEditorToolbarHandlerManager
{
	private readonly Dictionary<Type, Dictionary<GuidId, ITabbedEditorToolbarCommandHandler>> _mappings = new Dictionary<Type, Dictionary<GuidId, ITabbedEditorToolbarCommandHandler>>();

	public void AddMapping(Type tabbedEditorType, ITabbedEditorToolbarCommandHandler commandHandler)
	{
		if (!_mappings.ContainsKey(tabbedEditorType))
		{
			_mappings.Add(tabbedEditorType, new Dictionary<GuidId, ITabbedEditorToolbarCommandHandler>());
		}
		Dictionary<GuidId, ITabbedEditorToolbarCommandHandler> dictionary = _mappings[tabbedEditorType];
		GuidId guidId = commandHandler.GuidId;
		if (dictionary.ContainsKey(guidId))
		{
			_ = dictionary[guidId];
		}
		else
		{
			dictionary.Add(guidId, commandHandler);
		}
	}

	public bool TryGetCommandHandler(Type tabbedEditorType, GuidId guidId, out ITabbedEditorToolbarCommandHandler commandHandler)
	{
		commandHandler = null;
		bool result = false;
		if (_mappings.TryGetValue(tabbedEditorType, out var value))
		{
			result = value.TryGetValue(guidId, out commandHandler);
		}
		return result;
	}


}
