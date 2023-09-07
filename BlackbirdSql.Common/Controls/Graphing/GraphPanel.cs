// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.GraphPanel
using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Properties;
using Microsoft.AnalysisServices.Graphing;


namespace BlackbirdSql.Common.Controls.Graphing;

public class GraphPanel : ContainerControl
{
	private class EditQueryTextBtnContainer : ContainerControl
	{
		private class EditQueryButton : Button
		{
			private const float C_EditQueryTextBtnFontSize = 5f;

			private const string C_EditQueryTextBtnFont = "Segoe UI";

			private const int C_EditQueryTextBtnHeight = 20;

			private ToolTip toolTip;

			public EditQueryButton(EventHandler editQueryTextHandler, int width)
			{
				AutoSize = true;
				Name = "EditQueryTextButton";
				Font = new Font(C_EditQueryTextBtnFont, C_EditQueryTextBtnFontSize);
				Text = "...";
				Width = width;
				Height = C_EditQueryTextBtnHeight;
				Dock = DockStyle.None;
				if (editQueryTextHandler != null)
				{
					Click += editQueryTextHandler;
					toolTip = new ToolTip();
					toolTip.SetToolTip(this, ControlsResources.EditQueryText);
				}
				else
				{
					Enabled = false;
				}
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				if (Parent != null)
				{
					Top = (Parent.Height - Height) / 2;
					Left = Parent.Width - Width;
				}
				base.OnPaint(e);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && toolTip != null)
				{
					toolTip.Dispose();
					toolTip = null;
				}
				base.Dispose(disposing);
			}
		}

		private readonly EditQueryButton editQueryBtn;

		private const int C_EditQueryTextBtnWidth = 21;

		public EditQueryTextBtnContainer(EventHandler editQueryTextHandler)
		{
			Width = C_EditQueryTextBtnWidth;
			BackColor = SystemColors.Window;
			editQueryBtn = new EditQueryButton(editQueryTextHandler, C_EditQueryTextBtnWidth);
			Controls.Add(editQueryBtn);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.DrawLine(SystemPens.WindowText, 0, 0, Width - 1, 0);
			e.Graphics.DrawLine(SystemPens.WindowText, 0, Height - 1, Width - 1, Height - 1);
		}
	}

	protected DescriptionControl _DescriptionCtl;

	private GraphControl graphControl;

	private EditQueryTextBtnContainer btnContainer;

	private ContainerControl queryContainer;

	public string Title
	{
		get
		{
			return DescriptionCtl.Title;
		}
		set
		{
			DescriptionCtl.Title = value;
		}
	}

	public DescriptionControl DescriptionCtl => _DescriptionCtl;

	public GraphControl GraphControl => graphControl;

	public bool IsActive => GraphControl.IsActive;

	public event GraphEventHandler SelectionChanged
	{
		add
		{
			graphControl.SelectionChanged += value;
		}
		remove
		{
			graphControl.SelectionChanged -= value;
		}
	}

	public event EventHandler IsActiveChanged;

	public event EventHandler ShowContextMenu;

	public GraphPanel()
	{
		Initialize(null);
	}

	public GraphPanel(EventHandler editQueryTextHandler)
	{
		Initialize(editQueryTextHandler);
	}

	public void Activate()
	{
		GraphControl.Activate();
	}

	public void Deactivate()
	{
		GraphControl.Deactivate();
	}

	public void SetGraph(IGraph graph)
	{
		GraphControl.SetGraph(graph);
		DescriptionCtl.QueryText = GraphControl.Statement;
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		Size size = GraphControl.WorldToView(GraphControl.GraphBoundingRectangle.Size);
		return new Size(proposedSize.Width, size.Height + DescriptionCtl.Height + SystemInformation.HorizontalScrollBarHeight + 2);
	}

	private void Initialize(EventHandler editQueryTextHandler)
	{
		SetStyle(ControlStyles.Opaque, value: true);
		SuspendLayout();
		graphControl = CreateGraphControl();
		graphControl.Name = "ShowPlanGraph";
		graphControl.AccessibleName = ControlsResources.ExecutionPlanGraphAAName;
		graphControl.Dock = DockStyle.Fill;
		graphControl.MouseDown += OnChildControlMouseDown;
		graphControl.IsActiveChanged += OnGraphControlIsActiveChanged;
		Controls.Add(graphControl);
		queryContainer = new ContainerControl
		{
			Name = "GraphPanelQueryContainer",
			Dock = DockStyle.Top,
			AutoSize = true
		};
		_DescriptionCtl = MakeNewDescription();
		_DescriptionCtl.Name = "DescriptionCtl";
		_DescriptionCtl.AccessibleName = ControlsResources.ExecutionPlanDescriptionAAName;
		_DescriptionCtl.Dock = DockStyle.Top;
		_DescriptionCtl.Width = Width;
		_DescriptionCtl.MouseDown += OnChildControlMouseDown;
		queryContainer.Controls.Add(_DescriptionCtl);
		if (editQueryTextHandler != null)
		{
			btnContainer = new EditQueryTextBtnContainer(editQueryTextHandler)
			{
				Name = "GraphPanelBtnContainer",
				Dock = DockStyle.Right
			};
			queryContainer.Controls.Add(btnContainer);
		}
		Controls.Add(queryContainer);
		ResumeLayout();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			SuspendLayout();
			if (graphControl != null)
			{
				graphControl.MouseDown -= OnChildControlMouseDown;
				graphControl.IsActiveChanged -= OnGraphControlIsActiveChanged;
				graphControl = null;
			}
			if (_DescriptionCtl != null)
			{
				_DescriptionCtl.MouseDown -= OnChildControlMouseDown;
				_DescriptionCtl.Dispose();
				_DescriptionCtl = null;
			}
			Controls.Clear();
		}
		base.Dispose(disposing);
	}

	protected virtual DescriptionControl MakeNewDescription()
	{
		return new DescriptionControl();
	}

	private void OnChildControlMouseDown(object sender, MouseEventArgs e)
	{
		if (!IsActive)
		{
			Activate();
		}
		if (e.Button == MouseButtons.Right && e.Clicks == 1 && ShowContextMenu != null)
		{
			GraphControl.Refresh();
			ShowContextMenu(this, EventArgs.Empty);
		}
	}

	private void OnGraphControlIsActiveChanged(object sender, EventArgs e)
	{
		IsActiveChanged?.Invoke(this, e);
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		_DescriptionCtl.Height = 0;
		base.OnLayout(levent);
	}

	protected virtual GraphControl CreateGraphControl()
	{
		return new GraphControl();
	}
}
