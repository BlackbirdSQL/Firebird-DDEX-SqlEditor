
using System.Collections.Generic;
using System.Net;




namespace BlackbirdSql.Core;


// =========================================================================================================
//
//											PropertySet Class
//
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
