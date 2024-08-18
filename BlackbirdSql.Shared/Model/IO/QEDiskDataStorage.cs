// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEDiskDataStorage
using System;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Model.IO;

public class QEDiskDataStorage : AbstractDiskDataStorage, IBsQEStorage, IBsDataStorage, IDisposable
{
	private bool _IsClosed = true;


	public QEDiskDataStorage()
	{
		// Tracer.Trace(GetType(), "QEDiskDataStorage.QEDiskDataStorage", "", null);
		_DataStorageEnabled = true;
	}



	public override async Task<bool> SerializeDataAsync(CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "QEDiskDataStorage.SerializeData", "", null);
		try
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QEDiskDataStorage.SerializeData", "_DataStorageEnabled = {0}", _DataStorageEnabled);
			await base.SerializeDataAsync(cancelToken);
		}
		catch (Exception e)
		{
			Diag.Debug(e);
			long rowCount = RowCount;

			if (rowCount > 0)
				await OnStorageNotifyAsync(rowCount, true, cancelToken);

			throw;
		}
		finally
		{
			_IsClosed = true;
		}

		return !cancelToken.IsCancellationRequested;
	}

	public new bool IsClosed()
	{
		return _IsClosed;
	}

	public new async Task<bool> StartStoringDataAsync(CancellationToken cancelToken)
	{
		if (!_IsClosed)
		{
			throw new InvalidOperationException(Resources.ExStorageAlreadyStoring);
		}
		_IsClosed = false;

		await SerializeDataAsync(cancelToken);

		return !cancelToken.IsCancellationRequested;
	}

	public void InitiateStopStoringData()
	{
		_DataStorageEnabled = false;
	}

	public override IBsStorageView GetStorageView()
	{
		// Tracer.Trace(GetType(), "QEDiskDataStorage.GetStorageView", "", null);
		QEDiskStorageView qEDiskStorageView = new QEDiskStorageView(this);
		if (MaxCharsToStore > 0)
		{
			qEDiskStorageView.MaxNumBytesToDisplay = MaxCharsToStore / 2;
		}
		return qEDiskStorageView;
	}
}
