// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.FileStreamWriter

using System;
using System.IO;
using System.Text;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Ctl.IO;


public sealed class FileStreamResultsWriter : AbstractResultsWriter
{
	private StreamWriter writer;

	public const int C_FileStreamWriterBufferSizeInBytes = 8192;

	public override TextWriter TextWriter => writer;

	public FileStreamResultsWriter(string path, bool append)
	{
		writer = new(path, append, Encoding.UTF8, C_FileStreamWriterBufferSizeInBytes)
		{
			AutoFlush = false
		};
	}

	public FileStreamResultsWriter(StreamWriter writer)
	{
		this.writer = writer;
	}

	private void AppendCommon(string text, bool noCRLF)
	{
		if (noCRLF || text.EndsWith("\r\n", StringComparison.Ordinal))
		{
			writer.Write(text);
		}
		else
		{
			writer.WriteLine(text);
		}
	}

	public override void AppendNormal(string text, bool noCRLF)
	{
		lock (this)
		{
			AppendCommon(text, noCRLF);
		}
	}

	public override void AppendError(string text, bool noCRLF)
	{
		lock (this)
		{
			AppendCommon(text, noCRLF);
		}
	}

	public override void AppendError(string text, int line, IBsTextSpan textSpan, bool noCRLF)
	{
		AppendError(text, noCRLF);
	}

	public override void AppendWarning(string text, bool noCRLF)
	{
		lock (this)
		{
			AppendCommon(text, noCRLF);
		}
	}

	public override void Close()
	{
		lock (this)
		{
			writer.Close();
			writer = null;
		}
	}

	public override void Flush()
	{
		lock (this)
		{
			writer.Flush();
		}
	}
}
