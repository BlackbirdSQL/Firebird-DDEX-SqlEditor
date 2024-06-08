// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.Controls.MultiControlPanel

using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Events;

namespace BlackbirdSql.Shared.Controls.Widgets;


public sealed class MultiControlPanel : Panel
{
	private class SmartPanelWithLayout : Panel
	{
		public int TotalHeightOfHostedControls
		{
			get
			{
				int num = 0;
				for (int i = 0; i < Controls.Count; i++)
				{
					num += Controls[i].Height;
				}

				return num;
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			int x = 0;
			int num = 0;
			int width = ClientRectangle.Width;
			int num2;
			Control control;
			for (int i = 0; i < Controls.Count; i++)
			{
				control = Controls[i];
				num2 = control.Height;
				control.SetBounds(x, num, width, num2, BoundsSpecified.All);
				num += control.Bounds.Height;
			}

			if (Controls.Count <= 1 || num >= ClientSize.Height)
			{
				return;
			}

			for (int num3 = Controls.Count - 1; num3 >= 0; num3--)
			{
				control = Controls[num3];
				if (control is not QESplitter && control is not Splitter)
				{
					control.Height += ClientSize.Height - num;
					num = control.Bounds.Bottom;
					for (num3++; num3 < Controls.Count; num3++)
					{
						control = Controls[num3];
						num2 = control.Height;
						control.SetBounds(x, num, width, num2, BoundsSpecified.All);
						num += control.Bounds.Height;
					}

					break;
				}
			}
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (((specified & BoundsSpecified.Height) == BoundsSpecified.Height || (specified & BoundsSpecified.Y) == BoundsSpecified.Y) && Controls.Count == 1)
			{
				Controls[0].Height = height;
			}

			base.SetBoundsCore(x, y, width, height, specified);
		}
	}

	private SmartPanelWithLayout hostPanel;

	private int hostedControlsMinSize = 10;

	private int hostedControlsMinInitialSize = 10;

	private const int C_SplitterHeight = 3;

	public int HostedControlsMinInitialSize
	{
		get
		{
			return hostedControlsMinInitialSize;
		}
		set
		{
			// Tracer.Trace(GetType(), "MultiControlPanel.HostedControlsMinInitialSize", "value = {0}", value);
			hostedControlsMinInitialSize = value;
		}
	}

	public int HostedControlsMinSize
	{
		get
		{
			return hostedControlsMinSize;
		}
		set
		{
			// Tracer.Trace(GetType(), "MultiControlPanel.HostedControlsMinSize", "value = {0}", value);
			hostedControlsMinSize = value;
		}
	}

	public int HostedControlsCount
	{
		get
		{
			int count = hostPanel.Controls.Count;
			if (count <= 1)
			{
				return count;
			}

			return count / 2;
		}
	}

	public MultiControlPanel()
	{
		// Tracer.Trace(GetType(), "MultiControlPanel.MultiControlPanel", "", null);
		AutoScroll = true;
		Clear();
	}

	public void Clear()
	{
		if (hostPanel != null)
		{
			hostPanel.Parent = null;
			hostPanel.Dispose();
		}

		hostPanel = new SmartPanelWithLayout
		{
			Location = new Point(0, 0),
			Size = ClientSize,
			Dock = DockStyle.Top,
			BorderStyle = BorderStyle.None,
			Height = Height
		};
		Controls.Add(hostPanel);
	}

	public void AddControl(Control controlToHost)
	{
		AddControl(controlToHost, limitMaxControlHeightToClientArea: true);
	}

	public void AddControl(Control controlToHost, bool limitMaxControlHeightToClientArea)
	{
		// Tracer.Trace(GetType(), "MultiControlPanel.AddControl", "", null);
		if (controlToHost == null)
		{
			Exception ex = new ArgumentNullException("controlToHost");
			Diag.ThrowException(ex);
		}

		int count = hostPanel.Controls.Count;

		if (count > 0)
		{
			hostPanel.SuspendLayout();

			try
			{
				if (count <= 1)
				{
					hostPanel.Controls.Add(AllocateNewSplitter(hostPanel.Controls[0], hostedControlsMinSize));
				}

				hostPanel.Controls.Add(controlToHost);
				hostPanel.Controls.Add(AllocateNewSplitter(controlToHost, hostedControlsMinSize));
				ResizeControls(usePreferredSize: true, limitMaxControlHeightToClientArea);
			}
			finally
			{
				hostPanel.ResumeLayout();
			}
		}
		else
		{
			controlToHost.Height = hostPanel.ClientRectangle.Height;
			hostPanel.Controls.Add(controlToHost);
		}
	}

	public Control GetHostedControl(int controlIndex)
	{
		if (hostPanel.Controls.Count == 0)
		{
			Exception ex = new InvalidOperationException();
			Diag.ThrowException(ex);
		}

		if (controlIndex < 0 || controlIndex > HostedControlsCount - 1)
		{
			Exception ex2 = new ArgumentOutOfRangeException("controlIndex");
			Diag.ThrowException(ex2);
		}

		return hostPanel.Controls[controlIndex * 2];
	}

