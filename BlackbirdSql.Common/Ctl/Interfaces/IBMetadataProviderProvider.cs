﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.IMetadataProviderProvider
using System;
using System.Threading;
using BlackbirdSql.Common.Ctl.Parser.Interfaces;
using BlackbirdSql.Common.Model.Parsers.Interfaces;

namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBMetadataProviderProvider : IDisposable
{
	IBMetadataProvider MetadataProvider { get; }

	IBBinder Binder { get; }

	ManualResetEvent BuildEvent { get; }

	string DatabaseName { get; }

	IBinderQueue BinderQueue { get; }

	// ParseOptions CreateParseOptions();
}
