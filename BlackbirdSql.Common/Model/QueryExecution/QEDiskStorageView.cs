// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEDiskStorageView
using System;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Model.QueryExecution;

internal class QEDiskStorageView : DiskStorageView, IQEStorageView, IStorageView, IDisposable
{
	public int MaxNumBytesToDisplay
	{
		get
		{
			return _MaxBytesToDisplay;
		}
		set
		{
			if (_MaxBytesToDisplay != value)
			{
				Tracer.Trace(GetType(), "QEDiskStorageView.MaxNumBytesToDisplay", "value = {0}", value);
				_MaxBytesToDisplay = value;
				_SbWork.Capacity = _MaxBytesToDisplay * 2 + 2;
			}
		}
	}

	public QEDiskStorageView(IDiskDataStorage storage)
		: base(storage)
	{
		Tracer.Trace(GetType(), "QEDiskStorageView.QEDiskStorageView", "", null);
	}
}
