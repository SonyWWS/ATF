:: Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

:: This script builds all 4 configurations: 
::   - VS2010 Debug
::   - VS2010 Release
:: And runs the full test suite against the release configurations:
::   - VS2010 Release

@echo off

:: Usage: <script> <true|false>
:: Defaults: <script> true

:: Default is to pause; pass in "false" to not pause
SET PAUSE=%1
IF (%PAUSE%)==() SET PAUSE=true

:: Bamboo doesn't delete test results, so delete it ourselves to prevent the last
:: build's test results from being reported if an earlier step fails
del *TestResult*.xml /S

echo.
echo ==== Building and testing all ATF builds ====
echo.

pushd "%~dp0"
(
call "%~dp0\BuildAll.cmd" false && ^
call "%~dp0\RunTests.cmd" false 2010 All
) || ((IF %PAUSE%==true pause) & exit /b 1)
popd

echo.
echo ==== Building and testing all ATF builds succeeded ====
echo.

IF %PAUSE%==true pause
exit /b 0
