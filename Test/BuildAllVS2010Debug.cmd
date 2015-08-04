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


:: Find the Visual Studio compiler. Prefer the oldest that we support, because
::	we developers tend to use later versions day-to-day.
IF NOT "%VS140COMNTOOLS%"==() SET DEVENV="%VS140COMNTOOLS%..\ide\devenv.com"
IF NOT "%VS120COMNTOOLS%"==() SET DEVENV="%VS120COMNTOOLS%..\ide\devenv.com"
IF NOT "%VS110COMNTOOLS%"==() SET DEVENV="%VS110COMNTOOLS%..\ide\devenv.com"
IF NOT "%VS100COMNTOOLS%"==() SET DEVENV="%VS100COMNTOOLS%..\ide\devenv.com"

IF NOT EXIST %DEVENV% (
echo devenv[2010] not found at %DEVENV%, aborting
exit /B 2
)


echo.
echo ==== Building ATF VS 2010 - Debug ====
echo.
pushd "%~dp0"
(
!DEVENV! "Everything.vs2010.sln" /!VERB! "Debug"
) || (echo. & echo **** Error building ATF VS 2010 Debug **** & echo. & (IF %PAUSE%==true pause) & exit /b 1)
popd


IF %PAUSE%==true pause
exit /b 0
