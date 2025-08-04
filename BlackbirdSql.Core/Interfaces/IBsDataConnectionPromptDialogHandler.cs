
using System;



namespace BlackbirdSql.Core.Interfaces;


internal interface IBsDataConnectionPromptDialogHandler : IDisposable
{
	string CompleteConnectionString { get; }
	string PublicConnectionString { set; }

	bool ShowDialog();
}
