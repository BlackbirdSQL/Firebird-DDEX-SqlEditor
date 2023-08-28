#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.NotifyPropertyChangedDispatcherConnection
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;




namespace BlackbirdSql.Common.Model;


// =========================================================================================================
//									AbstractDispatcherConnection Class
//
/// <summary>
/// Merges SqlServer's NotifyPropertyChangedDispatcherConnection into the
/// <see cref="AbstractModelPropertyAgent"/> universal class.
/// </summary>
// =========================================================================================================
public class AbstractDispatcherConnection : AbstractModelPropertyAgent
{

	// ---------------------------------------------------------------------------------
	#region Variables - AbstractDispatcherConnection
	// ---------------------------------------------------------------------------------


	private readonly Dictionary<string, List<string>> _InternalPropertyDependencies;

	private readonly Lazy<Dictionary<INotifyPropertyChanged, string>> _ExternalPropertySources;

	private readonly Dictionary<string, Dictionary<string, HashSet<string>>> _ExternalPropertyDependencies;

	private readonly Lazy<Dictionary<INotifyCollectionChanged, string>> _CollectionSources;

	private readonly Dictionary<string, HashSet<string>> _CollectionPropertyDependencies;

	private readonly Dispatcher _Dispatcher;


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - AbstractDispatcherConnection
	// =========================================================================================================


	public Dispatcher Dispatcher => _Dispatcher;


	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - AbstractDispatcherConnection
	// =========================================================================================================


	/// <summary>
	/// Universal .ctor
	/// </summary>
	public AbstractDispatcherConnection(Dispatcher dispatcher, IBEventsChannel channel,
		AbstractDispatcherConnection rhs, bool generateNewId) : base(channel, rhs, generateNewId)
	{
		Cmd.CheckForNull(dispatcher, "dispatcher");
		_Dispatcher = dispatcher;
		_ExternalPropertySources = new Lazy<Dictionary<INotifyPropertyChanged, string>>();
		_CollectionSources = new Lazy<Dictionary<INotifyCollectionChanged, string>>();
		_InternalPropertyDependencies = BuildInternalPropertyDependencies();
		_ExternalPropertyDependencies = BuildExternalPropertyDependencies();
		_CollectionPropertyDependencies = BuildCollectionPropertyDependencies();
	}

	public AbstractDispatcherConnection(IBEventsChannel channel, AbstractDispatcherConnection rhs, bool generateNewId = true)
	: this(Dispatcher.CurrentDispatcher, channel, rhs, generateNewId)
	{
	}

	public AbstractDispatcherConnection(Dispatcher dispatcher, AbstractDispatcherConnection rhs, bool generateNewId = true)
		: this(dispatcher, null, rhs, generateNewId)
	{
	}

	public AbstractDispatcherConnection(AbstractDispatcherConnection rhs, bool generateNewId = true)
	: this(Dispatcher.CurrentDispatcher, null, rhs, generateNewId)
	{
	}


	public AbstractDispatcherConnection() : this(Dispatcher.CurrentDispatcher, null, null, true)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - AbstractDispatcherConnection
	// =========================================================================================================


	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool CheckAccess()
	{
		return Dispatcher.CheckAccess();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected void VerifyAccess()
	{
		Dispatcher.VerifyAccess();
	}

	public void CheckAccessExecute(Action action)
	{
		Cmd.CheckForNull(action, "action");
		if (CheckAccess())
		{
			action();
		}
		else
		{
			Dispatcher.Invoke(action, DispatcherPriority.Normal);
		}
	}

	public void CheckAccessBeginExecute(Action action)
	{
		Cmd.CheckForNull(action, "action");
		if (CheckAccess())
		{
			action();
		}
		else
		{
			Dispatcher.BeginInvoke(action, DispatcherPriority.Normal);
		}
	}

	public TResult CheckAccessExecute<TResult>(Func<TResult> func)
	{
		Cmd.CheckForNull(func, "action");
		if (CheckAccess())
		{
			return func();
		}

		return Dispatcher.Invoke(func, DispatcherPriority.Normal);
	}


	protected override void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		// Do nothing
	}



	protected void AddDependencySource(string name, INotifyPropertyChanged source)
	{
		Cmd.CheckStringForNullOrEmpty(name, "name");
		Cmd.CheckForNull(source, "source");

		UiTracer.TraceSource.AssertTraceEvent(_ExternalPropertyDependencies != null
			&& _ExternalPropertyDependencies.ContainsKey(name), TraceEventType.Error,
			EnUiTraceId.UiInfra, $"No dependencies have been declared for {name}");

		_ExternalPropertySources.Value.Add(source, name);
		PropertyChangedEventManager.AddListener(source, this, string.Empty);
	}

	protected void RemoveDependencySource(string name, INotifyPropertyChanged source)
	{
		Cmd.CheckStringForNullOrEmpty(name, "name");
		Cmd.CheckForNull(source, "source");
		PropertyChangedEventManager.RemoveListener(source, this, string.Empty);
		_ExternalPropertySources.Value.Remove(source);
	}

	protected void AddDependencySource(string name, INotifyCollectionChanged source)
	{
		Cmd.CheckStringForNullOrEmpty(name, "name");
		Cmd.CheckForNull(source, "source");

		UiTracer.TraceSource.AssertTraceEvent(_CollectionPropertyDependencies != null
			&& _CollectionPropertyDependencies.ContainsKey(name), TraceEventType.Error,
			EnUiTraceId.UiInfra, $"No dependencies have been declared for {name}");

		_CollectionSources.Value.Add(source, name);
		CollectionChangedEventManager.AddListener(source, this);
	}

