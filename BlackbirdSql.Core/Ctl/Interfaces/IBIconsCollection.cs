
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using BlackbirdSql.Sys;


namespace BlackbirdSql.Core.Ctl.Interfaces;

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
public interface IBIconsCollection
{
	IList<IBIconType> Icons { get; }

	BitmapImage GetImage(IBIconType icon);

	void LoadIconResourceList(IList<IBIconType> iconResources);
}
