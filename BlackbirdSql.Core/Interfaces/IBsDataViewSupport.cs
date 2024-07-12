
using System;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.Core.Interfaces;

public interface IBsDataViewSupport : IVsDataSupportImportResolver, IVsDataViewIconProvider
{
	delegate void CloseDelegate(object sender, EventArgs e);

	event CloseDelegate OnCloseEvent;

}
