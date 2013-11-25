#-------------------------------------------------------------------------------  
# basic build file powershell script for psake for watch
#-------------------------------------------------------------------------------  
properties { 
  $base_dir  = resolve-path .
  $lib_dir = "$base_dir\lib"
  $source_dir = "$base_dir\src"
  $tests_dir = "$base_dir\tests"
  $build_dir = "$base_dir\build" 
  $buildartifacts_dir = "$build_dir\" 
  $sln_file = "$base_dir\xspec.sln" 
  $test_lib = "xspec.tests.dll"
  $version = "1.1.0.0"
  $tools_dir = "$base_dir\tools"
  $release_dir = "$base_dir\release"
} 

#-- external tools here:
include .\watch_ext.ps1

#-------------------------------------------------------------------------------  
# entry task to start the build script
#-------------------------------------------------------------------------------  
task default -depends release

#-------------------------------------------------------------------------------  
# clean the "build" directory and make ready for the build actions
#-------------------------------------------------------------------------------  
task clean { 

  Kill-Notification-Agents
    
  remove-item -force -recurse $buildartifacts_dir -ErrorAction SilentlyContinue
  remove-item -force -recurse $release_dir -ErrorAction SilentlyContinue 
} 

#-------------------------------------------------------------------------------  
# initializes all of the directories and other objects in preparation for the 
# build process
#-------------------------------------------------------------------------------  
task init -depends clean { 
	new-item $release_dir -itemType directory 
	new-item $buildartifacts_dir -itemType directory 
} 

#-------------------------------------------------------------------------------  
# compile the solution or project via msbuild
#-------------------------------------------------------------------------------  
task compile -depends init { 
  copy-item "$lib_dir\*" $build_dir
  copy-item "$tools_dir\*" $build_dir
  $results = exec { msbuild /p:OutDir="$buildartifacts_dir" "$sln_file" /p:Configuration=Release }
  Parse-Build $results
} 

#-------------------------------------------------------------------------------  
# task to run tests for currently modified test library:
#-------------------------------------------------------------------------------  
task test -depends compile {
  $old = pwd
  cd $build_dir
  
  #-- grab all test libraries and run the tests:
  $test_files = [System.IO.Directory]::GetFiles($build_dir, "*test.dll")
  
  foreach ($test_file in $test_files)
  {
    # using .NET 4.0 runner for xspec [or your runner of choice]
    $xspec = "$$tools_dir\xspec.console.exe"
    $results = .$xspec "/library $buildartifacts_dir$project"
    
    if($lastexitcode -ne 0)
    {
        Parse-Test $results
    }
  }
 }
 
#-------------------------------------------------------------------------------  
# task to move successfully tested code to "release" directory for other tasks
#-------------------------------------------------------------------------------   
task release -depends compile, test {
	$old = pwd
    cd $build_dir
	
	# move items to the release directory (package task):
	copy-item "$build_dir\*.*" $release_dir
	 
	cd $old		
}



