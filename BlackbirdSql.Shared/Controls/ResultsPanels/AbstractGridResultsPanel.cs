// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.GridResultsPanelBase

using System;
using System.Drawing.Printing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Controls.Widgets;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys;



namespace BlackbirdSql.Shared.Controls.ResultsPanels;


public abstract class AbstractGridResultsPanel : AbstractResultsPanel
{

	protected AbstractGridResultsPanel(string defaultResultsDirectory) : base(defaultResultsDirectory)
	{
	}



	protected Control _lastFocusedControl;

	protected MultiControlPanel _FirstGridPanel = new MultiControlPanel();

	protected PageSettings _cachedPageSettings;

	protected PrinterSettings _cachedPrinterSettings;

	public Control BottomControl => _FirstGridPanel.GetHostedControl(_FirstGridPanel.HostedControlsCount - 1);

	public Control TopControl
	{
		get
		{
			if (_FirstGridPanel.HostedControlsCount > 0)
			{
				return _FirstGridPanel.GetHostedControl(0);
			}

			return null;
		}
	}

	public Control LastFocusedControl => _lastFocusedControl;

	private PrintDocument CurrentGridPrintDocument
	{
		get
		{
			GridControl focusedGrid = FocusedGrid;
			if (focusedGrid == null)
			{
				return null;
			}

			PrintDocument printDocument = focusedGrid.PrintDocument;
			if (_cachedPageSettings != null)
			{
				printDocument.DefaultPageSettings = _cachedPageSettings;
			}

			if (_cachedPrinterSettings != null)
			{
				printDocument.PrinterSettings = _cachedPrinterSettings;
			}

			return printDocument;
		}
	}

	public GridControl FocusedGrid
	{
		get
		{
			if (_FirstGridPanel == null)
			{
				InvalidOperationException ex = new(ControlsResources.MultiPanelGridContainerIsNotAvailable);
				Diag.Dug(ex);
				throw ex;
			}

			for (int i = 0; i < _FirstGridPanel.HostedControlsCount; i++)
			{
				Control hostedControl = _FirstGridPanel.GetHostedControl(i);
				if (hostedControl.Focused)
				{
					return (GridControl)(object)(hostedControl is GridControl ? hostedControl : null);
				}
			}

			return null;
		}
	}


	public override void ActivateControl()
	{
		Control control = LastFocusedControl;
		control ??= TopControl;

		control?.Focus();
	}

	protected void OnCopy(object sender, EventArgs a)
	{
		// Tracer.Trace(GetType(), "GridResultsTabPanel.OnCopy", "", null);
		GridControl focusedGrid = FocusedGrid;
		if (focusedGrid != null)
		{
			try
			{
				Clipboard.SetDataObject(focusedGrid.GetDataObject(false));
			}
			catch (Exception e)
			{
				Diag.Dug(e);
				MessageCtl.ShowEx(string.Empty, e);
			}
		}
	}

	protected void OnCopyWithHeaders(object sender, EventArgs a)
	{
		// Tracer.Trace(GetType(), "GridResultsTabPanel.OnCopyWithHeaders", "", null);
		GridControl focusedGrid = FocusedGrid;
		if (focusedGrid == null)
		{
			return;
		}

		bool includeHeadersOnDragAndDrop = false;
		if (focusedGrid is GridResultsGrid)
		{
			includeHeadersOnDragAndDrop = ((GridResultsGrid)(object)focusedGrid).IncludeHeadersOnDragAndDrop;
			((GridResultsGrid)(object)focusedGrid).IncludeHeadersOnDragAndDrop = true;
			((GridResultsGrid)(object)focusedGrid).ForceHeadersOnDragAndDrop = true;
		}

		try
		{
			Clipboard.SetDataObject(focusedGrid.GetDataObject(false));
		}
		catch (Exception e)
		{
			Diag.Dug(e);
			MessageCtl.ShowEx(string.Empty, e);
		}
		finally
		{
			if (focusedGrid is GridResultsGrid)
			{
				((GridResultsGrid)(object)focusedGrid).IncludeHeadersOnDragAndDrop = includeHeadersOnDragAndDrop;
				((GridResultsGrid)(object)focusedGrid).ForceHeadersOnDragAndDrop = false;
			}
		}
	}

	protected void OnPrint(object sender, EventArgs a)
	{
		// Tracer.Trace(GetType(), "GridResultsTabPanel.OnPrint", "", null);
		PrintDialog printDialog = new()
		{
			Document = CurrentGridPrintDocument
		};
		try
		{
			if (FormHost.ShowDialog(printDialog) == DialogResult.OK)
			{
				printDialog.Document.Print();
			}
		}
		catch (Exception e)
		{
			Diag.Dug(e);
			MessageCtl.ShowEx(ControlsResources.ErrUnableToPrintResults, e);
		}
	}

	protected void OnPrintPageSetup(object sender, EventArgs a)
	{
		// Tracer.Trace(GetType(), "GridResultsTabPanel.OnPrintPageSetup", "", null);
		try
		{
			PageSetupDialog pageSetupDialog = new PageSetupDialog();
			_cachedPageSettings ??= new PageSettings();

			_cachedPrinterSettings ??= new PrinterSettings();

			pageSetupDialog.PageSettings = _cachedPageSettings;
			pageSetupDialog.PrinterSettings = _cachedPrinterSettings;
			pageSetupDialog.AllowPrinter = true;
			FormHost.ShowDialog(pageSetupDialog);
		}
		catch (Exception e)
		{
			Diag.Dug(e);
			MessageCtl.ShowEx(ControlsResources.ErrUnableToPageSetup, e);
		}
	}

	public override void Clear()
	{
		_FirstGridPanel.Clear();
		_lastFocusedControl = null;
	}
}
