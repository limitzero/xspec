#-------------------------------------------------------------------------------  
# basic build file Powershell script for psake
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

#-------------------------------------------------------------------------------  
# entry task to start the build script
#-------------------------------------------------------------------------------  
task default -depends release

#-------------------------------------------------------------------------------  
# clean the "build" directory and make ready for the build actions
#-------------------------------------------------------------------------------  
task clean { 
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

task compile -depends init { 
  copy-item "$lib_dir\*" $build_dir
  copy-item "$tools_dir\*" $build_dir
  exec { msbuild /p:OutDir="$buildartifacts_dir" "$sln_file" /p:Configuration=Release }
} 

task test -depends compile {
  $old = pwd
  cd $build_dir
  
  # using .NET 4.0 runner for xunit:
  $xunit = "$tools_dir\xUnit\xunit.console.clr4.x86.exe"
  .$xunit "$buildartifacts_dir$test_lib"
  cd $old		
}

task release -depends compile, test {
	$old = pwd
    cd $build_dir
	
	Remove-Item xspec.partial.dll -ErrorAction SilentlyContinue 
	Rename-Item $build_dir\xspec.dll xspec.partial.dll
	
	& $tools_dir\ILMerge.exe xspec.partial.dll `
		Moq.dll `
		/out:xspec.dll `
		/t:library `
	
	if ($lastExitCode -ne 0) {
        throw "Error: Failed to merge assemblies xspec library!"
    }
	Remove-Item xspec.partial.dll -ErrorAction SilentlyContinue
	
	Remove-Item xspec.console.partial.exe -ErrorAction SilentlyContinue 
	Rename-Item $build_dir\xspec.console.exe xspec.console.partial.exe
	
	& $tools_dir\ILMerge.exe xspec.console.partial.exe `
		xspec.runner.utility.dll `
		xspec.dll `
		/out:xspec.console.exe `
		/t:exe `
	
	if ($lastExitCode -ne 0) {
        throw "Error: Failed to merge assemblies into xspec.console executable!"
    }
	Remove-Item xspec.console.partial.exe -ErrorAction SilentlyContinue
	
	# -- merge xspec.watcher.exe --
	#Remove-Item xspec.watcher.partial.exe -ErrorAction SilentlyContinue 
	#Rename-Item $build_dir\xspec.watcher.exe xspec.watcher.partial.exe
	
	#& $tools_dir\ILMerge.exe xspec.watcher.partial.exe `
	#	xspec.runner.utility.dll `
	#	xspec.dll `
	#	/out:xspec.watcher.exe `
	#	/t:exe `
	
	#if ($lastExitCode -ne 0) {
    #    throw "Error: Failed to merge assemblies into xspec.watcher executable!"
    #}
	
	#Remove-Item xspec.watcher.partial.exe -ErrorAction SilentlyContinue
	
	# move items to the release directory (package task):
	copy-item "$build_dir\xspec.notifier.exe" $release_dir
	copy-item "$build_dir\xspec.watcher.exe.config" $release_dir
	copy-item "$build_dir\xspec.dll" $release_dir
	copy-item "$build_dir\xspec.runner.utility.dll" $release_dir
	copy-item "$build_dir\xspec.watcher.exe" $release_dir
	copy-item "$build_dir\xspec.watcher.exe.config" $release_dir
	copy-item "$build_dir\xspec.console.exe" $release_dir
	 
	cd $old		
}



