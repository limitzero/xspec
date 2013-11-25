#--- test system for current solution and/or project
param (
	[string]$rootpath = "",
    [string]$project  = ""
 )

 
#-------------------------------------------------------------------------------  
# basic build file Powershell script for psake
#-------------------------------------------------------------------------------  
properties { 
  $base_dir  = $rootpath
  $lib_dir = "$base_dir\lib"
  $source_dir = "$base_dir\src"
  $tests_dir = "$base_dir\tests"
  $build_dir = "$base_dir\build" 
  $buildartifacts_dir = "$build_dir\" 
  $sln_file = "$base_dir\$solution" 
  $tools_dir = "$base_dir\tools"
  $release_dir = "$base_dir\release"
} 

#-------------------------------------------------------------------------------  
# entry task to start the build script
#-------------------------------------------------------------------------------  
task default -depends release

#-------------------------------------------------------------------------------  
# task to run tests for currently modified test library:
#-------------------------------------------------------------------------------  
task test -depends compile {
  $old = pwd
  cd $build_dir
  
  # using .NET 4.0 runner for xspec [or your runner :)]
  $xspec = "$build_dir\xspec.console.exe"
  $results = .$xspec "/library $buildartifacts_dir$project"
  
  
  
  
  
  cd $old		
}

#-------------------------------------------------------------------------------  
# task move successfully tested code to a release directory for staging
# or other post-success tasks:
#-------------------------------------------------------------------------------  
task release -depends test {
	$old = pwd
    cd $build_dir
	
	# move all items to the release directory (package task):
	copy-item "$build_dir\*.*" $release_dir
	
	cd $old		
}