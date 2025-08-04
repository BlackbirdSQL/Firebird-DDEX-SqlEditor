// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.SaveFormatInfo

using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


internal class SaveFormatInfo(EnGridSaveFormats saveFormat, string description)
{
	internal EnGridSaveFormats SaveFormat { get; set; } = saveFormat;

	internal string Description { get; set; } = description;
}
