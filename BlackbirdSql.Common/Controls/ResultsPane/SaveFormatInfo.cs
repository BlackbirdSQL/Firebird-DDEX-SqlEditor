// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.SaveFormatInfo

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane;




using BlackbirdSql.Common.Controls.Enums;

namespace BlackbirdSql.Common.Controls.ResultsPane;


public class SaveFormatInfo
{
	public EnGridSaveFormats SaveFormat { get; set; }

	public string Description { get; set; }

	public SaveFormatInfo(EnGridSaveFormats saveFormat, string description)
	{
		SaveFormat = saveFormat;
		Description = description;
	}
}
