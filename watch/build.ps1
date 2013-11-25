#--- build system for current solution and/or project
param (
	[string]$rootpath = "c:\repositories\xspec",
    [string]$solution = "xspec.sln"
 )

include .\configuration.ps1
 
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
task default -depends compile

#-------------------------------------------------------------------------------  
# clean the "build" directory and make ready for the build actions
#-------------------------------------------------------------------------------  
task clean { 
  remove-item -force -recurse $buildartifacts_dir -ErrorAction SilentlyContinue 
} 

#-------------------------------------------------------------------------------  
# initializes all of the directories and other objects in preparation for the 
# build process
#-------------------------------------------------------------------------------  
task init -depends clean { 
	new-item $buildartifacts_dir -itemType directory 
} 

#-------------------------------------------------------------------------------  
# compile the solution or project via msbuild
#-------------------------------------------------------------------------------  
task compile -depends init { 
  copy-item "$lib_dir\*" $build_dir
  copy-item "$tools_dir\*" $build_dir
  exec { msbuild /p:OutDir="$buildartifacts_dir" "$sln_file" /p:Configuration=Release }
} 

