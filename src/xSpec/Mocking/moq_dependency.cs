using System;
using Moq;

namespace xSpec.Mocking
{
	internal class moq_dependency<TDependency> where TDependency : class
	{
		public Type DependencyType
		{
			get { return typeof(TDependency); }
		}

		public Mock<TDependency> DependencyMock { get; private set; }

		public TDependency DependencyInstance
		{
			get { return DependencyMock.Object; }
		}

		public moq_dependency()
		{
			DependencyMock = new Mock<TDependency>();
		}
	}
}