using System;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Exceptions;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Model.QueryExecution;


public class ResultsToGridBatchConsumer : AbstractQESQLBatchConsumer
{
	private ResultSetAndGridContainer _GridContainer;

	private bool _CouldNotAddGrid;

	public ResultsToGridBatchConsumer(IBsQueryExecutionHandler resultsControl)
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

	public override async Task<bool> OnNewResultSetAsync(object sender, BatchNewResultSetEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnNewResultSetAsync()");

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
				await args.ResultSet.InitializeAsync(true, args.CancelToken);
				await RegisterNewResultSetForDiscardEventsAsync(args);
				return false;
			}

			await args.ResultSet.InitializeAsync(false, args.CancelToken);

			if (args.CancelToken.IsCancellationRequested)
				return false;

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
			await _GridContainer.StartRetrievingDataAsync(_MaxCharsPerColumn, _ResultsControl.LiveSettings.EditorResultsGridMaxCharsPerColumnXml, args.CancelToken);
		}
		catch (BatchConsumerException ex)
		{
			Diag.Dug(ex);
			if (ex.ExtraInfo == BatchConsumerException.EnErrorType.CannotShowMoreResults)
			{
				_CouldNotAddGrid = true;
				return false;
			}
			throw;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		// Tracer.Trace(GetType(), "OnNewResultSetAsync()", "Completed");

		return !args.CancelToken.IsCancellationRequested;
	}

	public override void OnStatementCompleted(object sender, BatchStatementCompletedEventArgs args)
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


