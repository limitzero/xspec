#-- base path variables
$base_dir  = resolve-path .
$build_dir = "$base_dir\build"
$buildartifacts_dir = "$build_dir\" 

# -- clean the build directory:
remove-item -force -recurse $buildartifacts_dir -ErrorAction SilentlyContinue

#-- create the build directory:
new-item $buildartifacts_dir -itemType directory 

#-- build the solution:
$solution_file = $args[0]
msbuild /v:quiet /p:OutDir="$buildartifacts_dir" "$solution_file"
