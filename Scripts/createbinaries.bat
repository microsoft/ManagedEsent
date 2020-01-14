setlocal
set version=%1
if %version%.==. goto :usage

@rem ==============
@rem Workflow:
@rem
@rem createbinaries.bat
@rem  -Compiles binaries
@rem  -Copies binaries to tosign-version
@rem
@rem signbinaries.bat
@rem  -Sign the binaries (DLLs only!) using ESRP
@rem
@rem processsignedbinaries.bat
@rem  -Copies (PDBs from tosign, and DLLs from signed) to tozip-version and nuget-version.
@rem  -Zips up the files in tozip-version. (requirement: zip.exe)
@rem
@rem Manual
@rem  -Upload the zip file
@rem  -run `nuget push`
@rem ==============


@rem Moving build to VS2017
@rem @set msbuildpath="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
@set msbuildpath="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe"
@if not exist %msbuildpath% set @set msbuildpath="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
@if not exist %msbuildpath% set msbuildpath=%windir%\microsoft.net\framework\v4.0.30319\msbuild.exe
@rem verbosity=minimal;Summary would be better, but
set msbuildexe=%msbuildpath% /nologo /property:Configuration=Release

@echo =-=-=-=-=-=-=-=
@echo Compiling... (http://xkcd.com/303/)

%msbuildexe% ..\EsentInterop\EsentInterop.csproj
if errorlevel 1 goto :eof

REM %msbuildexe% ..\EsentInterop\EsentInteropMetro.csproj
REM if errorlevel 1 goto :eof

%msbuildexe% ..\EsentCollections\EsentCollections.csproj
if errorlevel 1 goto :eof

%msbuildexe% ..\isam\Isam.csproj
if errorlevel 1 goto :eof

@echo =-=-=-=-=-=-=-=
@echo Copying output files to staging area...
set dest=%~dp0tosign-%version%

for %%i in ( esent.collections.dll esent.collections.pdb esent.collections.xml ) do (
  robocopy /S ..\EsentCollections\bin\release\ %dest%\ %%i
)

@rem for %%i in ( esent.interop.dll esent.interop.pdb esent.interop.wsa.dll esent.interop.wsa.pdb esent.interop.xml esent.interop.wsa.xml ) do (
for %%i in ( esent.interop.dll esent.interop.pdb esent.interop.xml ) do (
  robocopy /S ..\EsentInterop\bin\release\ %dest%\ %%i
)

for %%i in ( esent.isam.dll esent.isam.pdb esent.isam.xml ) do (
  robocopy /S ..\isam\bin\release\ %dest%\ %%i
)

for %%i in ( esedb.py esedbshelve.py ) do (
  xcopy /d ..\esedb\%%i %dest%\
)

@echo =-=-=-=-=-=-=-=
@echo.
@echo The next step is to sign the binaries (DLLs only!) using signbinaries.bat
@echo The source dir for the signing is %dest%
@echo Use both Strong Naming (72) and an Authenticode certificate (10006).
@echo DisplayName =
@echo ManagedEsent-%version%
@echo URL =
@echo http://github.com/Microsoft/ManagedEsent
@echo.
@echo And then run processsignedbinaries.bat %version%
@echo.
@goto :eof

:usage
@echo Usage: %0 [version-number]
@echo   e.g. %0 1.8.1
