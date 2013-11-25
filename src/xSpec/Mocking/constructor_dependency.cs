namespace xSpec.Mocking
{
	internal class constructor_dependency
	{
		public object Dependency { get; private set; }

		public constructor_dependency(object dependency)
		{
			Dependency = dependency;
		}
	}
}