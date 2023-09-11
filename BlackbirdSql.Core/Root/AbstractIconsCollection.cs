// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.ImageListBase<TIcons>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media.Imaging;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Core.Interfaces;




namespace BlackbirdSql.Core;


/// <summary>
/// Performs system-wide management of icons.
/// </summary>
/// <remarks>
/// Each class in the inheritance tree of AbstractIconsCollection can be instanciated as a singleton private instance.
/// However there may be multiple instances of the same parent class if it is a member of the ancestor trees of
/// different child classes.
/// An IconType consists of a Name, the absolute namespace path to the resource and a inique id that is allocated
/// when it is instantiated.
/// All instances of a class, whether it is the private singleton instance, or ancestor instances of different child
/// class instances, will share the IconTypes in it's class. In other words each IconType will be singleton globally
/// across an ide shell instance and have a unique id.
/// There is no guarantee that the id of an IconType will be the same across different instances of the ide shell,
/// as they are allocated first come first served, but they will be allocated sequentially within a given class, and
/// their object will be in the class's private _Icons list at the index position <code>Id - _Seed + 1</code>.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class AbstractIconsCollection
{
	protected static object _LockObject = new();

	private readonly Dictionary<IBIconType, Lazy<BitmapImage>> _IconsRef;

	public abstract IList<IBIconType> Icons { get; }

	protected AbstractIconsCollection()
	{
		IList<IBIconType> iconResourceList = new List<IBIconType>();

		LoadIconResourceList(iconResourceList);


		UiTracer.TraceSource.AssertTraceEvent(
			iconResourceList != null && iconResourceList.Count > 0, TraceEventType.Error,
			EnUiTraceId.UiInfra, "Must have a valid list of image names.");

		if (iconResourceList == null)
			return;

		_IconsRef = new Dictionary<IBIconType, Lazy<BitmapImage>>();

		foreach (IBIconType item in iconResourceList)
		{
			Uri uri = new Uri(item.ToString(), UriKind.Absolute);
			Lazy<BitmapImage> value = new Lazy<BitmapImage>(() => new BitmapImage(uri));
			_IconsRef.Add(item, value);
		}
	}



	public BitmapImage GetImage(IBIconType icon)
	{
		return _IconsRef[icon].Value;
	}

	public virtual void LoadIconResourceList(IList<IBIconType> iconResources)
	{
	}
}
