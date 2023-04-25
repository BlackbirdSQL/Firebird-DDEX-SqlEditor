using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;



namespace BlackbirdSql.Common.Extensions;


// =========================================================================================================
//											ErmBindingSource Class
//
/// <summary>
/// Provides a simple ERM set for Master and Dependent BindingSources. 
/// </summary>
/// <remarks>
/// Important: At a minimum <see cref="PrimaryKey"/>, <see cref="ForeignKey"/>, <see cref="BindingSource.DataSource"/> and <see cref="DependentSource"/> must
/// be defined before the class object will be placed into a ready state.
/// </remarks>
// =========================================================================================================
internal class ErmBindingSource : BindingSource, System.Collections.IEnumerable
{
	#region Variables


	private string _PrimaryKey = "";
	private string _ForeignKey = "";
	private object _NullableConstraintValue = null;

	private bool _FilterChanging = false;

	private BindingSource _Dependent;
	private object _DependentSource = null;

	private static readonly object EVENT_DEPENDENCYPOSITIONCHANGED = new object();
	private static readonly object EVENT_DEPENDENCYCURRENTCHANGED = new object();
	private static readonly object EVENT_DEPENDENCYLISTCHANGED = new object();


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - ErmBindingSource
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the value of the <see cref="PrimaryKey"/> for the <see cref="Current"/> object
	/// </summary>
	// ---------------------------------------------------------------------------------
	public object CurrentValue
	{
		get
		{
			if (_PrimaryKey == "" || Row == null)
				return null;

			if (Row[_PrimaryKey] is string str && str == "")
				return null;

			return Row[_PrimaryKey];
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	///  Gets or sets the <see cref="BindingSource.DataSource"/> for the master <see cref="BindingSource"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public new object DataSource
	{
		get
		{
			return base.DataSource;
		}
		set
		{
			base.DataSource = value;

			if (IsReady)
				Initialize();
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds or removes a <see cref="Dependent"/> <see cref="BindingSource.CurrentChanged"/> event handler.
	/// </summary>
	/// <remarks>
	/// Do not directly add to the CurrerntChanged delegate of <see cref="Dependent"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public event EventHandler DependencyCurrentChanged
	{
		add
		{
			if (Dependent == null)
				return;

			base.Events.AddHandler(EVENT_DEPENDENCYCURRENTCHANGED, value);
		}
		remove
		{
			if (Dependent == null)
				return;

			base.Events.RemoveHandler(EVENT_DEPENDENCYCURRENTCHANGED, value);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Occurs when the underlying <see cref="Dependent"/> BindingSource's list changes due to a <see cref="OnCurrentChanged()"/>
	/// event of this parent.
	/// </summary>
	/// <remarks>
	/// Occurs when the <see cref="Dependent"/>'s list changes as a result of a row change in the master
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public event ListChangedEventHandler DependencyListChanged
	{
		add
		{
			base.Events.AddHandler(EVENT_DEPENDENCYLISTCHANGED, value);
		}
		remove
		{
			base.Events.RemoveHandler(EVENT_DEPENDENCYLISTCHANGED, value);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds or removes a <see cref="Dependent"/> <see cref="BindingSource.PositionChanged"/> event handler.
	/// </summary>
	/// <remarks>
	/// Do not directly add to the PositionChanged delegate of <see cref="Dependent"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public event EventHandler DependencyPositionChanged
	{
		add
		{
			if (Dependent == null)
				return;

			base.Events.AddHandler(EVENT_DEPENDENCYPOSITIONCHANGED, value);
		}
		remove
		{
			if (Dependent == null)
				return;

			base.Events.RemoveHandler(EVENT_DEPENDENCYPOSITIONCHANGED, value);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the internal dependent <see cref="BindingSource"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public BindingSource Dependent
	{
		get
		{
			if (_Dependent != null)
				return _Dependent;

			if (_DependentSource == null || _PrimaryKey == "" || _ForeignKey == "")
				return null;

			_Dependent = new()
			{
				DataSource = _DependentSource,
			};

			Dependent.PositionChanged += OnDependencyPositionChanged;
			Dependent.CurrentChanged += OnDependencyCurrentChanged;


			return _Dependent;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the value of the <see cref="ForeignKey"/> for the <see cref="Dependent.Current"/> object
	/// </summary>
	// ---------------------------------------------------------------------------------
	public object DependentCurrentValue
	{
		get
		{
			if (_ForeignKey == "" || DependentRow == null)
				return null;

			if (DependentRow[_ForeignKey] is string str && str == "")
				return null;

			return DependentRow[_ForeignKey];
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or sets the cursor <see cref="BindingSource.Position"/> of the internal <see cref="Dependent"/> <see cref="BindingSource"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public int DependentPosition
	{
		get
		{
			if (Dependent == null)
				return -1;

			return Dependent.Position;
		}
		set
		{
			if (Dependent == null)
				return;
			Dependent.Position = value;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the <see cref="Dependent.Current"/> object of <see cref="Dependent"/> as a DataRow else returns null
	/// </summary>
	// ---------------------------------------------------------------------------------
	public DataRow DependentRow
	{
		get
		{
			if (Dependent == null || Dependent.Current == null)
				return null;

			try
			{
				if (Dependent.Current is DataRowView view)
					return view.Row;
				else
					return null;
			}
			catch { }

			return null;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	///  Gets or sets the <see cref="BindingSource.DataSource"/> for the internal dependent <see cref="BindingSource"/> <see cref="Dependent"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public object DependentSource
	{
		get
		{
			return _DependentSource;
		}
		set
		{
			_DependentSource = value;

			if (IsReady)
				Initialize();
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The column in the <see cref="DependentSource"/> DataSource that will serve as the selector for
	/// the <see cref="Dependent"/> BindingSource
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string ForeignKey
	{
		get
		{
			return _ForeignKey;
		}
		set
		{
			_ForeignKey = value.Trim();

			if (IsReady)
				Initialize();
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if <see cref="DataSource"/>, <see cref="DependentSource"/>, <see cref="PrimaryKey"/> and <see cref="ForeignKey"/>
	/// have been set.
	/// </summary>
	/// <remarks>
	/// Once <see cref="ErmBindingSource"/> is in a ready state
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public bool IsReady
	{
		get
		{
			return (Dependent != null && DataSource != null);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The null value of the <see cref="ForeignKey"/> if <see cref="BindingSource.Current"/> is invalidated.
	/// The default 'null' assumes the ForeignKey is nullable else you can use an empty
	/// string or any other value [and type] that will return an empty set for <see cref="DependentSource"/>
	/// </summary>
	// ---------------------------------------------------------------------------------
	public object NullableConstraintValue
	{
		get { return _NullableConstraintValue; }
		set { _NullableConstraintValue = value; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The column in the master <see cref="BindingSource.DataSource"/> that will serve as the primary
	/// key in the ERM
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string PrimaryKey
	{
		get
		{
			return _PrimaryKey;
		}
		set
		{
			_PrimaryKey = value.Trim();

			if (IsReady)
				Initialize();
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the <see cref="BindingSource.Current"/> object as a DataRow else returns null
	/// </summary>
	///
	// ---------------------------------------------------------------------------------
	public DataRow Row
	{
		get
		{
			if (Current == null)
				return null;

			try
			{
				if (Current is DataRowView view)
					return view.Row;
				else
					return null;
			}
			catch { }

			return null;
		}
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - ErmBindingSource
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the master <see cref="BindingSource"/>'s cursor <see cref="BindingSource.Position"/> to zero and updates
	/// the <see cref="Dependent"/>'s filter once <see cref="IsReady"/> is true.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void Initialize()
	{
		if (Position > 0)
			Position = -1;
		else
			UpdateDependencyFilter();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a <see cref="BindingSource.Find"/> on the master BindingSource's <see cref="PrimaryKey"/> column.
	/// </summary>
	/// <param name="key">The <see cref="PrimaryKey"/> value to find</param>
	/// <returns>The zero-based Position of the row else -1</returns>
	// ---------------------------------------------------------------------------------
	public int Find(object key)
	{
		if (_PrimaryKey == "")
			return -1;

		return Find(_PrimaryKey, key);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a <see cref="BindingSource.Find"/> on the <see cref="Dependent"/>'s <see cref="ForeignKey"/> column.
	/// </summary>
	/// <param name="key">The <see cref="ForeignKey"/> value to find</param>
	/// <returns>The zero-based Position of the row else -1</returns>
	// ---------------------------------------------------------------------------------
	public int FindDependent(object key)
	{
		if (Dependent == null || _ForeignKey == "")
			return -1;

		return Dependent.Find(_ForeignKey, key);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a <see cref="BindingSource.Find"/> on the <see cref="Dependent"/> BindingSource.
	/// </summary>
	/// <param name="propertyName">The column/property to search in</param>
	/// <param name="key">The value to find</param>
	/// <returns>The zero-based Position of the row else -1</returns>
	// ---------------------------------------------------------------------------------
	public int FindDependent(string propertyName, object key)
	{
		if (Dependent == null)
			return -1;

		return Dependent.Find(propertyName, key);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the <see cref="Dependent"/>'s filter based on the value in <see cref="PrimaryKey"/> filtered on
	/// <see cref="ForeignKey"/>.
	/// </summary>
	/// <param name="initializing"></param>
	// ---------------------------------------------------------------------------------
	protected void UpdateDependencyFilter(bool initializing = false)
	{
		if (Dependent == null)
			return;

		try
		{
			string filter;

			if (Row != null)
			{
				filter = _ForeignKey + " = '" + (string)Row[_PrimaryKey] + "'";
			}
			else
			{
				if (_NullableConstraintValue == null)
					filter = _ForeignKey + " is NULL";
				else if (_NullableConstraintValue is string)
					filter = _ForeignKey + " = '" + _NullableConstraintValue.ToString() + "'";
				else
					filter = _ForeignKey + " = " + _NullableConstraintValue.ToString();
			}

			if (Dependent.Filter == filter)
				return;

			int position = DependentPosition;

			_FilterChanging = true;
			Dependent.Filter = filter;
			_FilterChanging = false;
			Dependent.Position = -1;

			if (!initializing)
			{
				ListChangedEventArgs e = new(ListChangedType.Reset, position, DependentPosition);
				OnDependencyListChanged(e);
			}
			// Diag.Trace("Dependency filter set: Position: " + DependentPosition);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return;
		}
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handlers - ErmBindingSource
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Raises the <see cref="BindingSource.CurrentChanged"/> event and updates the <see cref="Dependent"/>'s
	/// <see cref="BindingSource.Filter"/>.
	/// </summary>
	/// <param name="e">
	/// An System.EventArgs that contains the event data.
	/// </param>
	// ---------------------------------------------------------------------------------
	protected override void OnCurrentChanged(EventArgs e)
	{
		// if (!InvalidatedChanging)
		// 	Invalidated = false;

		base.OnCurrentChanged(e);

		UpdateDependencyFilter();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The event handler for <see cref="BindingSource.CurrentChanged"/> of <see cref="Dependent"/>.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	// ---------------------------------------------------------------------------------
	private void OnDependencyCurrentChanged(object sender, EventArgs e)
	{
		if (_FilterChanging)
			return;

		((EventHandler)Events[EVENT_DEPENDENCYCURRENTCHANGED])?.Invoke(this, e);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The event handler when the data source list of <see cref="Dependent"/> changes after
	/// the master <see cref="BindingSource"/>'s <see cref="BindingSource.Current"/> has changed.
	/// </summary>
	/// <param name="e"></param>
	// ---------------------------------------------------------------------------------
	protected virtual void OnDependencyListChanged(ListChangedEventArgs e)
	{
		((ListChangedEventHandler)Events[EVENT_DEPENDENCYLISTCHANGED])?.Invoke(this, e);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The event handler for <see cref="BindingSource.PositionChanged"/> of <see cref="Dependent"/>.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	// ---------------------------------------------------------------------------------
	private void OnDependencyPositionChanged(object sender, EventArgs e)
	{
		if (_FilterChanging)
			return;

		((EventHandler)Events[EVENT_DEPENDENCYPOSITIONCHANGED])?.Invoke(this, e);
	}


	#endregion Event Handlers


}
