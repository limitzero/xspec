using xspec.watcher.Notifications.Messages;

namespace xspec.watcher.Notifications
{
	public interface INotificationService
	{
		void Notify(NotificationMessage level);
	}
}