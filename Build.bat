@echo off
@title Building

@set projectName=Unity-2048
@REM @set BuildTarget=Android

@set unityPath="C:\Program Files\Unity 2018.4.11f1\Editor\Unity.exe"

@REM for %%d in (%~dp0..) do set outputRoot=%%~fd\Build
@set outputRoot=%cd%\Build

@set projectPath=%cd%\%projectName%

@set logPath=%outputRoot%\Build.log

@echo Start building...
@echo Build target : %BuildTarget%
@echo Project path : %projectPath%
@echo Output root : %outputRoot%
@echo Log path : %logPath%

if %BuildTarget%==StandaloneWindows64 (
    %unityPath% -quit -batchmode -projectPath %projectPath% -executeMethod Builder.BuildStandaloneWindows64 -logFile %logPath%
) ^
else if %BuildTarget%==Android (
    %unityPath% -quit -batchmode -projectPath %projectPath% -executeMethod Builder.BuildAndroid -logFile %logPath%
) ^
else if %BuildTarget%==iOS (
    @echo not implemented
)

@setlocal enabledelayedexpansion
@echo error level : !errorlevel!

if !errorlevel!==0 (
    @echo success
) else (
    @echo failed
)

@REM if exist %outputRoot% explorer %outputRoot%
@REM if exist %logPath% start "" %logPath%

@REM @pause