	public int GetHostedControlIndex(Control c)
	{
		for (int i = 0; i < hostPanel.Controls.Count; i++)
		{
			if (hostPanel.Controls[i] == c)
			{
				return i / 2;
			}
		}

		return -1;
	}

	public void ResizeControlsToPreferredHeight(bool limitMaxControlHeightToClientArea)
	{
		ResizeControls(usePreferredSize: true, limitMaxControlHeightToClientArea);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		int hostedControlsCount = HostedControlsCount;

		hostPanel.SuspendLayout();

		try
		{
			int height = ClientRectangle.Height;
			if (hostedControlsCount <= 1)
			{
				hostPanel.Height = height;
			}
			else if (hostPanel.Height < height)
			{
				hostPanel.Height = height;
			}
			else if (hostPanel.TotalHeightOfHostedControls <= height)
			{
				hostPanel.Height = height;
			}
		}
		finally
		{
			hostPanel.ResumeLayout();
		}

		base.OnSizeChanged(e);
	}

	private QESplitter AllocateNewSplitter(Control boundControl, int minSize)
	{
		// Tracer.Trace(GetType(), "MultiControlPanel.AllocateNewSplitter", "minSize = {0}", minSize);
		QESplitter qESplitter = new QESplitter(boundControl, minSize)
		{
			BackColor = SystemColors.Control
		};
		qESplitter.SplitterMovedEvent += OnSplitterMoved;
		return qESplitter;
	}

	public void ResizeControls(bool usePreferredSize, bool limitMaxControlHeightToClientArea)
	{
		// Tracer.Trace(GetType(), "MultiControlPanel.ResizeControls", "", null);
		int count = hostPanel.Controls.Count;
		switch (count)
		{
			case 0:
				return;
			case 1:
				hostPanel.Controls[0].Height = hostPanel.ClientRectangle.Height;
				return;
		}

		count /= 2;
		int num = 0;
		int num2 = 0;
		_ = hostedControlsMinInitialSize;
		int[] array = null;
		int num3 = 0;
		if (usePreferredSize)
		{
			array = new int[count];
			Size proposedSize = new Size(hostPanel.ClientRectangle.Width, hostedControlsMinInitialSize);
			foreach (Control control3 in hostPanel.Controls)
			{
				if (control3 is not QESplitter)
				{
					array[num3] = control3.GetPreferredSize(proposedSize).Height;
					if (limitMaxControlHeightToClientArea)
					{
						array[num3] = Math.Max(Math.Min(array[num3], Height - C_SplitterHeight), hostedControlsMinSize);
					}
					else
					{
						array[num3] = Math.Max(array[num3], hostedControlsMinSize);
					}

					num += array[num3] + C_SplitterHeight;
					num3++;
				}
			}
		}
		else
		{
			num = hostedControlsMinInitialSize * count + C_SplitterHeight * count;
		}

		if (hostPanel.Height < num)
		{
			hostPanel.Height = num;
		}
		else
		{
			num2 = (hostPanel.Height - num) / count;
		}

		/*
		if (usePreferredSize)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "MultiControlPanel.ResizeControls", "control number = {0}, control heights = {1}, adjustment = {2}", count, IntArrayToString(array), num2);
		}
		else
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "MultiControlPanel.ResizeControls", "control number = {0}, control height = {1}", count, hostedControlsMinInitialSize + num2);
		}
		*/

		num3 = 0;
		foreach (Control control4 in hostPanel.Controls)
		{
			if (control4 is not QESplitter)
			{
				if (usePreferredSize)
				{
					control4.Height = array[num3++] + num2;
				}
				else
				{
					control4.Height = hostedControlsMinInitialSize + num2;
				}
			}
		}
	}



	/*
	private string IntArrayToString(int[] values)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int value in values)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}

			stringBuilder.Append(value);
		}

		return stringBuilder.ToString();
	}
	*/



	private void OnSplitterMoved(object sender, QESplitterMovedEventArgs e)
	{
		int num = e.SplitPosition - e.BoundControl.Bounds.Top - e.BoundControl.Height;

		hostPanel.SuspendLayout();

		try
		{
			int height = hostPanel.Height;
			int height2 = ClientRectangle.Height;
			int totalHeightOfHostedControls = hostPanel.TotalHeightOfHostedControls;
			e.BoundControl.Height += num;
			if (num > 0)
			{
				if (totalHeightOfHostedControls >= height2)
				{
					hostPanel.Height += num;
				}
				else
				{
					hostPanel.Height = Math.Max(height2, totalHeightOfHostedControls + num);
				}
			}
			else
			{
				hostPanel.Height = Math.Max(height2, height + num);
			}
		}
		finally
		{
			hostPanel.ResumeLayout();
		}
	}
}
