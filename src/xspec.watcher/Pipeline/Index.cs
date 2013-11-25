using System;
using System.Collections.Generic;
using System.IO;

namespace xSpec.watcher.Pipeline
{
	public class Index
	{
		public bool IsTestProject { get; set; }
		public string ProjectFilePath { get; set; }
		public string ProjectLibraryPath { get; set; }
		public ICollection<string> ClassFiles { get; set; }

		public Index()
		{
			this.ClassFiles = new List<string>();
		}
	}
}