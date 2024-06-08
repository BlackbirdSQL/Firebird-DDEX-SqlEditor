// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.SaveFormats

using System;
using System.Collections.Generic;
using BlackbirdSql.Core;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Controls.ResultsPanels;


public class SaveFormats
{
	private const string C_FilterStringSeperator = "|";

	private readonly List<SaveFormatInfo> saveFormatList = [];

	public string FilterString
	{
		get
		{
			int num = 0;
			string[] array = new string[saveFormatList.Count];
			foreach (SaveFormatInfo saveFormat in saveFormatList)
			{
				array[num++] = saveFormat.Description;
			}
			return string.Join(C_FilterStringSeperator, array);
		}
	}

	public EnGridSaveFormats this[int index]
	{
		get
		{
			if (saveFormatList.Count <= index || index < 0)
			{
				ArgumentOutOfRangeException ex = new("index");
				Diag.Dug(ex);
				throw ex;
			}
			return saveFormatList[index].SaveFormat;
		}
	}

	public SaveFormats()
	{
		saveFormatList.Add(new SaveFormatInfo(EnGridSaveFormats.CommaSeparated, ControlsResources.SqlExportFromGridFilterCSV));
		saveFormatList.Add(new SaveFormatInfo(EnGridSaveFormats.TabSeparated, ControlsResources.SqlExportFromGridFilterTabDelimitted));
		saveFormatList.Add(new SaveFormatInfo(EnGridSaveFormats.TabSeparated, ControlsResources.SqlExportFromGridFilterAllFiles));
	}
}
