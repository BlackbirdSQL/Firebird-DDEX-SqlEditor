// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Common.IDesignerDocumentService

using System.Collections.Generic;

namespace BlackbirdSql.Common.Interfaces;


public interface IDesignerDocumentService
{
	uint GetPrimaryDocCookie();

	IEnumerable<uint> GetEditableDocuments();
}
