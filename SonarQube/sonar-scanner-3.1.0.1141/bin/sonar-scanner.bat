@REM SonarQube Scanner Startup Script for Windows
@REM
@REM Required ENV vars:
@REM   JAVA_HOME - location of a JDK home dir
@REM
@REM Optional ENV vars:
@REM   SONAR_SCANNER_OPTS - parameters passed to the Java VM when running the SonarQube Scanner

@echo off

set ERROR_CODE=0

@REM set local scope for the variables with windows NT shell
@setlocal

set "scriptdir=%~dp0"
if #%scriptdir:~-1%# == #\# set scriptdir=%scriptdir:~0,-1%
set "SONAR_SCANNER_HOME=%scriptdir%\.."

@REM ==== START VALIDATION ====
@REM *** JAVA EXEC VALIDATION ***

set use_embedded_jre=false
if "%use_embedded_jre%" == "true" (
  set "JAVA_HOME=%SONAR_SCANNER_HOME%\jre"
)

if not "%JAVA_HOME%" == "" goto foundJavaHome

for %%i in (java.exe) do set JAVA_EXEC=%%~$PATH:i

if not "%JAVA_EXEC%" == "" (
  set JAVA_EXEC="%JAVA_EXEC%"
  goto OkJava
)

if not "%JAVA_EXEC%" == "" goto OkJava

echo.
echo ERROR: JAVA_HOME not found in your environment, and no Java
echo        executable present in the PATH.
echo Please set the JAVA_HOME variable in your environment to match the
echo location of your Java installation, or add "java.exe" to the PATH
echo.
goto error

:foundJavaHome
if EXIST "%JAVA_HOME%\bin\java.exe" goto foundJavaExeFromJavaHome

echo.
echo ERROR: JAVA_HOME exists but does not point to a valid Java home
echo        folder. No "\bin\java.exe" file can be found there.
echo.
goto error

:foundJavaExeFromJavaHome
set JAVA_EXEC="%JAVA_HOME%\bin\java.exe"

:OkJava
goto run


@REM ==== START RUN ====
:run

set PROJECT_HOME=%CD%

@REM remove trailing backslash, see https://groups.google.com/d/msg/sonarqube/wi7u-CyV_tc/3u9UKRmABQAJ
IF %PROJECT_HOME:~-1% == \ SET PROJECT_HOME=%PROJECT_HOME:~0,-1%

%JAVA_EXEC% -Djava.awt.headless=true %SONAR_SCANNER_DEBUG_OPTS% %SONAR_SCANNER_OPTS% -cp "%SONAR_SCANNER_HOME%\lib\sonar-scanner-cli-3.1.0.1141.jar" "-Dscanner.home=%SONAR_SCANNER_HOME%" "-Dproject.home=%PROJECT_HOME%" org.sonarsource.scanner.cli.Main %*
if ERRORLEVEL 1 goto error
goto end

:error
set ERROR_CODE=1

@REM ==== END EXECUTION ====

:end
@REM set local scope for the variables with windows NT shell
@endlocal & set ERROR_CODE=%ERROR_CODE%

@REM see http://code-bear.com/bearlog/2007/06/01/getting-the-exit-code-from-a-batch-file-that-is-run-from-a-python-program/
goto exit

:returncode
exit /B %1

:exit
call :returncode %ERROR_CODE%
