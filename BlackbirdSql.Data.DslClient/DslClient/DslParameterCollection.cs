/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Collections;

using BlackbirdSql.Common;



namespace BlackbirdSql.Data.DslClient;

[ListBindable(false)]
public sealed class DslParameterCollection : DbParameterCollection
{
	#region Fields

	private List<DslParameter> _parameters;
	private bool? _hasParameterWithNonAsciiName;

	#endregion

	#region Indexers

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new DslParameter this[string parameterName]
	{
		get { return this[IndexOf(parameterName)]; }
		set { this[IndexOf(parameterName)] = value; }
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new DslParameter this[int index]
	{
		get { return _parameters[index]; }
		set { _parameters[index] = value; }
	}

	#endregion

	#region DbParameterCollection overriden properties

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override int Count
	{
		get { return _parameters.Count; }
	}

	public override bool IsFixedSize
	{
		get { return ((IList)_parameters).IsFixedSize; }
	}

	public override bool IsReadOnly
	{
		get { return ((IList)_parameters).IsReadOnly; }
	}

	public override bool IsSynchronized
	{
		get { return ((ICollection)_parameters).IsSynchronized; }
	}

	public override object SyncRoot
	{
		get { return ((ICollection)_parameters).SyncRoot; }
	}

	#endregion

	#region Internal properties

	internal bool HasParameterWithNonAsciiName
	{
		get
		{
			return _hasParameterWithNonAsciiName ?? (bool)(_hasParameterWithNonAsciiName = _parameters.Any(x => x.IsUnicodeParameterName));
		}
	}

	#endregion

	#region Constructors

	internal DslParameterCollection()
	{
		_parameters = new List<DslParameter>();
		_hasParameterWithNonAsciiName = null;
	}

	#endregion

	#region DbParameterCollection overriden methods

	public void AddRange(IEnumerable<DslParameter> values)
	{
		foreach (var p in values)
		{
			Add(p);
		}
	}

	public override void AddRange(Array values)
	{
		AddRange(values.Cast<object>().Select(x => { EnsureFbParameterType(x); return (DslParameter)x; }));
	}

	public DslParameter AddWithValue(string parameterName, object value)
	{
		return Add(new DslParameter(parameterName, value));
	}

	public DslParameter Add(string parameterName, object value)
	{
		return Add(new DslParameter(parameterName, value));
	}

	public DslParameter Add(string parameterName, DslDbType type)
	{
		return Add(new DslParameter(parameterName, type));
	}

	public DslParameter Add(string parameterName, DslDbType fbType, int size)
	{
		return Add(new DslParameter(parameterName, fbType, size));
	}

	public DslParameter Add(string parameterName, DslDbType fbType, int size, string sourceColumn)
	{
		return Add(new DslParameter(parameterName, fbType, size, sourceColumn));
	}

	public DslParameter Add(DslParameter value)
	{
		EnsureFbParameterAddOrInsert(value);

		AttachParameter(value);
		_parameters.Add(value);
		return value;
	}

	public override int Add(object value)
	{
		EnsureFbParameterType(value);

		return IndexOf(Add((DslParameter)value));
	}

	public bool Contains(DslParameter value)
	{
		return _parameters.Contains(value);
	}

	public override bool Contains(object value)
	{
		EnsureFbParameterType(value);

		return Contains((DslParameter)value);
	}

	public override bool Contains(string parameterName)
	{
		return IndexOf(parameterName) != -1;
	}

	public int IndexOf(DslParameter value)
	{
		return _parameters.IndexOf(value);
	}

	public override int IndexOf(object value)
	{
		EnsureFbParameterType(value);

		return IndexOf((DslParameter)value);
	}

	public override int IndexOf(string parameterName)
	{
		return IndexOf(parameterName, -1);
	}

	internal int IndexOf(string parameterName, int luckyIndex)
	{
		var isNonAsciiParameterName = DslParameter.IsNonAsciiParameterName(parameterName);
		var usedComparison = isNonAsciiParameterName || HasParameterWithNonAsciiName
			? StringComparison.CurrentCultureIgnoreCase
			: StringComparison.OrdinalIgnoreCase;
		var normalizedParameterName = DslParameter.NormalizeParameterName(parameterName);
		if (luckyIndex != -1 && luckyIndex < _parameters.Count)
		{
			if (_parameters[luckyIndex].InternalParameterName.Equals(normalizedParameterName, usedComparison))
			{
				return luckyIndex;
			}
		}

		return _parameters.FindIndex(x => x.InternalParameterName.Equals(normalizedParameterName, usedComparison));
	}

	public void Insert(int index, DslParameter value)
	{
		EnsureFbParameterAddOrInsert(value);

		AttachParameter(value);
		_parameters.Insert(index, value);
	}

	public override void Insert(int index, object value)
	{
		EnsureFbParameterType(value);

		Insert(index, (DslParameter)value);
	}

	public void Remove(DslParameter value)
	{
		if (!_parameters.Remove(value))
		{
			Diag.Dug(true, "The parameter does not exist in the collection.");
			throw new ArgumentException("The parameter does not exist in the collection.");
		}

		ReleaseParameter(value);
	}

	public override void Remove(object value)
	{
		EnsureFbParameterType(value);

		Remove((DslParameter)value);
	}

	public override void RemoveAt(int index)
	{
		if (index < 0 || index > Count)
		{
			Diag.Dug(true, "Index out of range: " + index.ToString());
			throw new IndexOutOfRangeException("The specified index does not exist.");
		}

		var parameter = this[index];
		_parameters.RemoveAt(index);
		ReleaseParameter(parameter);
	}

	public override void RemoveAt(string parameterName)
	{
		RemoveAt(IndexOf(parameterName));
	}

	public void CopyTo(DslParameter[] array, int index)
	{
		_parameters.CopyTo(array, index);
	}

	public override void CopyTo(Array array, int index)
	{
		((IList)_parameters).CopyTo(array, index);
	}

	public override void Clear()
	{
		var parameters = _parameters.ToArray();
		_parameters.Clear();
		foreach (var parameter in parameters)
		{
			ReleaseParameter(parameter);
		}
	}

	public override IEnumerator GetEnumerator()
	{
		return _parameters.GetEnumerator();
	}

	#endregion

	#region DbParameterCollection overriden protected methods

	protected override DbParameter GetParameter(string parameterName)
	{
		return this[parameterName];
	}

	protected override DbParameter GetParameter(int index)
	{
		return this[index];
	}

	protected override void SetParameter(int index, DbParameter value)
	{
		this[index] = (DslParameter)value;
	}

	protected override void SetParameter(string parameterName, DbParameter value)
	{
		this[parameterName] = (DslParameter)value;
	}

	#endregion

	#region Internal Methods

	internal void ParameterNameChanged()
	{
		_hasParameterWithNonAsciiName = null;
	}

	#endregion

	#region Private Methods

	private string GenerateParameterName()
	{
		var index = Count + 1;
		while (true)
		{
			var name = "Parameter" + index.ToString(CultureInfo.InvariantCulture);
			if (!Contains(name))
			{
				return name;
			}
			index++;
		}
	}

	private void EnsureFbParameterType(object value)
	{
		if (!(value is DslParameter))
		{
			Diag.Dug(true, $"The parameter passed was not a {nameof(DslParameter)}.");
			throw new InvalidCastException($"The parameter passed was not a {nameof(DslParameter)}.");
		}
	}

	private void EnsureFbParameterAddOrInsert(DslParameter value)
	{
		if (value == null)
		{
			Diag.Dug(true, "Null argument");
			throw new ArgumentNullException();
		}
		if (value.Parent != null)
		{
			Diag.Dug(true, $"The {nameof(DslParameter)} specified in the value parameter is already added to this or another {nameof(DslParameterCollection)}.");
			throw new ArgumentException($"The {nameof(DslParameter)} specified in the value parameter is already added to this or another {nameof(DslParameterCollection)}.");
		}
		if (value.ParameterName == null || value.ParameterName.Length == 0)
		{
			value.ParameterName = GenerateParameterName();
		}
		else
		{
			if (Contains(value.ParameterName))
			{
				Diag.Dug(true, $"{nameof(DslParameterCollection)} already contains {nameof(DslParameter)} with {nameof(DslParameter.ParameterName)} '{value.ParameterName}'.");
				throw new ArgumentException($"{nameof(DslParameterCollection)} already contains {nameof(DslParameter)} with {nameof(DslParameter.ParameterName)} '{value.ParameterName}'.");
			}
		}
	}

	private void AttachParameter(DslParameter parameter)
	{
		parameter.Parent = this;
	}

	private void ReleaseParameter(DslParameter parameter)
	{
		parameter.Parent = null;
	}

	#endregion
}
