:: Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

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

echo.
echo ==== Build All Started ====
echo.


:: Tell scripts not to pause since this script
:: will handle any pausing that needs to be done.
pushd "%~dp0"
(
call "%~dp0\BuildAllVS2010.cmd" false !VERB!
) || (echo. & echo **** Build All Failed! **** & echo. & (IF %PAUSE%==true pause) & exit /b 1)
popd


echo.
echo ==== Build All Successful! ====
echo.
IF %PAUSE%==true pause
exit /b 0
