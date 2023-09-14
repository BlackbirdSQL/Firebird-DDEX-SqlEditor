#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Threading;

using Microsoft.SqlServer.Management.SqlParser.Binder;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Parser;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBMetadataProviderProvider : IDisposable
{
	IMetadataProvider MetadataProvider { get; }

	IBinder Binder { get; }

	ManualResetEvent BuildEvent { get; }

	string DatabaseName { get; }

	IBinderQueue BinderQueue { get; }

	ParseOptions CreateParseOptions();
}
