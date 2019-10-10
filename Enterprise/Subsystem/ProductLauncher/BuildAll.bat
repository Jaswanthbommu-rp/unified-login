echo off
set solutionDir=%cd%

rem do .net build
echo Start .Net build...
start ".Net Build" Build.Net.bat
echo Start UI build...
start "UI Build" BuildUI.bat



