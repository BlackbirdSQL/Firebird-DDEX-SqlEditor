#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Shared.Interfaces;

// using Microsoft.SqlServer.Management.QueryExecution;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns2 = Microsoft.SqlServer.Management.QueryExecution;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Shared.Events
{
	public class QESQLBatchErrorMessageEventArgs : EventArgs
	{
		private readonly string _DetailedMessage = "";

		private readonly string _DescriptionMessage = "";

		private readonly int _Line = -1;

		private readonly IBTextSpan _TextSpan;

		public string DetailedMessage => _DetailedMessage;

		public string DescriptionMessage => _DescriptionMessage;

		public int Line => _Line;

		public IBTextSpan TextSpan => _TextSpan;

		protected QESQLBatchErrorMessageEventArgs()
		{
		}

		public QESQLBatchErrorMessageEventArgs(string detailedMessage, string description)
		{
			_DetailedMessage = detailedMessage;
			_DescriptionMessage = description;
		}

		public QESQLBatchErrorMessageEventArgs(string detailedMessage, string description, int line, IBTextSpan textSpan)
		{
			_DetailedMessage = detailedMessage;
			_DescriptionMessage = description;
			_Line = line;
			_TextSpan = textSpan;
		}
	}
}
