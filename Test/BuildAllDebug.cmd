:: Copyright (c) Sony Computer Entertainment 2011.
:: All rights Reserved. Confidential.

@echo off
setlocal
setlocal ENABLEDELAYEDEXPANSION


:: Usage: <script> <true|false> <build|rebuild>
:: Defaults: <script> true rebuild


:: Default is to pause; pass in "false" to not pause
SET PAUSE=%1
IF (%PAUSE%)==() SET PAUSE=true


:: Second parameter is the verb; by default it's "Rebuild"
SET VERB=%2
IF (%VERB%)==() SET VERB=Rebuild


:: Bamboo doesn't delete test results, so delete it ourselves to prevent the last
:: build's test results from being reported if an earlier step fails
del *TestResult*.xml /S


echo.
echo ==== Build All Started ====
echo.


:: Tell scripts not to pause since this script
:: will handle any pausing that needs to be done.
pushd "%~dp0"
(
call "%~dp0\BuildAllVS2010Debug.cmd" false !VERB!
) || (echo. & echo **** Build All Failed! **** & echo. & (IF %PAUSE%==true pause) & exit /b 1)
popd


echo.
echo ==== Build All Successful! ====
echo.
IF %PAUSE%==true pause
exit /b 0
