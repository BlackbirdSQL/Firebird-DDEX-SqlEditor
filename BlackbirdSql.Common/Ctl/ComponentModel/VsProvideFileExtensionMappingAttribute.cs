﻿
using System;
using System.Globalization;
using System.Threading;
using BlackbirdSql.Core;
using Microsoft.VisualStudio.Shell;




namespace BlackbirdSql.Common.Ctl.ComponentModel;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
class VsProvideFileExtensionMappingAttribute(Type factoryType, string defaultName, short nameResourceID, int sortPriority = 0) : RegistrationAttribute
{
	private readonly string _DefaultName = defaultName;
	private readonly Type _FactoryType = factoryType;
	private readonly short _NameResourceID = nameResourceID;
	private readonly int _SortPriority = sortPriority;




	private string EditorRegKey => string.Format(CultureInfo.InvariantCulture, "FileExtensionMapping\\{0}", FactoryType.GUID.ToString("B"));

	public Type FactoryType => _FactoryType;
	public short NameResourceID => _NameResourceID;

	public override void Register(RegistrationContext context)
	{
		Key key = null;

		try
		{
			key = context.CreateKey(EditorRegKey);

			key.SetValue(string.Empty, _DefaultName);
			key.SetValue("DisplayName", string.Format(CultureInfo.InvariantCulture, "#{0}", NameResourceID));
			key.SetValue("EditorGUID", FactoryType.GUID.ToString("B"));
			key.SetValue("Package", context.ComponentType.GUID.ToString("B"));
			key.SetValue("SortPriority", _SortPriority);
		}
		finally
		{
			DisposeKey(ref key);
		}

	}

	public override void Unregister(RegistrationContext context)
	{
		if (context == null)
		{
			ArgumentNullException ex = new("context");
			Diag.Dug(ex);
			throw ex;
		}
		context.RemoveKey(EditorRegKey);
	}

	private static void DisposeKey(ref Key key)
	{
		if (key != null)
		{
			Key lockKey = Interlocked.Exchange(ref key, null);
			lockKey.Close();
			((IDisposable)lockKey).Dispose();
		}
	}

}