	protected void RemoveDependencySource(string name, INotifyCollectionChanged source)
	{
		Cmd.CheckStringForNullOrEmpty(name, "name");
		Cmd.CheckForNull(source, "source");
		CollectionChangedEventManager.RemoveListener(source, this);
		_CollectionSources.Value.Remove(source);
	}

	protected string GetDependencySourceName(INotifyPropertyChanged source)
	{
		string value = null;
		if (_ExternalPropertySources.IsValueCreated)
		{
			_ExternalPropertySources.Value.TryGetValue(source, out value);
		}

		return value;
	}

	protected string GetDependencySourceName(INotifyCollectionChanged source)
	{
		string value = null;
		if (_CollectionSources.IsValueCreated)
		{
			_CollectionSources.Value.TryGetValue(source, out value);
		}

		return value;
	}





	[Conditional("DEBUG")]
	protected void ValidateExternalPropertyDependencies(string sourceName, INotifyPropertyChanged source)
	{
		if (_ExternalPropertyDependencies == null)
		{
			return;
		}

		HashSet<string> hashSet = new(
			from property
			in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			select property.Name)
		{
			string.Empty
		};

		foreach (string item in _ExternalPropertyDependencies[sourceName].Keys.Except(hashSet))
		{
			UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error,
				EnUiTraceId.UiInfra, $"{item} is not a property of {source.GetType().Name}");
		}
	}

	private Dictionary<string, List<string>> BuildInternalPropertyDependencies()
	{
		Dictionary<string, List<string>> dictionary = null;

		PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance
			| BindingFlags.Public | BindingFlags.NonPublic);

		foreach (PropertyInfo propertyInfo in properties)
		{
			DependsOnPropertyAttribute[] array = propertyInfo.GetCustomAttributes(typeof(DependsOnPropertyAttribute),
				inherit: true).Cast<DependsOnPropertyAttribute>().ToArray();

			foreach (DependsOnPropertyAttribute obj in array)
			{
				dictionary ??= new Dictionary<string, List<string>>();

				string[] propertyNames = obj.PropertyNames;

				foreach (string text in propertyNames)
				{
					string propertyName = text;

					if (string.IsNullOrEmpty(propertyName))
					{
						propertyName = string.Empty;
					}
					else
					{
						UiTracer.TraceSource.AssertTraceEvent(properties.FirstOrDefault((prop)
							=> prop.Name.Equals(propertyName, StringComparison.Ordinal)) != null, TraceEventType.Error, EnUiTraceId.UiInfra,
							$"Dependent property not found: {propertyName}, Property: {propertyInfo.Name}, Type: {propertyInfo.DeclaringType.Name}");
					}

					if (!dictionary.TryGetValue(propertyName, out var value))
					{
						value = new List<string>();
						dictionary.Add(propertyName, value);
					}

					value.Add(propertyInfo.Name);
				}
			}
		}

		return dictionary;
	}

	private Dictionary<string, Dictionary<string, HashSet<string>>> BuildExternalPropertyDependencies()
	{
		Dictionary<string, Dictionary<string, HashSet<string>>> dictionary = null;

		PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance
			| BindingFlags.Public | BindingFlags.NonPublic);

		foreach (PropertyInfo propertyInfo in properties)
		{
			foreach (ValueDependsOnExternalPropertyAttribute customAttribute
				in propertyInfo.GetCustomAttributes<ValueDependsOnExternalPropertyAttribute>(inherit: false))
			{
				dictionary ??= new Dictionary<string, Dictionary<string, HashSet<string>>>();

				if (!dictionary.TryGetValue(customAttribute.SourceName, out var value))
				{
					value = new Dictionary<string, HashSet<string>>();
					dictionary.Add(customAttribute.SourceName, value);
				}

				if (!value.TryGetValue(customAttribute.PropertyName, out var value2))
				{
					value2 = new HashSet<string>();
					value.Add(customAttribute.PropertyName, value2);
				}

				value2.Add(propertyInfo.Name);
			}
		}

		return dictionary;
	}

	private Dictionary<string, HashSet<string>> BuildCollectionPropertyDependencies()
	{
		Dictionary<string, HashSet<string>> dictionary = null;

		PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance
			| BindingFlags.Public | BindingFlags.NonPublic);

		foreach (PropertyInfo propertyInfo in properties)
		{
			foreach (ValueDependsOnCollectionAttribute customAttribute
				in propertyInfo.GetCustomAttributes<ValueDependsOnCollectionAttribute>(inherit: false))
			{
				dictionary ??= new Dictionary<string, HashSet<string>>();

				if (!dictionary.TryGetValue(customAttribute.SourceName, out var value))
				{
					value = new HashSet<string>();
					dictionary.Add(customAttribute.SourceName, value);
				}

				value.Add(propertyInfo.Name);
			}
		}

		return dictionary;
	}



	public override IBPropertyAgent Copy()
	{
		NotImplementedException ex = new("AbstractDispatcherConnection::Copy");
		Diag.Dug(ex);
		throw ex;
	}


	protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
	{
		// This class does not have private descriptors. Just pass request on.
		AbstractPropertyAgent.CreateAndPopulatePropertySet(describers);
	}



	public override void RaisePropertyChanged(string propertyName)
	{
		CheckAccessExecute(delegate
		{
			_DispatcherPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

			if (_InternalPropertyDependencies != null && !string.IsNullOrEmpty(propertyName))
			{
				RaisePropertyChanged(_InternalPropertyDependencies, propertyName);
				RaisePropertyChanged(_InternalPropertyDependencies, string.Empty, propertyName);
			}
		});
	}


	#endregion Methods - AbstractDispatcherConnection
}
