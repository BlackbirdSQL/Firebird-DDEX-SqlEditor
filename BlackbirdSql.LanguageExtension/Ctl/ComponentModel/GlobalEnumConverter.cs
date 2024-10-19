// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions2.GlobalEnumConverter
using System;
using BlackbirdSql.Core.Ctl.ComponentModel;


namespace BlackbirdSql.LanguageExtension.Ctl.ComponentModel;

/// <summary>
/// Localized dll globalized enum type convertor.
/// Resources are in Core.GlobalizedDescriptionAttribute AttributeResources.resx.
/// This is because the enum converter uses GlobalizedDescriptionAttribute
/// for it's globalized resource strings.
/// </summary>
public class GlobalEnumConverter(Type type) : AbstractEnumConverter(type)
{
}
