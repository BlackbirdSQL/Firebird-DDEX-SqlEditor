#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Interfaces;

// using Microsoft.SqlServer.Management.QueryExecution;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns2 = Microsoft.SqlServer.Management.QueryExecution;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Events
{
	public class QESQLBatchErrorMessageEventArgs : EventArgs
	{
		private readonly string _detailedMessage = "";

		private readonly string _description = "";

		private readonly int _line = -1;

		private readonly ITextSpan _TextSpan;

		public string DetailedMessage => _detailedMessage;

		public string DescriptionMessage => _description;

		public int Line => _line;

		public ITextSpan TextSpan => _TextSpan;

		protected QESQLBatchErrorMessageEventArgs()
		{
		}

		public QESQLBatchErrorMessageEventArgs(string detailedMessage, string description)
		{
			_detailedMessage = detailedMessage;
			_description = description;
		}

		public QESQLBatchErrorMessageEventArgs(string detailedMessage, string description, int line, ITextSpan textSpan)
		{
			_detailedMessage = detailedMessage;
			_description = description;
			_line = line;
			_TextSpan = textSpan;
		}
	}
}
