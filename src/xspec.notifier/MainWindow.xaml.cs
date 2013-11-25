using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using WPFGrowlNotification.Modes;
using WPFGrowlNotification.Runner;
using WPFGrowlNotification.Runner.CommandLine;

namespace WPFGrowlNotification
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private const double topOffset = 5;
		private const double leftOffset = 420;
		readonly GrowlNotifiactions growlNotifications = new GrowlNotifiactions();
		private static readonly IDictionary<Actions, IRunMode> runmodes = new Dictionary<Actions, IRunMode>();

		public MainWindow()
		{
			InitializeComponent();
			this.Visibility = System.Windows.Visibility.Hidden;
			growlNotifications.Top = SystemParameters.WorkArea.Top + topOffset;
			growlNotifications.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - leftOffset;

			InitializeRunModes();
			var args = System.Environment.GetCommandLineArgs();
			var notification = Run(args);

			if (notification != null)
			{
				growlNotifications.AddNotification(notification);
			}
		}

		private static void InitializeRunModes()
		{
			runmodes.Add(Actions.Notify, new NotifyMode());
		}

		private static Notification Run(string[] args)
		{
			Notification notification = null;
			var executingOptions = new ExecutingOptions();

			if (Parser.ParseArguments(args, executingOptions) == false && executingOptions.IsParsed == false)
			{
				Console.WriteLine("Invalid arguments:");
				Console.WriteLine("\t{0}", string.Join(" ", args));
				Console.WriteLine();
				Console.WriteLine(Parser.ArgumentsUsage(typeof(ExecutingOptions)));
			}

			try
			{
				notification = RunInMode(executingOptions);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}

			return notification;
		}

		private static Notification RunInMode(ExecutingOptions executingOptions)
		{
			return runmodes[executingOptions.Action].Execute(executingOptions);
		}

		private void ButtonClick1(object sender, RoutedEventArgs e)
		{
			growlNotifications.AddNotification(new Notification { Title = "Mesage #1", ImageUrl = "pack://application:,,,/Resources/notification-icon.png", Message = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." });
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			growlNotifications.AddNotification(new Notification { Title = "Mesage #2", ImageUrl = "pack://application:,,,/Resources/microsoft-windows-8-logo.png", Message = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." });
		}

		private void ButtonClick2(object sender, RoutedEventArgs e)
		{
			growlNotifications.AddNotification(new Notification { Title = "Mesage #3", ImageUrl = "pack://application:,,,/Resources/facebook-button.png", Message = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." });
		}

		private void ButtonClick3(object sender, RoutedEventArgs e)
		{
			growlNotifications.AddNotification(new Notification { Title = "Mesage #4", ImageUrl = "pack://application:,,,/Resources/Radiation_warning_symbol.png", Message = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." });
		}

		protected override void OnClosed(System.EventArgs e)
		{
			growlNotifications.Close();
			base.OnClosed(e);
		}

		private void WindowLoaded1(object sender, RoutedEventArgs e)
		{
			//this will make minimize restore of notifications too
			//growlNotifications.Owner = GetWindow(this);
		}
	}
}
