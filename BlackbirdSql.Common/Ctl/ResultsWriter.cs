﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.IO;
using BlackbirdSql.Common.Interfaces;

// using Microsoft.SqlServer.Management.QueryExecution;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane
namespace BlackbirdSql.Common.Ctl;


public abstract class ResultsWriter
{
	protected const char SlashR = '\r';

	protected const char SlashN = '\n';

	// protected const string C_SlashRSlashN = "\r\n";

	public abstract TextWriter TextWriter { get; }

	public void AppendNormal(string text)
	{
		AppendNormal(text, noCRLF: false);
	}

	public abstract void AppendNormal(string text, bool noCRLF);

	public void AppendError(string text)
	{
		AppendError(text, noCRLF: false);
	}

	public abstract void AppendError(string text, bool noCRLF);

	public void AppendError(string text, int line, ITextSpan textSpan)
	{
		AppendError(text, line, textSpan, noCRLF: false);
	}

	public abstract void AppendError(string text, int line, ITextSpan textSpan, bool noCRLF);

	public void AppendWarning(string text)
	{
		AppendWarning(text, noCRLF: false);
	}

	public abstract void AppendWarning(string text, bool noCRLF);

	public abstract void Flush();

	public virtual void Close()
	{
	}

	public virtual void Reset()
	{
	}
}
