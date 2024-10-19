// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.GridResultsPanelBase

using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys;



namespace BlackbirdSql.Shared.Controls.Results;


public abstract class AbstractGridResultsPanel : AbstractResultsPanel
{

	protected AbstractGridResultsPanel(string defaultResultsDirectory) : base(defaultResultsDirectory)
	{
	}



	protected Control _lastFocusedControl;

	protected MultiControlPanel _MultiControlPnl = new ();

	protected PageSettings _cachedPageSettings;

	protected PrinterSettings _cachedPrinterSettings;

	public Control BottomControl => _MultiControlPnl.GetHostedControl(_MultiControlPnl.HostedControlsCount - 1);

	public Control TopControl
	{
		get
		{
			if (_MultiControlPnl.HostedControlsCount > 0)
			{
				return _MultiControlPnl.GetHostedControl(0);
			}

			return null;
		}
	}

	public Control LastFocusedControl => _lastFocusedControl;

	public MultiControlPanel MultiControlPnl => _MultiControlPnl;

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
			if (_MultiControlPnl == null)
			{
				InvalidOperationException ex = new(ControlsResources.ExMultiPanelGridContainerIsNotAvailable);
				Diag.Dug(ex);
				throw ex;
			}

			for (int i = 0; i < _MultiControlPnl.HostedControlsCount; i++)
			{
				Control hostedControl = _MultiControlPnl.GetHostedControl(i);
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



	protected bool GetCoordinatesForPopupMenuFromWM_Context(ref Message m, out int x, out int y, Control c)
	{
		x = (short)(int)m.LParam;
		y = (int)m.LParam >> 16;
		if (c == null)
		{
			return false;
		}

		Point pt = c.PointToClient(new Point(x, y));
		if ((int)m.LParam == -1)
		{
			if (c is GridControl val)
			{
				val.GetCurrentCell(out long num, out int num2);
				Rectangle visibleCellRectangle = val.GetVisibleCellRectangle(num, num2);
				pt = visibleCellRectangle.IsEmpty || !c.ClientRectangle.Contains(visibleCellRectangle.Left, visibleCellRectangle.Top) ? c.PointToScreen(new Point(0, val.HeaderHeight)) : c.PointToScreen(new Point(visibleCellRectangle.Left, visibleCellRectangle.Top));
				x = pt.X;
				y = pt.Y;
			}
			else
			{
				pt = c.PointToScreen(new Point(c.ClientSize.Width / 2, c.ClientSize.Height / 2));
				x = pt.X;
				y = pt.Y;
			}

			pt = c.PointToClient(new Point(x, y));
		}

		return c.ClientRectangle.Contains(pt);
	}



	protected void OnCopy(object sender, EventArgs a)
	{
		// Evs.Trace(GetType(), "GridResultsTabPanel.OnCopy", "", null);
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
				MessageCtl.ShowEx("", e);
			}
		}
	}

	protected void OnCopyWithHeaders(object sender, EventArgs a)
	{
		// Evs.Trace(GetType(), "GridResultsTabPanel.OnCopyWithHeaders", "", null);
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
			MessageCtl.ShowEx("", e);
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
		// Evs.Trace(GetType(), "GridResultsTabPanel.OnPrint", "", null);
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
			MessageCtl.ShowEx(ControlsResources.ExUnableToPrintResults, e);
		}
	}

	protected void OnPrintPageSetup(object sender, EventArgs a)
	{
		// Evs.Trace(GetType(), "GridResultsTabPanel.OnPrintPageSetup", "", null);
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
			MessageCtl.ShowEx(ControlsResources.ExUnableToPageSetup, e);
		}
	}

	public override void Clear()
	{
		_MultiControlPnl.Clear();
		_lastFocusedControl = null;
	}
}
