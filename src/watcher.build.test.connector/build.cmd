cls
set current_directory = %CD%
set build_dir = %current_directory%\build
rmdir %build_dir%
mkdir %build_dir%
cls
msbuild /v:quiet /p:OutDir=%CD%\build\ %1
