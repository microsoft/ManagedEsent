setlocal
set version=%1
if %version%.==. goto :usage

@echo =-=-=-=-=-=-=-=
@echo Checking for files...
for %%i in ( esent.collections.dll esent.interop.dll esent.interop.wsa.dll ) do (
  if not exist %~dp0signed-%version%\%%i (
    echo Error: Prereq file %%i does not exist!
  )
)

for %%i in ( esent.collections.pdb esent.collections.xml esent.interop.pdb esent.interop.wsa.dll esedb.py esedbshelve.py ) do (
  if not exist %~dp0tosign-%version%\%%i (
    echo Error: Prereq file %%i does not exist!
  )
)

@echo =-=-=-=-=-=-=-=
@echo Copying files for nuget...
set dest=%~dp0nuget-%version%

@rem signed binaries
for %%i in ( esent.collections.dll esent.interop.dll ) do (
  xcopy /d %~dp0signed-%version%\%%i %dest%\lib\net40\
)

for %%i in ( esent.interop.wsa.dll ) do (
  xcopy /d %~dp0signed-%version%\%%i %dest%\lib\netcore45\
)

@rem unsigned files
for %%i in ( esent.collections.pdb esent.collections.xml esent.interop.xml esent.interop.pdb ) do (
  xcopy /d %~dp0tosign-%version%\%%i %dest%\lib\net40\
)

for %%i in ( esent.interop.wsa.pdb ) do (
  xcopy /d %~dp0tosign-%version%\%%i %dest%\lib\netcore45\
)

@echo =-=-=-=-=-=-=-=
@echo Generating nuget metadata file...

set dest=%~dp0nuget-%version%
set target=%dest%\ManagedEsent-%version%.nuspec

echo ^<?xml version="1.0"?^> >>%target%
echo ^<package ^> >>%target%
echo  ^<metadata^> >>%target%
echo    ^<id^>ManagedEsent^</id^> >>%target%
echo    ^<version^>%version%^</version^> >>%target%
echo    ^<authors^>martinc^</authors^> >>%target%
echo    ^<owners^>martinc^</owners^> >>%target%
echo    ^<licenseUrl^>http://managedesent.codeplex.com/license^</licenseUrl^> >>%target%
echo    ^<projectUrl^>http://managedesent.codeplex.com^</projectUrl^> >>%target%
echo    ^<!-- ^<iconUrl^>http://ICON_URL_HERE_OR_DELETE_THIS_LINE^</iconUrl^> --^> >>%target%
echo    ^<requireLicenseAcceptance^>false^</requireLicenseAcceptance^> >>%target%
echo    ^<description^> >>%target%
echo      ManagedEsent provides managed access to ESENT, the embeddable database engine native to Windows. ManagedEsent uses the esent.dll that is part of Microsoft Windows so there are no extra unmanaged binaries to download and install. >>%target%
echo    ^</description^> >>%target%
echo    ^<releaseNotes^>Release %version% from %date%. Windows 8.1 changes, dependent framework is now 4.0.^</releaseNotes^> >>%target%
echo    ^<copyright^>Copyright 2014 Microsoft. All Rights Reserved.^</copyright^> >>%target%
echo    ^<tags^>ManagedEsent NoSql ISAM^</tags^> >>%target%
echo    ^<!-- >>%target%
echo    ^<dependencies^> >>%target%
echo      ^<dependency id="SampleDependency" version="1.0" /^> >>%target%
echo    ^</dependencies^> >>%target%
echo    --^> >>%target%
echo  ^</metadata^> >>%target%
echo ^</package^> >>%target%


@echo =-=-=-=-=-=-=-=
@echo Copying files for zip...
set dest=%~dp0signed-%version%

for %%i in ( esent.collections.dll esent.interop.dll esent.interop.wsa.dll ) do (
  xcopy /d %~dp0signed-%version%\%%i %dest%\
)

for %%i in ( esent.collections.pdb esent.collections.xml esent.interop.pdb esent.interop.xml esent.interop.wsa.dll esedb.py esedbshelve.py ) do (
  xcopy /d %~dp0tosign-%version%\%%i %dest%\
)

@echo =-=-=-=-=-=-=-=
@echo Zipping files...

pushd %~dp0signed-%version%
zip.exe ManagedEsent%version%.zip *.dll *.pdb *.xml *.py

@if errorlevel 1 (
  echo zip.exe returned %errorlevel%. Have you installed/downloaded it?
)
popd

@echo Created %~dp0tosign-%version%\ManagedEsent%version%.zip !

@echo =-=-=-=-=-=-=-=
@echo.
@echo When ready to upload to nuget, run:
@echo.
@echo nuget pack %~dp0nuget-%version%\ManagedEsent-%version%.nuspec
@echo.
@echo And if that was successful (verify with nuget package explorer, http://nuget.codeplex.com/downloads/get/clickOnce/NuGetPackageExplorer.application )
@echo nuget push ManagedEsent.%version%.nupkg
@echo.
@echo You should also upload %~dp0signed-%version%\ManagedEsent%version%.zip to http://managedesent.codeplex.com/releases
@goto :eof

:usage
@echo Usage: %0 [version-number]
@echo   e.g. %0 1.8.1
