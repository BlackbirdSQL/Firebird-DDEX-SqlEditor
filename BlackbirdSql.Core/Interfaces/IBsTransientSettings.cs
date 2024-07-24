// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.PersistentSettings

namespace BlackbirdSql.Core.Interfaces;

public interface IBsTransientSettings
{
	object this[string name] { get; set; }

	bool PropertyExists(string name);
}
