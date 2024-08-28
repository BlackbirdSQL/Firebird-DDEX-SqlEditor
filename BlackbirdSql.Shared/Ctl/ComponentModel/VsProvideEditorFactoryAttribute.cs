// Microsoft.VisualStudio.Shell.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Shell.ProvideEditorFactoryAttribute

using System;
using System.ComponentModel;
using System.Globalization;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Shared.Ctl.ComponentModel;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]


public sealed class VsProvideEditorFactoryAttribute(Type factoryType, short nameResourceID) : RegistrationAttribute
{
	private readonly Type _factoryType = factoryType ?? throw new ArgumentNullException("factoryType");

	private readonly short _nameResourceID = nameResourceID;


	public VsProvideEditorFactoryAttribute(Type factoryType, short nameResourceID, bool deferUntilIntellisenseIsReady)
	: this(factoryType, nameResourceID)
	{
		DeferUntilIntellisenseIsReady = deferUntilIntellisenseIsReady;
	}



	private string _DefaultName = null;

	private __VSEDITORTRUSTLEVEL _trustLevel = __VSEDITORTRUSTLEVEL.ETL_NeverTrusted;

	public Type FactoryType => _factoryType;

	public string DefaultName
	{
		get
		{
			if (_DefaultName == null)
				return _factoryType.Name;
			return _DefaultName;
		}
		set
		{
			_DefaultName = value;
		}
	}
	public __VSEDITORTRUSTLEVEL TrustLevel
	{
		get
		{
			return _trustLevel;
		}
		set
		{
			_trustLevel = value;
		}
	}

	public bool? DeferUntilIntellisenseIsReady { get; } = null;

	public int CommonPhysicalViewAttributes { get; set; } = 0;

	public short NameResourceID => _nameResourceID;

	private string EditorRegKey => string.Format(CultureInfo.InvariantCulture, "Editors\\{0}", FactoryType.GUID.ToString("B"));


	public override void Register(RegistrationContext context)
	{
		context.Log.WriteLine(AttributeResources.RegNotifyEditorFactory.FmtRes(FactoryType.Name));
		using Key key = context.CreateKey(EditorRegKey);
		key.SetValue("", DefaultName);
		key.SetValue("DisplayName", string.Format(CultureInfo.InvariantCulture, "#{0}", NameResourceID));
		key.SetValue("Package", context.ComponentType.GUID.ToString("B"));
		key.SetValue("EditorTrustLevel", (int)_trustLevel);
		key.SetValue("CommonPhysicalViewAttributes", CommonPhysicalViewAttributes);
		if (DeferUntilIntellisenseIsReady.HasValue)
		{
			key.SetValue("DeferUntilIntellisenseIsReady", DeferUntilIntellisenseIsReady.Value);
		}
		using (Key key2 = key.CreateSubkey("LogicalViews"))
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(LogicalView));
			object[] customAttributes = FactoryType.GetCustomAttributes(typeof(ProvideViewAttribute), inherit: true);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				ProvideViewAttribute provideViewAttribute = (ProvideViewAttribute)customAttributes[i];
				if (provideViewAttribute.LogicalView != 0)
				{
					context.Log.WriteLine(AttributeResources.RegNotifyEditorView.FmtRes(converter.ConvertToString(provideViewAttribute.LogicalView)));
					Guid guid = (Guid)converter.ConvertTo(provideViewAttribute.LogicalView, typeof(Guid));
					string text = provideViewAttribute.PhysicalView;
					text ??= "";
					key2.SetValue(guid.ToString("B"), text);
				}
			}
		}
		object[] customAttributes2 = FactoryType.GetCustomAttributes(typeof(ProvidePhysicalViewAttributesAttribute), inherit: true);
		if (customAttributes2.Length == 0)
		{
			return;
		}
		using Key key3 = key.CreateSubkey("PhysicalViewAttributes");
		object[] array = customAttributes2;
		for (int j = 0; j < array.Length; j++)
		{
			ProvidePhysicalViewAttributesAttribute providePhysicalViewAttributesAttribute = (ProvidePhysicalViewAttributesAttribute)array[j];
			key3.SetValue(providePhysicalViewAttributesAttribute.PhysicalView, (int)providePhysicalViewAttributesAttribute.Attributes);
		}
	}

	public override void Unregister(RegistrationContext context)
	{
		context.RemoveKey(EditorRegKey);
	}
}
