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
#========================================================================
# Global variables and configuration settings for watchr:
#========================================================================
$global:build_executable = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild"
$global:item_exclude =".dll", ".suo"
$global:watcher = New-Object IO.FileSystemWatcher $folder, $filter -Property @{IncludeSubdirectories = $true;NotifyFilter = [IO.NotifyFilters]'FileName, LastWrite'} 

Register-ObjectEvent $global:watcher Changed -SourceIdentifier FileChanged -Action {     
$name = $Event.SourceEventArgs.Name 
$changeType = $Event.SourceEventArgs.ChangeType 
$timeStamp = $Event.TimeGenerated 
Write-Host "The file '$name' was $changeType at $timeStamp" -fore white 
Out-File -FilePath c:\scripts\filechange\outlog.txt -Append -InputObject "The file '$name' was $changeType at $timeStamp"} 

#========================================================================
# This will run the tests that were found while searching the directory:
#========================================================================
function MSBuildRunner 
{
    params(
    [string]$project=""
    )
    
    #-- build the project with "safe" defaults:
    $arguements = [Systm.String]::Format("/t:clean,build /property:WarningLevel=0;OutputDir={0}\ /v:quiet", $build_dir)
    $results = & .$global:build_executable $arguments $project
    
    MSBuildOutPutParser
    -results $results
}

function MSBuildOutputParser
{
    params(
        [string]$results=""
    )
    
    $newline = [System.Environment]::NewLine;
    $content = "";
    
    foreach($line in $results)
    {
        if($line:Contains("Microsoft") || line.trim() -ne "" )
        {
            $content = [System.String]::Concat($content, $line , $newline)
        }
			
    }
    
    if($content -eq "")
    {
        SendPassMessage ` 
        -title "Build" `
        -message "Success"
    }
    else
    {
        SendFailMessage ` 
        -title "Build Failed" `
        -message $content
    }
}
function Configure-Watcher
{
    Register-ObjectEvent $global:watcher Changed -SourceIdentifier FileChanged -Action {     
    
        Build-Solution
    $name = $Event.SourceEventArgs.Name 
    $changeType = $Event.SourceEventArgs.ChangeType 
    $timeStamp = $Event.TimeGenerated 
}

function Read-CommandLineToQuit
{
    $quit = $args[0]
    return $quit;
}
function Start-Watch
{
    Configure-Watcher
    while(Read-CommandLineToQuit -ne  "q")
    {
        
    }
}
#========================================================================
# Starts the watching process looking for changed files for build-test
#========================================================================
Start-Watch