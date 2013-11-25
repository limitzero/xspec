using xspec.runner.utility.Notification.Messages;

namespace xspec.runner.utility.Notification
{
	public interface INotificationService
	{
		void Notify(NotificationMessage level);
	}
}