
using System.Collections.Generic;
using System.Net;

namespace BlackbirdSql.Core.Ctl;


// =========================================================================================================
//
//											PropertySet Class
//
/// <summary>
/// The static constants base class for use by connection classes derived from AbstractPropertyAgent.
/// PropertySet's are utilized by an <see cref="IBPropertyAgent"/> class when it is the initiating instance
/// (Initiator), ie. the final descendent child class instance in the instance lineage. A class instance can
/// simultaneousy be an Initiator and a sub-class instance.
/// Each class in the IBPropertyAgent hierarchy must provide for it's own static private cumulative
/// PropertySet through the static <see cref="AbstractPropertyAgent.CreateAndPopulatePropertySet()"/>.
/// Descendents are provided a replica of their parent's cumulative static private property set, to which
/// they can then add their own custom properties.
/// Descendent <see cref="AbstractPropertyAgent"/> classes may or may not have a separate PropertySet class
/// dependent on the number of additional descriptors.
/// </summary>
// =========================================================================================================
public abstract class PropertySet
{
	#region Constants


	private static bool _IsLocalIPAddress = false;
	private static IPAddress[] _LocalIPs = null;


	#endregion





	// =========================================================================================================
	#region Static Methods
	// =========================================================================================================



	public static bool IsLocalIpAddress(string host)
	{
		if (_LocalIPs != null)
			return _IsLocalIPAddress;


		try
		{ // get host IP addresses
			IPAddress[] hostIPs = Dns.GetHostAddresses(host);
			// get local IP addresses
			_LocalIPs = Dns.GetHostAddresses(Dns.GetHostName());

			// test if any host IP equals to any local IP or to localhost
			foreach (IPAddress hostIP in hostIPs)
			{
				// is localhost
				if (IPAddress.IsLoopback(hostIP))
				{
					_IsLocalIPAddress = true;
					return true;
				}
				// is local address
				foreach (IPAddress localIP in _LocalIPs)
				{
					if (hostIP.Equals(localIP))
					{
						_IsLocalIPAddress = true;
						return true;
					}
				}
			}
		}
		catch { }

		return false;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a key and value to a KeyValuePair<string, string>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static KeyValuePair<string, string> StringPair(string key, string value)
	{
		return new KeyValuePair<string, string>(key, value);
	}



	#endregion Static Methods


}
