:: Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

@echo off

:: This script builds the release configurations:
::   - VS2008 Release
::   - VS2010 Release
:: And runs sanity tests against the VS2010 Release build

:: Usage: <script> <true|false>
:: Defaults: <script> true

:: Default is to pause; pass in "false" to not pause
SET PAUSE=%1
IF (%PAUSE%)==() SET PAUSE=true

:: Bamboo doesn't delete test results, so delete it ourselves to prevent the last
:: build's test results from being reported if an earlier step fails
del *TestResult*.xml /S

echo.
echo ==== Smoke testing ATF Release builds ====
echo.

pushd "%~dp0"
(
call "%~dp0\BuildAllRelease.cmd" false && ^
call "%~dp0\RunTests.cmd" false 2010 SmokeTest
) || ((IF %PAUSE%==true pause) & exit /b 1)
popd

echo.
echo ==== Smoke testing ATF Release builds Succeeded ====
echo.

IF %PAUSE%==true pause
exit /b 0
