#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Interfaces;




namespace BlackbirdSql.Common.Model.Events;


public class QEOLESQLOutputRedirectionEventArgs : EventArgs
{
	private readonly string _fullFileName;

	private readonly EnQEOLESQLOutputCategory _redirCategory;

	private IBQESQLBatchConsumer _curConsumer;

	public EnQEOLESQLOutputCategory OutputRedirectionCategory => _redirCategory;

	public string FullFileName => _fullFileName;

	public IBQESQLBatchConsumer BatchConsumer
	{
		get
		{
			return _curConsumer;
		}
		set
		{
			_curConsumer = value;
		}
	}

	protected QEOLESQLOutputRedirectionEventArgs()
	{
	}

	public QEOLESQLOutputRedirectionEventArgs(EnQEOLESQLOutputCategory category, string fullFileName, IBQESQLBatchConsumer curConsumer)
	{
		_fullFileName = fullFileName;
		_redirCategory = category;
		_curConsumer = curConsumer;
	}
}
