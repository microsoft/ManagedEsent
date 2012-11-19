setlocal
set version=%1
if %version%.==. goto :usage

set msbuildexe=%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /property:Configuration=Release

@echo =-=-=-=-=-=-=-=
@echo Compiling... (http://xkcd.com/303/)

%msbuildexe% ..\EsentInterop\EsentInterop.csproj
if errorlevel 1 goto :eof

%msbuildexe% ..\EsentCollections\EsentCollections.csproj
if errorlevel 1 goto :eof


@echo =-=-=-=-=-=-=-=
@echo Copying files for nuget...

for %%i in ( esent.collections.dll esent.collections.pdb esent.collections.xml ) do (
  xcopy /d ..\EsentCollections\bin\release\%%i %~dp0nuget\lib\net40\
)

for %%i in ( esent.interop.dll esent.interop.pdb esent.interop.xml ) do (
  xcopy /d ..\EsentInterop\bin\release\%%i %~dp0nuget\lib\net40\
)

@echo =-=-=-=-=-=-=-=
@echo Copying files for zip...

for %%i in ( esent.collections.dll esent.collections.pdb esent.collections.xml ) do (
  xcopy /d ..\EsentCollections\bin\release\%%i %~dp0tozip\
)

for %%i in ( esent.interop.dll esent.interop.pdb esent.interop.xml ) do (
  xcopy /d ..\EsentInterop\bin\release\%%i %~dp0tozip\
)

for %%i in ( esedb.py esedbshelve.py ) do (
  xcopy /d ..\esedb\%%i %~dp0tozip\
)

pushd %~dp0tozip
zip.exe ManagedEsent%version%.zip *.dll *.pdb *.xml *.py

@if errorlevel 1 (
  echo zip.exe returned %errorlevel%. Have you installed/downloaded it?
)
popd

@echo Created %~dp0tozip\ManagedEsent%version%.zip !

@echo =-=-=-=-=-=-=-=
@echo.
@echo When ready to upload to nuget, run:
@echo.
@echo nuget pack %~dp0nuget\ManagedEsent.nuspec
@echo.
@echo And if that was successful (verify with nuget package explorer, http://nuget.codeplex.com/downloads/get/clickOnce/NuGetPackageExplorer.application )
@echo nuget push ManagedEsent.%version%.nupkg
@echo.
@echo You should also upload %~dp0tozip\ManagedEsent%version%.zip to http://managedesent.codeplex.com/releases
@goto :eof

:usage
@echo Usage: %0 [version-number]
@echo   e.g. %0 1.8.1
