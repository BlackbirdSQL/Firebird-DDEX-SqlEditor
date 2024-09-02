// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchConsumerBase

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Model.QueryExecution;


public abstract class AbstractBatchConsumer : IBsQESQLBatchConsumer, IDisposable
{

	protected AbstractBatchConsumer(IBsQueryExecutionHandler resultsControl)
	{
		// Tracer.Trace(GetType(), "AbstractBatchConsumer()");
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





	protected int _MaxCharsPerColumn = SharedConstants.C_DefaultTextMaxCharsPerColumnStd;

	protected bool _DiscardResults = true;

	protected int _CurrentErrorCount;

	protected int _CurrentMessageCount;

	protected const int C_NumberOfFirstMessagesToFlush = 500;

	protected const int C_NewMessagesFlushFreqLess1000 = 50;

	protected const int C_NewMessagesFlushFreqMore1000 = 100;

	protected IBsQueryExecutionHandler _ResultsControl;

	private MoreRowsAvailableEventHandler _OnMoreRowsFromDSForDiscardEventAsync;

	private BatchNewResultSetEventArgs _ResultSetForDiscardArgs;

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
			return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}.xml", Cmd.GetDirectoryName(tempFileName), Cmd.GetFileNameWithoutExtension(tempFileName));
		}
		catch
		{
		}

		return "";
	}


	private async Task<bool> OnMoreRowsFromDSForDiscardAsync(object sender, MoreRowsAvailableEventArgs a)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.MoreRowsFromDSForDiscard", "", null);

		if (a.AllRows)
		{
			_ResultSetForDiscardArgs.ResultSet.MoreRowsAvailableEventAsync -= _OnMoreRowsFromDSForDiscardEventAsync;
			_OnMoreRowsFromDSForDiscardEventAsync = null;
			_ResultSetForDiscardArgs = null;

			return await Task.FromResult(true);
		}

		return await Task.FromResult(false);
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

	public virtual void OnMessage(object sender, BatchMessageEventArgs args)
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

	public virtual void OnErrorMessage(object sender, BatchErrorMessageEventArgs args)
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

	public virtual void OnSpecialAction(object sender, BatchSpecialActionEventArgs args)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnSpecialAction", "", null);
		_ResultsControl.ProcessBatchSpecialAction(args);
	}

	public virtual void OnStatementCompleted(object sender, BatchStatementCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.OnStatementCompleted", "sender: {0}, args.RecordCount: {1}", sender, args.RecordCount);
	}

	public virtual void Cleanup()
	{
		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.Cleanup", "", null);
		if (_ResultSetForDiscardArgs != null)
		{
			if (_OnMoreRowsFromDSForDiscardEventAsync != null)
			{
				_ResultSetForDiscardArgs.ResultSet.MoreRowsAvailableEventAsync -= _OnMoreRowsFromDSForDiscardEventAsync;
				_OnMoreRowsFromDSForDiscardEventAsync = null;
			}
			_ResultSetForDiscardArgs = null;
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

	public abstract Task<bool> OnNewResultSetAsync(object sender, BatchNewResultSetEventArgs args);

	protected virtual async Task<bool> RegisterNewResultSetForDiscardEventsAsync(BatchNewResultSetEventArgs args)
	{
		if (args.CancelToken.Cancelled())
			return false;

		// Tracer.Trace(GetType(), "QESQLBatchConsumerBase.HandleNewResultSetForDiscard", "", null);
		_OnMoreRowsFromDSForDiscardEventAsync = OnMoreRowsFromDSForDiscardAsync;
		_ResultSetForDiscardArgs = args;
		_ResultSetForDiscardArgs.ResultSet.MoreRowsAvailableEventAsync += _OnMoreRowsFromDSForDiscardEventAsync;

		await _ResultSetForDiscardArgs.ResultSet.StartConsumingDataWithoutStoringAsync(args.CancelToken);

		return !args.CancelToken.Cancelled();
	}


}
