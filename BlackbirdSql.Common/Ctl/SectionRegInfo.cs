// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Framework.SectionRegInfo

#define TRACE

using System;
using System.ComponentModel;
using System.Diagnostics;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Common.Ctl;

[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerDisplay("Id = {Id}, Priority = {Priority}")]
public class SectionRegInfo : IDisposable
{
	public Guid Id { get; private set; }

	public Lazy<IBSection, IBExportableMetadata> SectionTypeInfo { get; private set; }

	public int Priority { get; private set; }

	public object Tag { get; set; }

	public SectionRegInfo(Lazy<IBSection, IBExportableMetadata> sectionTypeInfo)
	{
		Id = new Guid(sectionTypeInfo.Metadata.Id);
		SectionTypeInfo = sectionTypeInfo;
		Priority = sectionTypeInfo.Metadata.Priority;
	}

	public void Dispose()
	{
		try
		{
			if (SectionTypeInfo != null && SectionTypeInfo.Value != null)
			{
				SectionTypeInfo.Value.Dispose();
			}
		}
		catch (Exception ex)
		{
			Trace.TraceError("SectionInfo.Dispose: {0}", ex.Message);
		}
		GC.SuppressFinalize(this);
	}
}
