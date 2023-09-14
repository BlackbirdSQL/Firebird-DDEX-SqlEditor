#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces
namespace BlackbirdSql.Common.Ctl.Interfaces;


public interface IBinderQueue
{
	IAsyncResult EnqueueUIThreadAction(Func<object> f);

	IAsyncResult EnqueueBindAction(Func<object> f);

	IAsyncResult EnqueueRecomputeMetadataAction(Func<object> f);
}
