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
echo ------------------------------
echo ---- Building ATF VS 2010 ----
echo ------------------------------
echo.


pushd "%~dp0"
(
call "%~dp0\BuildAllVS2010Debug.cmd" false !VERB! && ^
call "%~dp0\BuildAllVS2010Release.cmd" false !VERB!
) || (echo. & echo **** Error building ATF VS 2010 **** & echo. & (IF %PAUSE%==true pause) & exit /b 1)
popd


:END
echo.
echo ----------------------------------------
echo ---- Building ATF VS 2010 Succeeded ----
echo ----------------------------------------
echo.
IF %PAUSE%==true pause
exit /b 0
