using System;
using Moq;
using xSpec.Mocking;

namespace xspec.sample.tests.email
{
	// example using the auto-mocking features of xspec (using Moq):
	public class online_email_specifications : observes<HomeController>
	{
		private readonly Mock<IAuthenticationService> authenticationService;

		public online_email_specifications()
		{
			// create the dependency for the object being observed (must be top-level):
			authenticationService = depends_on<IAuthenticationService>();
		}

		private void when_providing_a_username_and_password_to_login()
		{
			string userName = string.Empty;
			string password = string.Empty;

			context["and the user name and password is valid"] = () =>
			{
				establish = () =>
								{
									userName = "jdoe";
									password = "password";

									// setup the mock for the call:
									authenticationService.Setup(s => s.Authenticate(userName, password));
								};

				// conduct the action of a login:
				act = () =>
						{
							sut.Login(userName, password);
						};

				it["will authenticate the user using the user name and password"] = () =>
				{
					authenticationService
						.Verify(v => v.Authenticate(It.IsAny<string>(), It.IsAny<string>()),
						Times.AtLeastOnce());
				};

			};

		}
	}

	public class HomeController
	{
		private readonly IAuthenticationService authenticationService;

		public HomeController(IAuthenticationService authenticationService)
		{
			this.authenticationService = authenticationService;
		}

		public void Login(string username, string password)
		{
			this.authenticationService.Authenticate(username, password);
		}
	}

	public interface IAuthenticationService
	{
		void Authenticate(string username, string password);
	}
}