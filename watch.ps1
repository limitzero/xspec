#---------------------------------------------
# Command-line options:
# watch.ps1 [solution file to watch]
#---------------------------------------------
param(
    [string]$desired_solution_to_watch = ""
)
#---------------------------------------------
# Customizable Values for Watch, Build, & Test
#---------------------------------------------
$solution ="c:\repositories\xspec\xspec.sln"
$code_file_extensions_to_trigger_build = "*.cs, *.asmx, *.txt" 
$project_file_extension = ".csproj" # for C# projects, changes to .vbproj for VB
$test_engine = ""
#---------------------------------------------
#Script Globals
#---------------------------------------------
$local_directory = resolve-path .
$global:notification_executable = "c:\repositories\xspec\build\xspec.notifier.exe"
$global:dev_engine = "c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe"
$build_engine = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild"
$global:test_engine = "C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\mstest"
$is_Build_test_session_active = $false;
#---------------------------------------------
# Configure Watcher....
#---------------------------------------------
$watcher = New-Object System.IO.FileSystemWatcher
#---------------------------------------------
# Private Functions
#---------------------------------------------
function initialize-watch-build-test-script()
{   
    $location = get-location
    
    if ([System.String]::IsNullOrEmpty($desired_solution_to_watch) -eq $false)
    {
        $solution = $desired_solution_to_watch
    }
    elseif ($solution -eq "")
    {
        # find the first solution file in the local directory to watch:
        if([System.IO.File]::GetFiles($location,"*.sln").Length -eq 0) 
        {
            throw "No solution file found in the following directory " + $location + " to watch."
        }
        else
        {
            $solution = [System.IO.File]::GetFiles($local_directory,"*.sln")[0];
        }
    }
    
    # all is good so far, start watching the directory that contains the solution:
    Configure-And-Watch;
}
#-----------------------------------------------
# Configure-And-Watch: This will start the session
# for watching all changes to a solution and 
# implementing the build and test cycle.
#-----------------------------------------------
function Configure-And-Watch()
{  
    # Unregister-Event -SourceIdentifier FileCreated 
    # Register-ObjectEvent $watcher Changed -SourceIdentifier FileCreated -Action {
    #    Build-Test $Event.SourceEventArgs.FullPath 
    #}
    
    # point the file watcher to the root directory of the solution
    $root_dir = Get-Root-Directory;
    $watcher.Path = $root_dir;
    $watcher.IncludeSubdirectories = $true
    $watcher.EnableRaisingEvents = $true
    $watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite -bor [System.IO.NotifyFilters]::FileName

    Write-Host "watching solution directory " $root_dir.trim() ".Press 'q' to quit watching";
    
    while($true)
    {
        # politely exit...if we need to:
       $exit = Get-Exit-Condition
   
       if($exit -eq "q" -or $exit -eq "Q")
       {
         break;
       }
       
       $result = $watcher.WaitForChanged([System.IO.WatcherChangeTypes]::Changed -bor [System.IO.WatcherChangeTypes]::Renamed -bOr [System.IO.WatcherChangeTypes]::Created, 1000);
	   if($result.TimedOut -or $is_build_test_session_active -eq $true){
		continue;
	   }
       
       # start the build and test cycle:
       $file = $result.Name; 
       
       Build-Test `
        -current_changed_file $file
        
        $is_build_test_session_active = $false
    }
}
#-----------------------------------------------
# Build-Test: This will start the build and test
# cycle for the changed file in the solution.
#-----------------------------------------------
function Build-Test
{
    param(
        [string]$current_changed_file=""
    )
    
    $current_changed_file_extension = [System.String]::Concat("*",[System.IO.Path]::GetExtension($current_changed_file));
    $compilable_extentions = $code_file_extensions_to_trigger_build.split(",");
    
    if($current_changed_file_extension -ne "" -or $compilable_extentions.Length -gt 0) 
    {
       foreach($extension in $compilable_extentions)
       { 
         if($extension.trim() -eq $current_changed_file_extension.trim())
         {
            # build the solution and run the tests:  
            if(Build-Solution -eq $true)
            {
                Run-Tests
            }
            
            $is_build_test_session_active = $false;
            break;
         }
       }
    }
}
#-----------------------------------------------
# Build-Solution: This will build the solution
# for running the test suite.
#-----------------------------------------------
function Build-Solution()
{
    # send marker to watcher that we are currently
    # building and ignore any change file notifications:
    $is_build_test_session_active = $true;

    Write-Host "building " $solution "..."
   
    # create new build directory:
    $build_dir = Get-Build-Directory;
    remove-item -force -recurse $build_dir -ErrorAction SilentlyContinue
    new-item  $build_dir -itemType directory 
    
    # copy over any dependencies to build directory:
    $lib_dir = Get-Library-Directory
    $tools_dir = Get-Tools-Directory
    copy-item "$lib_dir\*" $build_dir
    copy-item "$tools_dir\*" $build_dir
  
    # build solution and inspect results:
    $results = &$build_engine /p:OutDir=$build_dir\ /v:minimal $solution
    $parsed_results = @();
    $parsed_results = $parsed_results + ($results -like "*Build succeeded*");
    $success = $parsed_results -contains "Build succeeded."; 
    
    Write-Host $results
    
    return $success;
}
#-----------------------------------------------
# Run-Tests: This will run all of the test 
# projects against the indicated test runner.
#-----------------------------------------------
function Run-Tests()
{
  # get the listing of test libraries:
  $test_projects = Get-Test-Projects;
    
  # run the tests from the build directory:   
  $build_dir = Get-Build-Directory;
  $old = pwd
  cd $build_dir
  
  # using the xspec console for running the tests:
  $xspec = "$build_dir\xspec.console.exe" 
  
  foreach($test in $test_projects)
  {
     .$xspec "$test"
  }
  
  cd $old
}
#-----------------------------------------------
# Get-Test-Projects: This will open the solution 
# defined above and read out all of the test 
# projects in the solution (must have "test" 
# in the project name).
#-----------------------------------------------
function Get-Test-Projects
{   
    $build_dir = Get-Build-Directory; 
    $test_libraries = [System.IO.Directory]::GetFiles($build_dir,"*test*.dll");
    return $test_libraries;   
}
#-----------------------------------------------
# Utility Functions
#-----------------------------------------------
function Get-Exit-Condition
{
    $input = [Console]::ReadLine();
    return $input;
}
#-----------------------------------------------
# Get-Root-Directory: This will retrieve the 
# root directory that the solution file lives in
#-----------------------------------------------
function Get-Root-Directory
{
   return [System.IO.Path]::GetDirectoryName($solution);
}
#-----------------------------------------------
# Get-Build-Directory: This will retrieve the 
# common build directory for the solution build
# products
#-----------------------------------------------
function Get-Build-Directory
{
    $root_dir = Get-Root-Directory
    return $root_dir + "\build";
}
#-----------------------------------------------
# Get-Build-Directory: This will retrieve the 
# directory where all of the external libraries
# needed by the solution are located
#-----------------------------------------------
function Get-Library-Directory
{
    $root_dir = Get-Root-Directory
    return $root_dir + "\lib";
}
#-----------------------------------------------
# Get-Tools-Directory: This will retrieve the 
# directory where the external tools needed 
# by the solution are located
#-----------------------------------------------
function Get-Tools-Directory
{
    $root_dir = Get-Root-Directory
    return $root_dir + "\tools";
}
function Get-Test-ProjectsEx
{
    $test_projects = @();
    $root_dir = [System.IO.Path]::GetDirectoryName($solution);
	$content = [System.IO.File]::ReadAllText($solution); 
    $path = [System.IO.Path]::GetDirectoryName($solution);
    $path += "\";
    
    $lines = $content.split([System.Environment]::NewLine);
    
    foreach($line in $lines)
	{
        $projectFileFound = $line.StartsWith("Project")
        $unitTestProjectFound = $line.ToLower().Contains("test")
        
		if($projectFileFound -ne $false)
        {
            if($unitTestProjectFound -ne $false)
            {
            	$project = $line.split(",")[1]
                $unit_test = $path += $project.trim() -replace """", "";
                
			    Write-Host "Found unit test " $unit_test
                
                # create the directory to the unit test from the 
                # current unit test project location in the solution:
                $current_project_file_parts = $unit_test.split("\");
                [System.Array]::Reverse($current_project_file_parts);
                
                $current_unit_test_file = $current_project_file_parts[0] -replace """", "";
                
                $current_unit_test_location = $project -replace $current_unit_test_file,"bin\debug" 
                
                $current_unit_test_library = $current_unit_test_file -replace $project_file_extension,".dll"
                
                $excuting_unit_test = $path.trim() + "\" + $current_unit_test_library.trim();
                    
                $excuting_unit_test = $excuting_unit_test -replace """", "";
                
                # add the current test to the array for execution:                    
                $test_projects = $test_projects + $excuting_unit_test;
                
            }		
		}
	}
    
    return $test_projects;
}
#---------------------------------------------
# Start script...
#---------------------------------------------
initialize-watch-build-test-script; 







