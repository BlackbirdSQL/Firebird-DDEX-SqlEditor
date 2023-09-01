// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Text.Operations;

namespace BlackbirdSql.Common.Interfaces;


public interface IBLanguageExtensionAsyncPackage : IOleComponent
{


	IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; }

	LanguageService LanguageService { get; }

	ITextUndoHistoryRegistry TextUndoHistoryRegistry { get; }

	void PreferencesDispose();


}