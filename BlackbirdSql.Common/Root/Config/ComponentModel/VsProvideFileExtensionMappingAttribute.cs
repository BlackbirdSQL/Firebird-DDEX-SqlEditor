
using System;
using System.Globalization;
using System.Threading;
using BlackbirdSql.Core;
using Microsoft.VisualStudio.Shell;




namespace BlackbirdSql.Common.Config.ComponentModel;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
class VsProvideFileExtensionMappingAttribute : RegistrationAttribute
{
	private readonly string _DefaultName;
	private readonly Type _FactoryType;
	private readonly short _NameResourceID;
	private readonly int _SortPriority;



	private string EditorRegKey => string.Format(CultureInfo.InvariantCulture, "FileExtensionMapping\\{0}", FactoryType.GUID.ToString("B"));

	public Type FactoryType => _FactoryType;
	public short NameResourceID => _NameResourceID;



	public VsProvideFileExtensionMappingAttribute(Type factoryType, string defaultName, short nameResourceID, int sortPriority = 0)
	{
		_FactoryType = factoryType;
		_DefaultName = defaultName;
		_NameResourceID = nameResourceID;
		_SortPriority = sortPriority;
	}

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