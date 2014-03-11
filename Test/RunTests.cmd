:: Copyright (c) Sony Computer Entertainment 2011.
:: All rights Reserved. Confidential.

@echo off

:: Usage: <script> <true|false> <2008|2010> <All|SmokeTest>
:: Defaults: <script> true 2010 SmokeTest
:: Note: This script assumes ATF has already been built

:: Default is to pause; pass in "false" to not pause
SET PAUSE=%1
IF (%PAUSE%)==() SET PAUSE=true

:: Second parameter is the VS version; by default it's "2010"
SET VS_VERSION=%2
IF (%VS_VERSION%)==() SET VS_VERSION=2010

:: Third parameter is the test category; by default it's "SmokeTest" ro run a subset of tests
SET TEST_CATEGORY=%3
IF (%TEST_CATEGORY%)==() SET TEST_CATEGORY=SmokeTest

pushd "%~dp0"
cd "%~dp0/../bin/Release.vs%VS_VERSION%/tests"

:: Bamboo doesn't delete test results, so delete it ourselves to prevent the last
:: build's test results from being reported if an earlier step fails
del *TestResult*.xml /S

:: Unit tests are not supported for vs2010
IF (%VS_VERSION%)==(2008) goto test2008

:test2010
echo.
echo ==== Testing ATF VS %VS_VERSION% - Release, unit tests and functional tests ====
echo.

pushd "%cd%"

echo Run unit tests
UnitTests.exe
if not %errorlevel% == 0 (
  echo Error running unit tests
  popd
  if %PAUSE%==true pause
  exit /b %errorlevel%
)

echo Run functional tests
FunctionalTests.exe -category:%TEST_CATEGORY%
if not %errorlevel% == 0 (
  echo Error running functional tests
  popd
  if %PAUSE%==true pause
  exit /b %errorlevel%
)

:done
popd
echo.
echo ==== Testing ATF VS %VS_VERSION% - Release Succeeded ====
echo.

IF %PAUSE%==true pause
exit /b 0
