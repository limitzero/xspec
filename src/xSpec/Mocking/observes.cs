using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Moq;

namespace xSpec.Mocking
{
	public interface IObserves
	{
		void create_subject();
		void clean_up();
	}

	/// <summary>
	/// Base class that "observes" or sets-up the basic infrastructure for the subject 
	/// under test for interaction with its collaborators (i.e. dependencies). Using Moq 
	/// as the underlying "mocking" container.
	/// </summary>
	/// <typeparam name="TSubjectUnderTest">Current class under test with its collaborators</typeparam>
	public abstract class observes<TSubjectUnderTest> : specification, IObserves, IDisposable
		where TSubjectUnderTest : class
	{
		private static bool enable_auto_verifications = true;
		private static readonly object dependencies_lock = new object();
		private static IDictionary<Type, object> sut_mocked_dependencies;
		private static List<constructor_dependency> sut_constructor_dependencies;

		/// <summary>
		/// Gets the indicator as to whether or not the verifications on the mocks 
		/// will be inspected always on test condition execution.
		/// </summary>
		public bool use_auto_verifications { get { return enable_auto_verifications; } }

		protected observes()
		{
			if (sut_mocked_dependencies == null)
			{
				sut_mocked_dependencies = new Dictionary<Type, object>();
			}

			if (sut_constructor_dependencies == null)
			{
				sut_constructor_dependencies = new List<constructor_dependency>();
			}
		}

		/// <summary>
		/// The class or "subject under test" that may or may not have dependencies for 
		/// completion of a test scenario.
		/// </summary>
		protected static TSubjectUnderTest sut { get; private set; }

		/// <summary>
		/// This will enable or disable the context to use the default verification mechanism 
		/// for all mocked instances. The default behavior is to call "VerifyAll()" for each 
		/// mocked instance that is needed by the subject under test. Turning this off (enabled : false) will 
		/// allow for individual mock verifications.
		/// </summary>
		/// <param name="enable">(true = use VerifyAll() on each mock instance (default), false = user-provided verifications on each mock)</param>
		protected static void use_auto_verfications(bool enable)
		{
			enable_auto_verifications = enable;
		}

		~observes()
		{
			Dispose(true);
		}

		/// <summary>
		/// This returns an  instance of an object that will not be used as a dependency for the 
		/// system under test.
		/// of the sut.
		/// </summary>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		protected static TInstance instance_of_a<TInstance>() where TInstance : class
		{
			var instance = new Mock<TInstance>();
			return instance.Object;
		}

		/// <summary>
		/// indicates that the system under test has an explict dependency (via the constructor) that will 
		/// be generated as a mock for completion of scenario testing.
		/// </summary>
		/// <typeparam name="TDependency"></typeparam>
		/// <returns></returns>
		protected static Mock<TDependency> depends_on<TDependency>()
			where TDependency : class
		{
			lock (dependencies_lock)
			{
				var dependency = new moq_dependency<TDependency>();

				if (sut_mocked_dependencies.ContainsKey(typeof(TDependency)) == false)
				{
					sut_mocked_dependencies.Add(typeof(TDependency), dependency);
				}

				return dependency.DependencyMock;
			}
		}

		/// <summary>
		/// indicates that the system under test will not use any mocked instances for 
		/// constructor, they will be supplied by the scenario in question (the order of the 
		/// dependenices must match the order of parameters on the constructor in order 
		/// for the instance to be created properly).
		/// </summary>
		/// <param name="dependencies"></param>
		protected static void has_constructor_dependencies_of(params object[] dependencies)
		{
			lock (dependencies_lock)
			{
				foreach (var constructorDependency in dependencies)
				{
					sut_constructor_dependencies.Add(new constructor_dependency(constructorDependency));
				}
			}
		}

		/// <summary>
		/// Creates a stub or a place-holder to a call from within a dependency.
		/// </summary>
		/// <typeparam name="TStub"></typeparam> 
		/// <returns></returns>
		protected static Mock<TStub> the_stub<TStub>()
			where TStub : class
		{
			return new Mock<TStub>();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private static void Dispose(bool disposing)
		{
			if (disposing == true)
			{
				if (sut_mocked_dependencies != null)
				{
					sut_mocked_dependencies.Clear();
				}
				sut_mocked_dependencies = null;

				if (sut_constructor_dependencies != null)
				{
					sut_constructor_dependencies.Clear();
				}
				sut_constructor_dependencies = null;
			}
		}

		/// <summary>
		/// This will create an instance of the current subject under test (called by context).
		/// </summary>
		public void create_subject()
		{
			TSubjectUnderTest subject = default(TSubjectUnderTest);

			if (sut_mocked_dependencies.Count > 0)
			{
				ConstructorInfo constructor = find_greediest_constructor_for_subject_under_test();

				if (constructor == null)
				{
					throw new InvalidOperationException(string.Format("No constructor on '{0}' has a constructor with parameter(s) of type '{1}'",
						typeof(TSubjectUnderTest).Name,
						string.Join(",",
							sut_mocked_dependencies.SelectMany(t => t.Key.Name))
						));
				}

				subject = create_from_mocked_dependencies(constructor);
			}
			else if (sut_constructor_dependencies.Count > 0)
			{
				ConstructorInfo constructor = find_greediest_constructor_for_subject_under_test();

				if (constructor == null)
				{
					throw new InvalidOperationException(string.Format("No constructor on '{0}' has a constructor with parameter(s) of  '{1}'",
						typeof(TSubjectUnderTest).Name,
						string.Join(",",
							sut_constructor_dependencies.SelectMany(t => t.Dependency.GetType().Name))
						));
				}

				subject = create_from_non_mocked_dependencies(constructor);
			}
			else
			{
				subject = typeof(TSubjectUnderTest).Assembly.CreateInstance(typeof(TSubjectUnderTest).FullName) as TSubjectUnderTest;
			}

			sut = subject;
		}

		/// <summary>
		/// This will clean-up all internal dependencies after the test run (called by context)
		/// </summary>
		public void clean_up()
		{
			if (enable_auto_verifications)
				this.verify_all();

			if (sut_mocked_dependencies != null)
			{
				sut_mocked_dependencies.Clear();
			}

			if (sut_constructor_dependencies != null)
			{
				sut_constructor_dependencies.Clear();
			}
		}

		private Mock<T> resolve<T>() where T : class
		{
			var mock = resolve(typeof(T));
			return mock as Mock<T>;
		}

		private static object resolve(Type type)
		{
			lock (dependencies_lock)
			{
				var mock = sut_mocked_dependencies[type];
				return mock;
			}
		}

		private void verify_all()
		{
			foreach (var sut_mocked_dependency in sut_mocked_dependencies)
			{
				var mock = sut_mocked_dependency.Value as Mock;

				if (mock != null)
				{
					mock.VerifyAll();
				}
			}
		}

		private static TSubjectUnderTest create_from_mocked_dependencies(ConstructorInfo constructor)
		{
			var dependencies = new List<object>();

			foreach (var parameter in constructor.GetParameters())
			{
				foreach (var kvp in sut_mocked_dependencies)
				{
					var mock = resolve(kvp.Key);

					if (mock == null)
						throw new InvalidOperationException(string.Format("There was not a mock supplied for the type '{0}' for '{1}'",
							kvp.Key.Name,
							typeof(TSubjectUnderTest).Name));

					var mock_underlying_object = mock.GetType().GetProperty("DependencyInstance").GetValue(mock, null);

					if (kvp.Key == parameter.ParameterType)
					{
						dependencies.Add(mock_underlying_object);
					}
				}
			}

			// create the subject under test with the dependencies supplied to the constructor:
			TSubjectUnderTest subject_under_test = constructor.Invoke(BindingFlags.CreateInstance,
				null, dependencies.ToArray(), CultureInfo.CurrentCulture) as TSubjectUnderTest;

			return subject_under_test;
		}

		private static TSubjectUnderTest create_from_non_mocked_dependencies(ConstructorInfo constructor)
		{
			if (constructor.GetParameters().Length != sut_constructor_dependencies.Count)
				throw new InvalidOperationException(string.Format("Suject under test constructor requires {0} parameters but was supplied {1}",
					constructor.GetParameters().Length, sut_constructor_dependencies.Count));

			var dependencies = sut_constructor_dependencies.Select(d => d.Dependency).ToList();

			return constructor.Invoke(BindingFlags.CreateInstance, null, dependencies.ToArray(), CultureInfo.CurrentCulture) as TSubjectUnderTest;
		}

		private static ConstructorInfo find_greediest_constructor_for_subject_under_test()
		{
			int constructorParams = 1;
			ConstructorInfo theConstructor = null;
			ConstructorInfo[] constructors =
				typeof(TSubjectUnderTest).GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

			foreach (var constructorInfo in constructors)
			{
				if (constructorInfo.GetParameters().Length >= constructorParams)
				{
					constructorParams = constructorInfo.GetParameters().Length;
					theConstructor = constructorInfo;
				}
			}

			return theConstructor;
		}
	}
}