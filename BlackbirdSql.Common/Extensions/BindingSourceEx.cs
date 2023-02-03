using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualStudio.RpcContracts.FileSystem;

namespace BlackbirdSql.Common.Extensions
{
	/// <summary>
	/// Extends BindingSource to include limited support for Position -1
	/// </summary>
	internal class BindingSourceEx : BindingSource
	{
		private bool _Invalidated = false;
		private bool _InvalidatedChanging = false;


		//
		// Summary:
		//     Gets the current item in the list.
		//
		// Returns:
		//     An System.Object that represents the current item in the underlying list represented
		//     by the System.Windows.Forms.BindingSource.List property, or null if the list
		//     has no items.
		[Browsable(false)]
		public new object Current
		{
			get
			{
				if (_Invalidated)
				{
					return null;
				}

				return base.Current;
			}
		}

		//
		// Summary:
		//     Gets or sets the index of the current item in the underlying list.
		//
		// Returns:
		//     A zero-based index that specifies the position of the current item in the underlying
		//     list.
		[DefaultValue(-1)]
		[Browsable(false)]
		public new int Position
		{
			get
			{
				if (_Invalidated)
					return -1;

				return base.Position;
			}
			set
			{
				if (value == -1)
				{
					if (!_Invalidated)
					{
						_Invalidated = true;
						_InvalidatedChanging = true;
						OnCurrentChanged(new EventArgs());
						_InvalidatedChanging = false;
					}
					return;
				}

				if (value == base.Position && _Invalidated)
				{
					_Invalidated = false;
					OnCurrentChanged(new EventArgs());
					return;
				}

				_Invalidated = false;
				base.Position = value;
			}
		}


		//
		// Summary:
		//     Gets or sets the expression used to filter which rows are viewed.
		//
		// Returns:
		//     A string that specifies how rows are to be filtered. The default is null.
		[DefaultValue(null)]
		public override string Filter
		{
			get
			{
				return base.Filter;
			}
			set
			{
				if (value == base.Filter)
					return;

				_Invalidated = true;
				_InvalidatedChanging = true;
				base.Filter = value;
				_InvalidatedChanging = false;
			}
		}

		//
		// Summary:
		//     Raises the System.Windows.Forms.BindingSource.CurrentChanged event.
		//
		// Parameters:
		//   e:
		//     An System.EventArgs that contains the event data.
		protected override void OnCurrentChanged(EventArgs e)
		{
			if (!_InvalidatedChanging)
				_Invalidated = false;

			base.OnCurrentChanged(e);
		}
	}
}
