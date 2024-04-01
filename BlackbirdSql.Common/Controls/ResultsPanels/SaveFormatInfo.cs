// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.SaveFormatInfo

using BlackbirdSql.Common.Controls.Enums;



namespace BlackbirdSql.Common.Controls.ResultsPanels;


public class SaveFormatInfo(EnGridSaveFormats saveFormat, string description)
{
	public EnGridSaveFormats SaveFormat { get; set; } = saveFormat;

	public string Description { get; set; } = description;
}
