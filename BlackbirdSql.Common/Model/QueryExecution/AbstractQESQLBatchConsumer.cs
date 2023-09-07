// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchConsumerBase

using System;
using System.Globalization;
using System.IO;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;

namespace BlackbirdSql.Common.Model.QueryExecution;


public abstract class AbstractQESQLBatchConsumer : IQESQLBatchConsumer, IDisposable
{
	protected int _MaxCharsPerColumn = AbstractQESQLExec.DefaultMaxCharsPerColumnForText;

	protected bool _DiscardResults = true;

	protected int _CurrentErrorCount;

	protected int _CurrentMessageCount;

	protected const int C_NumberOfFirstMessagesToFlush = 500;

	protected const int C_NewMessagesFlushFreqLess1000 = 50;

	protected const int C_NewMessagesFlushFreqMore1000 = 100;

	protected ISqlQueryExecutionHandler _ResultsControl;

	private MoreRowsAvailableEventHandler _MoreRowsFromDSForDiscardDelegate;

	private QESQLBatchNewResultSetEventArgs _ResultSetArgsForDiscard;

	public int MaxCharsPerColumn
	{
		get
		{
			return _MaxCharsPerColumn;
		}
		set
		{
			_MaxCharsPerColumn = value;
		}
	}

	public bool DiscardResults
	{
		get
		{
			return _DiscardResults;
		}
		set
		{
			_DiscardResults = value;
		}
	}

	protected AbstractQESQLBatchConsumer(ISqlQueryExecutionHandler resultsControl)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.QESQLBatchConsumerBase", "", null);
		_ResultsControl = resultsControl;
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
	}

	protected virtual void Dispose(bool bDisposing)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.Dispose", "bDisposing = {0}", bDisposing);
		if (bDisposing)
		{
			Cleanup();
			DiscardResults = true;
			if (_ResultsControl != null)
			{
				_ResultsControl = null;
			}
		}
	}

	public static string GetTempXMLFileName()
	{
		string tempFileName = Path.GetTempFileName();
		try
		{
			File.Delete(tempFileName);
		}
		catch
		{
		}
		return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}.xml", Path.GetDirectoryName(tempFileName), Path.GetFileNameWithoutExtension(tempFileName));
	}

	protected virtual void HandleNewResultSetForDiscard(QESQLBatchNewResultSetEventArgs args)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.HandleNewResultSetForDiscard", "", null);
		_MoreRowsFromDSForDiscardDelegate = MoreRowsFromDSForDiscard;
		_ResultSetArgsForDiscard = args;
		_ResultSetArgsForDiscard.ResultSet.MoreRowsAvailable += _MoreRowsFromDSForDiscardDelegate;
		_ResultSetArgsForDiscard.ResultSet.StartConsumingDataWithoutStoring();
	}

	private void MoreRowsFromDSForDiscard(object sender, MoreRowsAvailableEventArgs a)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.MoreRowsFromDSForDiscard", "", null);
		if (a.AllRows)
		{
			_ResultSetArgsForDiscard.ResultSet.MoreRowsAvailable -= _MoreRowsFromDSForDiscardDelegate;
			_MoreRowsFromDSForDiscardDelegate = null;
			_ResultSetArgsForDiscard = null;
		}
	}

	private bool ShouldFlushMessages(int currentMessagesNumber)
	{
		if (currentMessagesNumber > C_NumberOfFirstMessagesToFlush && (currentMessagesNumber <= C_NumberOfFirstMessagesToFlush || currentMessagesNumber > 1000 || currentMessagesNumber % C_NewMessagesFlushFreqLess1000 != 0))
		{
			if (_CurrentMessageCount >= 1000)
			{
				return currentMessagesNumber % C_NewMessagesFlushFreqMore1000 == 0;
			}
			return false;
		}
		return true;
	}

	public virtual void OnCancelling(object sender, EventArgs args)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnCancelling", "", null);
	}

	public virtual void OnMessage(object sender, QESQLBatchMessageEventArgs args)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnMessage", "", null);
		if (!DiscardResults)
		{
			_CurrentMessageCount++;
			if (args.DetailedMessage.Length == 0)
			{
				_ResultsControl.AddStringToMessages(args.Message, ShouldFlushMessages(_CurrentMessageCount));
				return;
			}
			_ResultsControl.AddStringToMessages(args.Message, flush: false);
			_ResultsControl.AddStringToMessages(args.DetailedMessage, ShouldFlushMessages(_CurrentMessageCount));
		}
	}

	public virtual void OnErrorMessage(object sender, QESQLBatchErrorMessageEventArgs args)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnErrorMessage", "", null);
		if (!DiscardResults)
		{
			_CurrentErrorCount++;
			_ResultsControl.AddStringToErrors(string.IsNullOrEmpty(args.DescriptionMessage) ? args.DetailedMessage : args.DetailedMessage + "\r\n" + args.DescriptionMessage, args.Line, args.TextSpan, ShouldFlushMessages(_CurrentErrorCount));
		}
	}

	public virtual void OnSpecialAction(object sender, QESQLBatchSpecialActionEventArgs args)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnSpecialAction", "", null);
		_ResultsControl.ProcessSpecialActionOnBatch(args);
	}

	public virtual void OnStatementCompleted(object sender, QESQLBatchStatementCompletedEventArgs args)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnStatementCompleted", "sender: {0}, args.RecordCount: {1}, args.IsDebugging: {2}", sender, args.RecordCount, args.IsDebugging);
	}

	public virtual void Cleanup()
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.Cleanup", "", null);
		if (_ResultSetArgsForDiscard != null)
		{
			if (_MoreRowsFromDSForDiscardDelegate != null)
			{
				_ResultSetArgsForDiscard.ResultSet.MoreRowsAvailable -= _MoreRowsFromDSForDiscardDelegate;
				_MoreRowsFromDSForDiscardDelegate = null;
			}
			_ResultSetArgsForDiscard = null;
		}
	}

	public virtual void CleanupAfterFinishingExecution()
	{
		_CurrentErrorCount = 0;
		_CurrentMessageCount = 0;
	}

	public virtual void OnFinishedProcessingResultSet(object sender, EventArgs args)
	{
		Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnFinishedProcessingResultSet", "", null);
	}

	public abstract void OnNewResultSet(object sender, QESQLBatchNewResultSetEventArgs args);
}
