// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEDiskDataStorage
using System;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Model.IO;

internal class QEDiskDataStorage : AbstractDiskDataStorage, IBsQEStorage, IBsDataStorage, IDisposable
{
	private bool _IsClosed = true;


	public QEDiskDataStorage()
	{
		// Evs.Trace(GetType(), "QEDiskDataStorage.QEDiskDataStorage", "", null);
		_IsWriting = true;
	}


	public new bool IsClosed => _IsClosed;
	



	public override async Task<bool> SerializeDataAsync(CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), "QEDiskDataStorage.SerializeData", "", null);
		try
		{
			// Evs.Trace(GetType(), Tracer.EnLevel.Verbose, "QEDiskDataStorage.SerializeData", "_IsWriting = {0}", _IsWriting);
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

		return !cancelToken.Cancelled();
	}


	public new async Task<bool> StartStoringDataAsync(CancellationToken cancelToken)
	{
		if (!_IsClosed)
		{
			throw new InvalidOperationException(Resources.ExceptionStartAlreadyStoring.Fmt(nameof(QEDiskDataStorage)));
		}
		_IsClosed = false;

		await SerializeDataAsync(cancelToken);

		return !cancelToken.Cancelled();
	}

	public void InitiateStopStoringData()
	{
		_IsWriting = false;
	}

	public override IBsStorageView GetStorageView()
	{
		// Evs.Trace(GetType(), "QEDiskDataStorage.GetStorageView", "", null);
		QEDiskStorageView qEDiskStorageView = new QEDiskStorageView(this);
		if (MaxCharsToStore > 0)
		{
			qEDiskStorageView.MaxNumBytesToDisplay = MaxCharsToStore / 2;
		}
		return qEDiskStorageView;
	}
}
