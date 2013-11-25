xspec -  A small BDD testing library (with syntax of NSpec/RSpec with Moq as mocking framework)
=============================================================================
The only requirement for this library is xUnit and the class under test 
using the base object of 'Context' for the runner to find the conditions 
to test:

From TDD to BDD

TDD											BDD
-----------------								-----------------
TestFixtureSetup						before_each
TestSetup								establish
TestMethod		                        it
TestCleanUp						    finally

Arrange-Assert-Act Style
-----------------------------------
Arrange  ->		before_each/establish
Act			->     act (optional)
Assert		->	     it

 or 

Arrange		->		before_each/establish
Act/Assert	->     it


Ex:

// the "subject under test" or SUT:
public class calculator 
{
	private int first_term; 
	private int second_term:

	public calculator()
	 : (0, 0)
	 {}

	public calculator(int first_term, int second_term)
	{
		this.first_term = first_term; 
		this.second_term = second_term;
	}

	public int add()
	{
		return this.first_term + this.second_term;
	}

	public int add(int first_term, int second_term)
	{
		this.first_term = first_term; 
		this.second_term = second_term;
		this.add();
	}
}

// the "specification" (or set of tests and scenarios) to define/examine the subject under test:

public class calculator_specifications : specification  // -> this will be the only class that you will need
{
	private calculator sut;  // we will have a member variable be the subject under test:

	// this is a demonstration of an "example" or what core scenario/action we want the sut to accomplish:
	private void when_adding_two_numbers()
	{
		int add_result;

		// before the test is conducted, construct the sut:
		before_each = () => {
			this.sut = new calculator();
		};

		// just before the test conditions are inspected, set up your pre-conditions:
		establish = () => {
			this.add_result = this.sut.add(1, 2);
		};

		// now, test the conditions that you expect from the sut:
		it["should return a positive result"] = () => {
			this.add_result.should_be(3);
		}

	}

}

After running the specification, the results look like this:

calculator specifications
	when adding two numbers    --> example
		it should return a positive result  --> test condition

1 example(s), 1 conditions, 1 passed, 0 failed, 0 skipped, (0.5 seconds) (xspec)


Mocking:
=========================================
The xspec library can be used for testing the subject under test with its collaborators. The Moq 
library is built into the core of xspec for this specific purpose:

Ex: Sending a message for successful login

// here we use the "observes" variant of the specification to 
// note that the HomeController (i.e. SUT) is being "observed", 
// meaning that it will have collaborators that it will interact with 
// to accomplish a specific task or tasks:
public homecontroller_specifications : observes<HomeController>
{
	private Mock<IMessageSender> message_sender; 

	public homecontroller_specifications()
	{
		// tell the SUT that a constructor dependency will be injected (needs to be top-level):
		message_sender = depends_on<IMessageSender>(); 
	}

	private void when_a_user_logs_in()
	{
		string username; 
		string password; 

		context["with a valid user name and password"] = () =>{

			// setup the sub-scenario:
			establish = () => {
				username = "jdoe"; 
				password = "password";

				// need to setup the mock (we do not care about the inputs yet):
				message_sender.Setup(s =>s.SendMessage(string.Empty));

				};

				// separate the action on the sut from the testing condition:
				// (always executed before "it" conditions)
				act = () => {
					this.sut.Login(username, password);  // sut = instance of HomeController :)
				};

				// testing condition:
				it["should send the message that the user has been logged in"] = () =>
				{
					message_sender.Verify(v =>v.SendMessage(It.IsAny<string>())), Times.AtLeastOnce);
				};
			};
				
		};
	}
}

public class HomeController : Controller
{
	IMessageSender messageSender; 

	public HomeController(IMessageSender messageSender) 
	{
		this.messageSender = messageSender;
	}

	public void Login(string username, string password)
	{
		// just send something for the test to pass:
		this.messageSender.SendMessage(string.Empty); 
	}
}

public interface IMessageSender
{
	void SendMessage(string message);
}

After running the specification, the results look like this:

homecontroller specifications
	when a user logs in   --> example
		with a valid user name and password --> context
			it should send the message that the user has been logged in  --> test condition

1 example(s), 1 conditions, 1 passed, 0 failed, 0 skipped, (0.5 seconds) (xspec)




How to configure xSpec with xSpec Watcher
=========================================
1. Create a reference to the testing library xspec.dll as you would normally do for testing
2. Inside of your /tools directory on the root of your project, place the xspec.watcher.exe here 