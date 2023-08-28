using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Core;
using System;

namespace BlackbirdSql.Common.Model.QueryExecution;


public class ResultsToGridBatchConsumer : AbstractQESQLBatchConsumer
{
	private ResultSetAndGridContainer _GridContainer;

	private bool _CouldNotAddGrid;

	public ResultsToGridBatchConsumer(ISqlQueryExecutionHandler resultsControl)
		: base(resultsControl)
	{
		Tracer.Trace(GetType(), "ResultsToGridBatchConsumer.ResultsToGridBatchConsumer", "", null);
		_MaxCharsPerColumn = AbstractQESQLExec.DefaultMaxCharsPerColumnForGrid;
	}

	public override void CleanupAfterFinishingExecution()
	{
		_CouldNotAddGrid = false;
		base.CleanupAfterFinishingExecution();
	}

	public override void OnNewResultSet(object sender, QESQLBatchNewResultSetEventArgs args)
	{
		Tracer.Trace(GetType(), "ResultsToGridBatchConsumer.OnNewResultSet", "", null);

		try
		{
			Cleanup();
			if (!_ResultsControl.CanAddMoreGrids)
			{
				_CouldNotAddGrid = true;
				_ResultsControl.MarkAsCouldNotAddMoreGrids();
			}
			if (DiscardResults || _CouldNotAddGrid)
			{
				Diag.Trace("ERROR or DISCARD: Could not add more grids");
				args.ResultSet.Initialize(forwardOnly: true);
				HandleNewResultSetForDiscard(args);
				return;
			}
			args.ResultSet.Initialize(forwardOnly: false);
			_GridContainer = new ResultSetAndGridContainer(args.ResultSet, printColumnHeaders: true, _MaxCharsPerColumn);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		try
		{
			_ResultsControl.AddGridContainer(_GridContainer);
			_GridContainer.StartRetrievingData(_MaxCharsPerColumn, _ResultsControl.ResultsSettings.MaxCharsPerColumnForXml);
		}
		catch (QESQLBatchConsumerException ex)
		{
			Tracer.LogExCatch(GetType(), ex);
			if (ex.ExtraInfo == QESQLBatchConsumerException.ErrorType.CannotShowMoreResults)
			{
				_CouldNotAddGrid = true;
				return;
			}
			throw;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}

	public override void OnStatementCompleted(object sender, QESQLBatchStatementCompletedEventArgs args)
	{
		base.OnStatementCompleted(sender, args);
		if (args.IsDebugging && args.RecordCount > 0 && _GridContainer != null)
		{
			_GridContainer.UpdateGrid();
		}
	}

	public override void Cleanup()
	{
		Tracer.Trace(GetType(), "ResultsToGridBatchConsumer.Cleanup", "", null);
		base.Cleanup();
		if (_GridContainer != null)
		{
			_GridContainer = null;
		}
	}
}
