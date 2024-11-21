// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.ProvideTextEditorAutomationAttribute
using System;
using System.Globalization;
using BlackbirdSql.LanguageExtension.Properties;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.LanguageExtension.Ctl.ComponentModel;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class VsProvideEditorAutomationPageAttribute : ProvideOptionDialogPageAttribute
{

	public VsProvideEditorAutomationPageAttribute(Type pageType, string categoryName, string pageName, short categoryResourceID, short pageNameResourceID)
		: base(pageType, "#" + pageNameResourceID.ToString(CultureInfo.InvariantCulture))
	{
		_CategoryName = categoryName ?? throw new ArgumentNullException("categoryName");
		_PageName = pageName ?? throw new ArgumentNullException("pageName");
		_CategoryResourceID = categoryResourceID;
	}




	private readonly string _CategoryName;
	private readonly string _PageName;
	private readonly short _CategoryResourceID;




	public override object TypeId => this;

	public string CategoryName => _CategoryName;

	public short CategoryResourceId => _CategoryResourceID;

	public string PageName => _PageName;

	private static string AutomationTextEditorRegKey => "AutomationProperties\\TextEditor";

	private string AutomationCategoryRegKey => "{0}\\{1}".Fmti(AutomationTextEditorRegKey, CategoryName);





	public override void Register(RegistrationContext context)
	{
		context.Log.WriteLine(Resources.NotifyOptionPage.Fmt(CategoryName, PageName));
		using Key key = context.CreateKey(AutomationCategoryRegKey);
		key.SetValue(null, "#" + CategoryResourceId);
		key.SetValue("Name", CategoryName);
		key.SetValue("Package", context.ComponentType.GUID.ToString("B"));
		key.SetValue("ResourcePackage", context.ComponentType.GUID.ToString("B"));
		key.SetValue("ProfileSave", 1);
	}



	public override void Unregister(RegistrationContext context)
	{
		context.RemoveKey(AutomationCategoryRegKey);
	}
}
