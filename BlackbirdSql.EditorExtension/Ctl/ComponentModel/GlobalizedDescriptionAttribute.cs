﻿using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.EditorExtension.Properties;


namespace BlackbirdSql.EditorExtension.Ctl.ComponentModel;

public sealed class GlobalizedDescriptionAttribute(string resourceName)
	: AbstractGlobalizedDescriptionAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
