echo off
set solutionDir=%cd%

rem npm and grunt build for idenity ui src
echo npm and grunt build for identity ui src...
cd /D "%solutionDir%"\Web\identity-ui-src
call npm update
call grunt clean
call grunt

rem npm and grunt build for landing ui src
echo npm and grunt build for landing ui src...
cd /D "%solutionDir%"\Web\landing-ui-src
call npm update
call grunt clean
call grunt

rem back to original directory
cd /D "%solutionDir%"
echo.
echo.

pause
exit
