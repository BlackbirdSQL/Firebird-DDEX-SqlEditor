using System;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Exceptions;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Model.QueryExecution;


public class GridBatchConsumer : AbstractBatchConsumer
{
	private ResultSetAndGridContainer _GridContainer;

	private bool _CouldNotAddGrid;

	public GridBatchConsumer(IBsQueryExecutionHandler resultsControl)
		: base(resultsControl)
	{
		// Evs.Trace(GetType(), ".ctor", "", null);
		_MaxCharsPerColumn = SharedConstants.C_DefaultGridMaxCharsPerColumnStd;
	}

	public override void CleanupAfterFinishingExecution()
	{
		_CouldNotAddGrid = false;
		base.CleanupAfterFinishingExecution();
	}

	public override async Task<bool> OnNewResultSetAsync(object sender, BatchNewResultSetEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnNewResultSetAsync));

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

			if (args.CancelToken.Cancelled())
				return false;

			_GridContainer = new ResultSetAndGridContainer(args.ResultSet, printColumnHeaders: true, _MaxCharsPerColumn);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw ex;
		}


		try
		{
			_ResultsControl.AddGridContainer(_GridContainer);
			await _GridContainer.StartRetrievingDataAsync(_MaxCharsPerColumn, _ResultsControl.LiveSettings.EditorResultsGridMaxCharsPerColumnXml, args.CancelToken);
		}
		catch (ResultsException ex)
		{
			Diag.Ex(ex);
			_CouldNotAddGrid = true;
		}
		catch (Exception ex)
		{
			Diag.Debug(ex);
			throw ex;
		}

		// Evs.Trace(GetType(), nameof(OnNewResultSetAsync), "Completed");

		return !args.CancelToken.Cancelled();
	}

	public override void OnStatementCompleted(object sender, BatchStatementCompletedEventArgs args)
	{
		base.OnStatementCompleted(sender, args);
	}

	public override void Cleanup()
	{
		// Evs.Trace(GetType(), nameof(Cleanup));
		base.Cleanup();

		if (_GridContainer != null)
			_GridContainer = null;
	}
}


