using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace xSpec.watcher.Pipeline
{
	public class TestProjectFromSolutionExtractor
	{	
		public IEnumerable<string> Extract(string solutionFilePath, string solutionFile)
		{
			var extracted_unit_test_libraries = new List<string>();
			var content = File.ReadAllText(Path.Combine(solutionFilePath, solutionFile));

			var lines = content.Split(new string[] { System.Environment.NewLine },
			                          StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				if (line.StartsWith("Project") && line.ToLower().Contains("test"))
				{
					// extract out unit test project name plus path:
					var project = line.Split(new string[] { "," },
					                         StringSplitOptions.RemoveEmptyEntries)[1].Replace(" ", "");

					// create the file name and location for the unit test found in the solution file:   
					var unit_test_location_in_solution_file = project.Substring(1, project.Length - 2);
					var unit_test_file_location_on_disk = Path.Combine(solutionFilePath, unit_test_location_in_solution_file);

					if (File.Exists(unit_test_file_location_on_disk))
					{
						FileInfo found_unit_test = new FileInfo(unit_test_file_location_on_disk);
						var extracted_file_extension = found_unit_test.Name.Replace(found_unit_test.Extension, ".dll");
						var extracted_file_name = string.Format(@"{0}\bin\Debug\{1}", found_unit_test.Directory, extracted_file_extension);
						extracted_unit_test_libraries.Add(extracted_file_name);
					}
				}
			}

			return extracted_unit_test_libraries.ToList().Distinct();
		}
	}
}