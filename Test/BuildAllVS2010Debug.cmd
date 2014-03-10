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


SET DEVENV100="%VS100COMNTOOLS%..\ide\devenv.com"
IF NOT EXIST %DEVENV100% (
echo devenv[2010] not found at %DEVENV100%, aborting
exit /B 2
)


echo.
echo ==== Building ATF VS 2010 - Debug ====
echo.
pushd "%~dp0"
(
!DEVENV100! "Everything.vs2010.sln" /!VERB! "Debug"
) || (echo. & echo **** Error building ATF VS 2010 Debug **** & echo. & (IF %PAUSE%==true pause) & exit /b 1)
popd


IF %PAUSE%==true pause
exit /b 0
