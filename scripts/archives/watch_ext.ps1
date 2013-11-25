Set-ExecutionPolicy Unrestricted -Force -Scope CurrentUser

#---------------------------------------------
#Script Globals
#---------------------------------------------
$global:pass_indicator = "pass"
$global:fail_indicator = "fail"
$global:notification_executable = "c:\repositories\xspec\build\xspec.notifier.exe"
$global:solution ="c:\repositories\xspec\xspec.sln"
$global:dev_engine = "c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe"
$global:build_engine = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild"
$global:test_engine = "C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\mstest"
#---------------------------------------------
#Script Functions
#---------------------------------------------
function Parse-BuildOutput{
	param(
		[string]$result = ""
	)
	
	#-- parse the msbuild output from the solution and determine whether or not to continue:
    $newline = [System.Environment]::NewLine;
	$content = "";
	$failed_build_triggered = $false;
	
	foreach( $line in $result )
	{
		if($line.StartsWith("Build Failed"))
		{
			$failed_build_triggered = $true;
		}
		
		if($line.trim() -ne "" )
		{
		 $content += [System.String]::Concat($content, $line, $newline);
		}
	}

	$title = "Build - OK";
	$level = $global:pass_indicator; 
	$message = "Success";
	
	if($content -ne "")
	{
		$title = "Build - Error";
		$level = $global:fail_indicator;
		$message = $content;
	}
	
	# -- send notification about build:	
	Generate-Notification `
		-level $level `
		-title $title `
		-message $message
}
function Parse-TestOutput{
	param(
		[string]$result = ""
	)
	
	#-- use xspec console test output parser:
	Parse-xSpecTestOutput `
	-result $result
}
#---------------------------------------------------------------------------------------  
# function to generate the user feedback notification for the build and test cycle.
#--------------------------------------------------------------------------------------- 
function Generate-Notification{
	param(
	[string]$level = "info", 
	[string]$title = "", 
	[string]$message = ""
	)
	
#    UseGrowlForNotifications `
#    -level $level `
#	 -title $title `
#	 -message $message
    
	UseXSpecNotifyForNotification `
    -level $level `
	-title $title `
	-message $message
}

# ---------- private functions ------------
function Parse-xSpecTestOutput{
	param(
		[string]$result = ""
		)
	
	$newline = [System.Environment]::NewLine;
	$content = "";
	
	foreach( $line in $result )
	{
		if($line.trim().StartsWith("it") -ne $false)
		{
            if($line.trim().Contains("FAILED") -ne $false)
            {
			 $content += [System.String]::Concat($content, ">> ", $line, $newline);
            }
		}
	}
	
	$title = "Test(s) Passed";
	$level = $global:pass_indicator; 
	$message = "Success";
	
	if($content -ne "")
	{
		$title = "Test(s) Failed";
		$level = $global:fail_indicator;
		$message = $content;
	}
	
	# -- send notification about test session:	
	Generate-Notification `
		-level $level `
		-title $title `
		-message $message
}
function Kill-Notification-Agents{

	#-- using xspec notify for feedback to the user:
    $processes = [System.Diagnostics.Process]::GetProcessesByName("xspec.notifier");
    foreach($process in $processes)
    {
        $process.kill();
    }
}
function UseXSpecNotifyForNotification{
	param(
	[string]$level = "info", 
	[string]$title = "Title", 
	[string]$message = "Message"
	)
	
    Kill-Notification-Agents
    
    $escaped_title = Escape-DoubleQuotes `
                      -value $title
                      
    $escaped_msg =  Escape-DoubleQuotes `
                      -value $message                     
        
    $command = @(); 
    $command += $global:notification_executable + " /level:$level /title:$escaped_title /message:$escaped_msg"
    Invoke-Expression $command[0];
}
function UseGrowlForNotifications{
	param(
	[string]$level = "info", 
	[string]$title = "Title", 
	[string]$message = "Message"
	)
	
	# -- using growl notification for feedback to user (change to growl at top of script):    
    $escaped_title = Escape-DoubleQuotes `
                      -value $title
                      
    $escaped_msg =  Escape-DoubleQuotes `
                      -value $message 
                      
    $command = @(); 
    $command += $global:notification_executable + " /i:$level.png /t:$escaped_title $escaped_msg"
	Invoke-Expression $command[0];
}
function Escape-DoubleQuotes
{
 param(
 [string]$value = ""
 )
 
 return "`"$value`""
}
#---------------------------------------------
#Script Start
#---------------------------------------------
Parse-TestOutput `
-result "it should pass"