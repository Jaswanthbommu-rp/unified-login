echo off
set solutionDir=%cd%
set solutionFile=%cd%\MasterProjectSolution.sln

rem do nuget restore

rem change dir where msbuild 12 is
cd /D "c:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\"

echo Starting nuget restore...
msbuild /t:restore "%solutionFile%"
rem .nuget\nuget.exe restore IdentityAPI.sln
rem .nuget\nuget.exe restore Landing.sln
rem .nuget\nuget.exe restore LandingAPI.sln
rem .nuget\nuget.exe restore RPIdentity.sln

echo End nuget restore...

echo.
echo.

echo Starting .net build...

rem build the solution
msbuild "%solutionFile%" /m /property:Configuration=Debug

rem The problem of building each solution separately is the build summary would appear after each solution build which is tedious to find.
rem msbuild "%solutionDir%"\IdentityAPI.sln
rem msbuild "%solutionDir%"\Landing.sln
rem msbuild "%solutionDir%"\LandingAPI.sln
rem msbuild "%solutionDir%"\RPIdentity.sln

rem back to original directory
cd /D "%solutionDir%"
echo.
echo.

pause
rem exit

