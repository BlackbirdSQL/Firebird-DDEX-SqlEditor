#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Text;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Ctl;


public class SqlDebugBatchContext
{
	private static readonly object _LockObject = new object();

	private static SqlDebugBatchContext _Instance;

	public static SqlDebugBatchContext Instance
	{
		get
		{
			lock (_LockObject)
			{
				_Instance ??= new SqlDebugBatchContext();
			}

			return _Instance;
		}
	}

	public SQL_DEBUG_BATCH_CONTEXT BatchContext { get; set; }

	public string GetBatchDebugScript(string script, ITextSpan textSpan)
	{
		if (script == null)
		{
			ArgumentNullException ex = new("script");
			Diag.Dug(ex);
			throw ex;
		}

		if (textSpan == null)
		{
			ArgumentNullException ex = new("textSpan");
			Diag.Dug(ex);
			throw ex;
		}

		int value = textSpan.AnchorLine + textSpan.LineWithinTextSpan;
		int value2 = 0;
		if (textSpan.LineWithinTextSpan == 0)
		{
			value2 = textSpan.AnchorCol;
			if (textSpan.EndLine == textSpan.AnchorLine && textSpan.EndCol < textSpan.AnchorCol)
			{
				value2 = textSpan.EndCol;
			}
		}

		StringBuilder stringBuilder = new StringBuilder("-- Batch submitted through debugger: ");
		stringBuilder.Append(BatchContext.batchName);
		stringBuilder.Append('|');
		stringBuilder.Append(value);
		stringBuilder.Append('|');
		stringBuilder.Append(value2);
		stringBuilder.Append('|');
		stringBuilder.Append(BatchContext.batchFileName);
		stringBuilder.Append(Environment.NewLine);
		stringBuilder.Append(script);
		return stringBuilder.ToString();
	}
}
