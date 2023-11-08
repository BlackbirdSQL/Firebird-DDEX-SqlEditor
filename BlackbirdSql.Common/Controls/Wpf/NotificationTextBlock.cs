#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace BlackbirdSql.Common.Controls.Wpf;


public class NotificationTextBlock : TextBlock
{
	public class NotificationTextBlockAutomationPeer : TextBlockAutomationPeer
	{
		private readonly NotificationTextBlock _notificationTextBlock;

		private IRawElementProviderSimple _provider;

		public NotificationTextBlockAutomationPeer(NotificationTextBlock owner)
			: base(owner)
		{
			_notificationTextBlock = owner;
		}

		public void RaiseNotificationEvent(string notificationText, string notificationId)
		{
			if (!NotificationEventAvailable)
			{
				return;
			}

			if (_provider == null)
			{
				AutomationPeer automationPeer = FromElement(_notificationTextBlock);
				if (automationPeer != null)
				{
					_provider = ProviderFromPeer(automationPeer);
				}
			}

			if (_provider != null)
			{
				try
				{
					Native.UiaRaiseNotificationEvent(_provider,
						Native.EnNotificationKind.NotificationKind_ActionCompleted,
						Native.EnNotificationProcessing.NotificationProcessing_MostRecent,
						notificationText, notificationId);
				}
				catch (EntryPointNotFoundException)
				{
					NotificationEventAvailable = false;
				}
			}
		}
	}

	private NotificationTextBlockAutomationPeer _peer;

	private static bool _notificationEventAvailable = true;

	public static bool NotificationEventAvailable
	{
		get
		{
			return _notificationEventAvailable;
		}
		private set
		{
			_notificationEventAvailable = value;
		}
	}

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		_peer = new NotificationTextBlockAutomationPeer(this);
		return _peer;
	}

	public void RaiseNotificationEvent(string notificationText, string notificationId)
	{
		_peer?.RaiseNotificationEvent(notificationText, notificationId);
	}
}
