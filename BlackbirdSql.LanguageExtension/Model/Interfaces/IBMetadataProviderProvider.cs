// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.IMetadataProviderProvider
using System;
using System.Threading;
using Microsoft.SqlServer.Management.SqlParser.Binder;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Parser;


namespace BlackbirdSql.LanguageExtension.Model.Interfaces;


public interface IBMetadataProviderProvider : IDisposable
{
	IMetadataProvider MetadataProvider { get; }

	IBinder Binder { get; }

	ManualResetEvent BuildEvent { get; }

	string DatabaseName { get; }

	IBBinderQueue BinderQueue { get; }

	ParseOptions CreateParseOptions();
}
