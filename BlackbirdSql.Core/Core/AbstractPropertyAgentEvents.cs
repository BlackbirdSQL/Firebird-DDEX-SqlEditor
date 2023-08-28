

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Core.Interfaces;




namespace BlackbirdSql.Core;


// =========================================================================================================
//
//									AbstractPropertyAgent Class - Events
//
// =========================================================================================================
public abstract partial class AbstractPropertyAgent : IBPropertyAgent
{

	// ---------------------------------------------------------------------------------
	#region Event Variables - AbstractPropertyAgent
	// ---------------------------------------------------------------------------------


	protected bool _ParametersChanged = false;

	protected PropertyChangedEventHandler _DispatcherPropertyChanged;
	protected EventHandler<DataErrorsChangedEventArgs> _ErrorsChanged;

	protected IBEventsChannel _Channel;


	#endregion Event Variables





	// =========================================================================================================
	#region Event Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public virtual IBEventsChannel Channel => _Channel;

	public virtual event EventHandler PropertyChanged;

	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add { _DispatcherPropertyChanged += value; }
		remove { _DispatcherPropertyChanged -= value; }
	}

	public virtual event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
	{
		add { _ErrorsChanged += value; }
		remove { _ErrorsChanged -= value; }
	}


	#endregion Event Accessors






	// =========================================================================================================
	#region Event Methods - AbstractPropertyAgent
	// =========================================================================================================


	public virtual EventDescriptorCollection GetEvents()
	{
		return TypeDescriptor.GetEvents(ConnectionStringBuilder, noCustomTypeDesc: true);
	}



	public virtual EventDescriptorCollection GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(ConnectionStringBuilder, attributes, noCustomTypeDesc: true);
	}



	public virtual void RaisePropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}



	public virtual void RaisePropertyChanged<TKey, TValue>(Dictionary<TKey, TValue> dictionary,
		TKey key, params string[] propertiesToExclude) where TValue : IEnumerable<string>
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			return;
		}

		foreach (string item in value.Except(propertiesToExclude))
		{
			RaisePropertyChanged(item);
		}
	}


	#endregion Event Methods





	// =========================================================================================================
	#region Event Handling - AbstractPropertyAgent
	// =========================================================================================================


	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		PropertyChanged?.Invoke(this, e);
	}




	public virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		if (!ReceiveWeakEvent(managerType, sender, e))
		{
			UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error,
				EnUiTraceId.UiInfra, "Weak event was not handled");
			return false;
		}

		return true;
	}

	#endregion Event Handling


}
