// Microsoft.VisualStudio.Shell.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Shell.ProvideOptionPageAttribute

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using BlackbirdSql.Core.Properties;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.Core.Ctl.ComponentModel;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]


public sealed class VsProvideOptionPageAttribute : ProvideOptionDialogPageAttribute
{

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public VsProvideOptionPageAttribute(Type pageType, string categoryName, string subCategoryName, string pageName, short categoryResourceID, short subCategoryResourceID, short pageNameResourceID, bool supportsAutomation = true)
	: this(pageType, categoryName, subCategoryName, null, pageName, categoryResourceID, subCategoryResourceID, -1, pageNameResourceID, supportsAutomation, [])
	{
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public VsProvideOptionPageAttribute(Type pageType, string categoryName, string subCategoryName, string subSubCategoryName, string pageName, short categoryResourceID, short subCategoryResourceID, short subSubCategoryResourceID, short pageNameResourceID, bool supportsAutomation = true)
	: this(pageType, categoryName, subCategoryName, subSubCategoryName, pageName, categoryResourceID, subCategoryResourceID, subSubCategoryResourceID, pageNameResourceID, supportsAutomation, [])
	{
	}

	public VsProvideOptionPageAttribute(Type pageType, string categoryName, string subCategoryName, string pageName, short categoryResourceID, short subCategoryResourceID, short pageNameResourceID, bool supportsAutomation, string keywordListResourceName)
		: this(pageType, categoryName, subCategoryName, null, pageName, categoryResourceID, subCategoryResourceID, -1, pageNameResourceID, supportsAutomation, ["@" + keywordListResourceName])
	{
	}

	public VsProvideOptionPageAttribute(Type pageType, string categoryName, string subCategoryName, string subSubCategoryName, string pageName, short categoryResourceID, short subCategoryResourceID, short subSubCategoryResourceID, short pageNameResourceID, bool supportsAutomation, string keywordListResourceName)
	: this(pageType, categoryName, subCategoryName, subSubCategoryName, pageName, categoryResourceID, subCategoryResourceID, subSubCategoryResourceID, pageNameResourceID, supportsAutomation, ["@" + keywordListResourceName])
	{
	}

	public VsProvideOptionPageAttribute(Type pageType, string categoryName, string subCategoryName, string pageName, short categoryResourceID, short subCategoryResourceID, short pageNameResourceID, bool supportsAutomation, int keywordListResourceId)
		: this(pageType, categoryName, subCategoryName, null, pageName, categoryResourceID, subCategoryResourceID, -1, pageNameResourceID, supportsAutomation, ["#" + keywordListResourceId])
	{
	}

	public VsProvideOptionPageAttribute(Type pageType, string categoryName, string subCategoryName, string subSubCategoryName,
		string pageName, short categoryResourceID, short subCategoryResourceID, short subSubCategoryResourceID,
		short pageNameResourceID, bool supportsAutomation, string[] keywords) : base(pageType, "#" + pageNameResourceID)
	{
		CategoryName = categoryName ?? throw new ArgumentNullException("categoryName");
		SubCategoryName = subCategoryName ?? throw new ArgumentNullException("pageName");
		SubSubCategoryName = subSubCategoryName;
		PageName = pageName ?? throw new ArgumentNullException("pageName");

		CategoryResourceID = categoryResourceID;
		SubCategoryResourceID = subCategoryResourceID;
		SubSubCategoryResourceID = subSubCategoryResourceID;

		SupportsAutomation = supportsAutomation;
		Keywords = keywords;
		Sort = C_UnspecifiedSortValue;
		ProfileMigrationType = ProfileMigrationType.None;
		ProvidesLocalizedCategoryName = true;
		// _PageType = pageType;
		// _PageNameResourceId = "#" + pageNameResourceID;
	}





	private const int C_UnspecifiedSortValue = int.MinValue;
	private const string C_IsServerAware = "IsServerAware";

	private static readonly char[] S_UIContextSeparators = [';'];

	// private readonly Type _PageType;

	// private readonly string _PageNameResourceId;



	public bool NoShowAllView { get; set; }

	public override object TypeId => this;

	public string CategoryName { get; private set; }

	public string SubCategoryName { get; private set; }

	public string SubSubCategoryName { get; private set; }

	public short CategoryResourceID { get; private set; }
	public short SubCategoryResourceID { get; private set; }
	public short SubSubCategoryResourceID { get; private set; }

	public string PageName { get; private set; }

	public bool SupportsAutomation { get; private set; }

	public bool SupportsProfiles { get; set; } = true;

	public ProfileMigrationType ProfileMigrationType { get; set; }

	public bool ProvidesLocalizedCategoryName { get; set; }

	public string[] Keywords { get; private set; }

	public int Sort { get; set; }

	public string VisibilityCmdUIContexts { get; set; }

	public string CategoryPackageGuid { get; set; }

	public string DescriptionResourceId { get; set; }

	public string CategoryDescriptionResourceId { get; set; }

	public bool IsServerAware { get; set; }


	private string ToolsOptionsCategoryRegKey => string.Format(CultureInfo.InvariantCulture, "ToolsOptionsPages\\{0}", CategoryName);

	// private string ToolsOptionsSubCategoryRegKey => string.Format(CultureInfo.InvariantCulture, "ToolsOptionsPages\\{0}\\{1}", CategoryName, SubCategoryName);

	private string AutomationCategoryRegKey => string.Format(CultureInfo.InvariantCulture, "AutomationProperties\\{0}", CategoryName);
	// private string AutomationSubCategoryRegKey => string.Format(CultureInfo.InvariantCulture, "AutomationProperties\\{0}\\{1}", CategoryName, SubCategoryName);

	private string AutomationCategoryPageRegKey => SubSubCategoryName == null
		? string.Format(CultureInfo.InvariantCulture, "{0}\\{1}|{2}", AutomationCategoryRegKey, SubCategoryName, PageName)
		: string.Format(CultureInfo.InvariantCulture, "{0}\\{1}|{2}|{3}", AutomationCategoryRegKey, SubCategoryName, SubSubCategoryName, PageName);
	// private string AutomationSubCategoryPageRegKey => string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", AutomationSubCategoryRegKey, PageName);



	public override void Register(RegistrationContext context)
	{
		context.Log.WriteLine(string.Format(AttributeResources.Culture, AttributeResources.Reg_NotifyOptionPage, CategoryName, SubCategoryName, PageName));

		Key key = null, key1 = null, key2 = null;
		Key keyd = null, keye = null, keyf = null;

		try
		{

			key = context.CreateKey(ToolsOptionsCategoryRegKey);

			if (ProvidesLocalizedCategoryName)
			{
				key.SetValue("", string.Format(CultureInfo.InvariantCulture, "#{0}", CategoryResourceID));
				key.SetValue("Package", context.ComponentType.GUID.ToString("B"));
			}

			key1 = key.CreateSubkey(SubCategoryName);

			if (SubSubCategoryName != null)
				key2 = key1.CreateSubkey(SubSubCategoryName);

			if (ProvidesLocalizedCategoryName)
			{
				key1.SetValue("", string.Format(CultureInfo.InvariantCulture, "#{0}", SubCategoryResourceID));
				key1.SetValue("Package", context.ComponentType.GUID.ToString("B"));

				if (SubSubCategoryName != null)
				{
					key2.SetValue("", string.Format(CultureInfo.InvariantCulture, "#{0}", SubSubCategoryResourceID));
					key2.SetValue("Package", context.ComponentType.GUID.ToString("B"));
				}
			}


			if (SubSubCategoryName == null)
				keyd = key1.CreateSubkey(PageName);
			else
				keyd = key2.CreateSubkey(PageName);

			keyd.SetValue("", base.PageNameResourceId);
			keyd.SetValue("Package", context.ComponentType.GUID.ToString("B"));
			keyd.SetValue("Page", base.PageType.GUID.ToString("B"));
			keyd.SetValue(C_IsServerAware, IsServerAware ? 1 : 0);
			if (NoShowAllView)
			{
				keyd.SetValue("NoShowAllView", 1);
			}
			if (Sort != C_UnspecifiedSortValue)
			{
				keyd.SetValue("Sort", Sort);
			}
			if (Keywords != null && Keywords.Length != 0)
			{
				keyd.SetValue("Keywords", string.Join(";", Keywords));
			}
			if (!string.IsNullOrWhiteSpace(VisibilityCmdUIContexts))
			{
				string[] array = VisibilityCmdUIContexts.Split(S_UIContextSeparators, StringSplitOptions.RemoveEmptyEntries);
				List<Guid> list = [];
				string[] array2 = array;
				foreach (string input in array2)
				{
					if (Guid.TryParse(input, out var result))
					{
						list.Add(result);
					}
				}
				if (list.Count > 0)
				{
					keye = keyd.CreateSubkey("VisibilityCmdUIContexts");
					foreach (Guid item in list)
					{
						keye.SetValue(item.ToString("B"), 1);
					}
				}
			}

			if (!SupportsAutomation)
			{
				return;
			}
			string text = context.ComponentType.GUID.ToString("B");
			string package = (string.IsNullOrEmpty(CategoryPackageGuid) ? text : CategoryPackageGuid);
			SetPackageAndResources(context, AutomationCategoryRegKey, package, "#" + CategoryResourceID, CategoryDescriptionResourceId);
			SetPackageAndResources(context, AutomationCategoryPageRegKey, text, base.PageNameResourceId.ToString(), DescriptionResourceId);
			keyf = context.CreateKey(AutomationCategoryPageRegKey);

			if (SubSubCategoryName == null)
				keyf.SetValue("Name", string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", CategoryName, SubCategoryName, PageName));
			else
				keyf.SetValue("Name", string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", CategoryName, SubCategoryName, SubSubCategoryName, PageName));

			if (SupportsProfiles)
			{
				keyf.SetValue("ProfileSave", 1);
				keyf.SetValue("VSSettingsMigration", (int)ProfileMigrationType);
			}
		}
		finally
		{
			DisposeKey(ref key);
			DisposeKey(ref key1);
			DisposeKey(ref key2);
			DisposeKey(ref keyd);
			DisposeKey(ref keye);
			DisposeKey(ref keyf);
		}
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


	private static void SetPackageAndResources(RegistrationContext context, string regPath, string package, string name, string description)
	{
		Key key = context.CreateKey(regPath);

		key.SetValue("Package", package);
		if (!string.IsNullOrEmpty(name) && name != "#0")
		{
			key.SetValue("", name);
		}
		if (!string.IsNullOrEmpty(description) && description != "#0")
		{
			key.SetValue("Description", description);
		}
		DisposeKey(ref key);
	}

	public override void Unregister(RegistrationContext context)
	{
		context.RemoveKey(ToolsOptionsCategoryRegKey);
		if (SupportsAutomation)
		{
			context.RemoveKey(AutomationCategoryPageRegKey);
			context.RemoveKeyIfEmpty(AutomationCategoryRegKey);
		}
	}
}
