﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.EditorContextSettings

using System;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Ctl.Enums;

namespace BlackbirdSql.Common.Ctl.Config;

public sealed class EditorContextSettings : IBEditorContextSettings, ICloneable
{
	public static class Defaults
	{
		public static readonly EnStatusBarPosition StatusBarPosition = EnStatusBarPosition.Bottom;
	}

	private EnStatusBarPosition? _statusBarPosition;

	public EnStatusBarPosition StatusBarPosition
	{
		get
		{
			if (!_statusBarPosition.HasValue)
			{
				return Defaults.StatusBarPosition;
			}

			return _statusBarPosition.Value;
		}
		set
		{
			_statusBarPosition = value;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public void ResetToDefault()
	{
		_statusBarPosition = Defaults.StatusBarPosition;
	}
}