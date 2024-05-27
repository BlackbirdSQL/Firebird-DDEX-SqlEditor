using System;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;



namespace BlackbirdSql.Common.Model.QueryExecution;


public class ResultsToGridBatchConsumer : AbstractQESQLBatchConsumer
{
	private ResultSetAndGridContainer _GridContainer;

	private bool _CouldNotAddGrid;

	public ResultsToGridBatchConsumer(IBSqlQueryExecutionHandler resultsControl)
		: base(resultsControl)
	{
		// Tracer.Trace(GetType(), "ResultsToGridBatchConsumer.ResultsToGridBatchConsumer", "", null);
		_MaxCharsPerColumn = SysConstants.C_DefaultGridMaxCharsPerColumnStd;
	}

	public override void CleanupAfterFinishingExecution()
	{
		_CouldNotAddGrid = false;
		base.CleanupAfterFinishingExecution();
	}

	public override void OnNewResultSet(object sender, QESQLBatchNewResultSetEventArgs args)
	{
		// Tracer.Trace(GetType(), "ResultsToGridBatchConsumer.OnNewResultSet", "", null);

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
				Diag.StackException("ERROR or DISCARD: Could not add more grids");
				args.ResultSet.Initialize(true);
				HandleNewResultSetForDiscard(args);
				return;
			}
			args.ResultSet.Initialize(false);
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
			_GridContainer.StartRetrievingData(_MaxCharsPerColumn, _ResultsControl.LiveSettings.EditorResultsGridMaxCharsPerColumnXml);
		}
		catch (QESQLBatchConsumerException ex)
		{
			Diag.Dug(ex);
			if (ex.ExtraInfo == QESQLBatchConsumerException.EnErrorType.CannotShowMoreResults)
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

	public override void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args)
	{
		base.OnStatementCompleted(sender, args);
	}

	public override void Cleanup()
	{
		// Tracer.Trace(GetType(), "ResultsToGridBatchConsumer.Cleanup", "", null);
		base.Cleanup();

		if (_GridContainer != null)
			_GridContainer = null;
	}
}
