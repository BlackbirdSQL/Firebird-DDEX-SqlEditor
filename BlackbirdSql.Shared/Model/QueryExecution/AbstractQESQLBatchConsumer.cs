// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchConsumerBase

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Sys;



namespace BlackbirdSql.Shared.Model.QueryExecution;


public abstract class AbstractQESQLBatchConsumer : IBQESQLBatchConsumer, IDisposable
{

	protected AbstractQESQLBatchConsumer(IBQueryExecutionHandler resultsControl)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.QESQLBatchConsumerBase", "", null);
		_ResultsControl = resultsControl;
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
	}

	protected virtual void Dispose(bool bDisposing)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.Dispose", "bDisposing = {0}", bDisposing);
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





	protected int _MaxCharsPerColumn = SysConstants.C_DefaultTextMaxCharsPerColumnStd;

	protected bool _DiscardResults = true;

	protected int _CurrentErrorCount;

	protected int _CurrentMessageCount;

	protected const int C_NumberOfFirstMessagesToFlush = 500;

	protected const int C_NewMessagesFlushFreqLess1000 = 50;

	protected const int C_NewMessagesFlushFreqMore1000 = 100;

	protected IBQueryExecutionHandler _ResultsControl;

	private MoreRowsAvailableEventHandler _MoreRowsFromDSForDiscardHandler;

	private QESQLBatchNewResultSetEventArgs _ResultSetArgsForDiscard;

	public int CurrentErrorCount => _CurrentErrorCount;

	public int CurrentMessageCount 
	{
		get { return _CurrentMessageCount; }
		set { _CurrentMessageCount = value; }
	}

	public int TotalInfoMessageCount => _CurrentMessageCount + _CurrentErrorCount;

	public int MaxCharsPerColumn
	{
		get { return _MaxCharsPerColumn; }
		set { _MaxCharsPerColumn = value; }
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

	protected virtual async Task<bool> HandleNewResultSetForDiscardAsync(QESQLBatchNewResultSetEventArgs args)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.HandleNewResultSetForDiscard", "", null);
		_MoreRowsFromDSForDiscardHandler = MoreRowsFromDSForDiscard;
		_ResultSetArgsForDiscard = args;
		_ResultSetArgsForDiscard.ResultSet.MoreRowsAvailableEvent += _MoreRowsFromDSForDiscardHandler;

		await _ResultSetArgsForDiscard.ResultSet.StartConsumingDataWithoutStoringAsync(args.CancelToken);

		return !args.CancelToken.IsCancellationRequested;
	}

	private void MoreRowsFromDSForDiscard(object sender, MoreRowsAvailableEventArgs a)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.MoreRowsFromDSForDiscard", "", null);
		if (a.AllRows)
		{
			_ResultSetArgsForDiscard.ResultSet.MoreRowsAvailableEvent -= _MoreRowsFromDSForDiscardHandler;
			_MoreRowsFromDSForDiscardHandler = null;
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
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnCancelling", "", null);
	}

	public virtual void OnMessage(object sender, QESQLBatchMessageEventArgs args)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnMessage", "", null);
		if (!DiscardResults)
		{
			// string msg = (TotalInfoMessageCount > 0 ? "\r\n" : "") + args.Message;
			string msg = args.Message;

			_CurrentMessageCount++;

			if (string.IsNullOrWhiteSpace(args.DetailedMessage))
			{
				_ResultsControl.AddStringToInfoMessages(msg, ShouldFlushMessages(_CurrentMessageCount));
				return;
			}

			_ResultsControl.AddStringToInfoMessages(msg + "\r\n\t" + args.DetailedMessage, ShouldFlushMessages(_CurrentMessageCount));
		}
	}

	public virtual void OnErrorMessage(object sender, QESQLBatchErrorMessageEventArgs args)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnErrorMessage", "", null);
		if (!DiscardResults)
		{
			string msg = TotalInfoMessageCount > 0 ? "\r\n" : "";

			msg += string.IsNullOrWhiteSpace(args.DescriptionMessage) ? args.DetailedMessage : args.DetailedMessage + "\r\n\t" + args.DescriptionMessage;

			_CurrentErrorCount++;

			_ResultsControl.AddStringToErrors(msg, args.Line, args.TextSpan, ShouldFlushMessages(_CurrentErrorCount));
		}
	}

	public virtual void OnSpecialAction(object sender, QESQLBatchSpecialActionEventArgs args)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnSpecialAction", "", null);
		_ResultsControl.ProcessSpecialActionOnBatch(args);
	}

	public virtual void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnStatementCompleted", "sender: {0}, args.RecordCount: {1}", sender, args.RecordCount);
	}

	public virtual void Cleanup()
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.Cleanup", "", null);
		if (_ResultSetArgsForDiscard != null)
		{
			if (_MoreRowsFromDSForDiscardHandler != null)
			{
				_ResultSetArgsForDiscard.ResultSet.MoreRowsAvailableEvent -= _MoreRowsFromDSForDiscardHandler;
				_MoreRowsFromDSForDiscardHandler = null;
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
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnFinishedProcessingResultSet", "", null);
	}

	public abstract Task<bool> OnNewResultSetAsync(object sender, QESQLBatchNewResultSetEventArgs args);
}
