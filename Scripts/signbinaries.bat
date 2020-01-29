@echo off
if %echoon%.==1. echo on
setlocal
set version=%1
if %version%.==. goto :usage

@echo =-=-=-=-=-=-=-=
@echo Checking for files...
for %%i in ( esent.collections.dll esent.interop.dll esent.isam.dll ) do (
  if not exist %~dp0tosign-%version%\netstandard2.0\%%i (
    echo Error: Prereq file %%i does not exist!
  )
)

@set esrpclient="g:\CxCache\EsrpClient.1.1.5\tools\EsrpClient.exe"
@if not exist %esrpclient% ( 
	@echo Can't find ESRPClient.exe
	goto :usage
)

@set esrpfiles=E:\managedesent_github2\ESRP
@pushd %esrpfiles%

@echo Fix source and destination to match version %version%
start "" /B /WAIT notepad.exe input.json
start "" /B /WAIT notepad.exe output.json

%esrpclient% sign -a authentication.json -p policy.json -i input.json -o output.json -l Verbose -f stdout
if errorlevel 1 goto :eof

@echo Checking for signed files...
for %%i in ( esent.collections.dll esent.interop.dll esent.isam.dll ) do (
  if not exist %~dp0signed-%version%\netstandard2.0\%%i (
    echo Error: Prereq file %%i does not exist!
  )
)

@echo =-=-=-=-=-=-=-=
@echo.
@echo The next step is to run processsignedbinaries.bat %version%
@echo.
@goto :eof

:usage
@echo Usage: %0 [version-number]
@echo   e.g. %0 1.9.5
