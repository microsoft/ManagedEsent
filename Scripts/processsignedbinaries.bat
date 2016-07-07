@echo off
if %echoon%.==1. echo on
setlocal
set version=%1
if %version%.==. goto :usage

@echo =-=-=-=-=-=-=-=
@echo Checking for files...
for %%i in ( esent.collections.dll esent.interop.dll esent.interop.wsa.dll esent.isam.dll ) do (
  if not exist %~dp0signed-%version%\%%i (
    echo Error: Prereq file %%i does not exist!
  )
)

for %%i in ( esent.collections.pdb esent.collections.xml esent.interop.pdb esent.interop.wsa.dll esedb.py esedbshelve.py esent.isam.pdb ) do (
  if not exist %~dp0tosign-%version%\%%i (
    echo Error: Prereq file %%i does not exist!
  )
)

@echo =-=-=-=-=-=-=-=
@echo Copying files for nuget...


@rem signed binaries; ManagedEsent
set dest=%~dp0nuget-%version%\ManagedEsent
for %%i in ( esent.interop.dll ) do (
  xcopy /d %~dp0signed-%version%\%%i %dest%\lib\net40\
)

for %%i in ( esent.interop.wsa.dll ) do (
  xcopy /d %~dp0signed-%version%\%%i %dest%\lib\netcore45\
)

@rem unsigned files; ManagedEsent
for %%i in ( esent.interop.xml esent.interop.pdb ) do (
  xcopy /d %~dp0tosign-%version%\%%i %dest%\lib\net40\
)

for %%i in ( esent.interop.wsa.pdb ) do (
  xcopy /d %~dp0tosign-%version%\%%i %dest%\lib\netcore45\
)

@rem signed binaries; PersistentDictionary
set dest=%~dp0nuget-%version%\Microsoft.Database.Collections.Generic
for %%i in ( esent.collections.dll ) do (
  xcopy /d %~dp0signed-%version%\%%i %dest%\lib\net40\
)

@rem unsigned files; PersistentDictionary
for %%i in ( esent.collections.pdb esent.collections.xml ) do (
  xcopy /d %~dp0tosign-%version%\%%i %dest%\lib\net40\
)

@rem signed binaries; Isam
set dest=%~dp0nuget-%version%\Microsoft.Database.Isam
for %%i in ( esent.isam.dll ) do (
  xcopy /d %~dp0signed-%version%\%%i %dest%\lib\net40\
)
@rem unsigned files; Isam
for %%i in ( esent.isam.pdb esent.isam.xml ) do (
  xcopy /d %~dp0tosign-%version%\%%i %dest%\lib\net40\
)

@echo =-=-=-=-=-=-=-=
@echo Generating nuget metadata file for ManagedEsent...

set dest=%~dp0nuget-%version%
set target=%dest%\ManagedEsent\ManagedEsent-%version%.nuspec

del %target%
echo ^<?xml version="1.0"?^> >>%target%
echo ^<package ^> >>%target%
echo  ^<metadata^> >>%target%
echo    ^<id^>ManagedEsent^</id^> >>%target%
echo    ^<version^>%version%^</version^> >>%target%
echo    ^<authors^>Microsoft^</authors^> >>%target%
echo    ^<owners^>Microsoft, nugetese, martinc^</owners^> >>%target%
echo    ^<licenseUrl^>http://managedesent.codeplex.com/license^</licenseUrl^> >>%target%
echo    ^<projectUrl^>http://managedesent.codeplex.com^</projectUrl^> >>%target%
echo    ^<!-- Unfortunately the following URL is not accepted by nuget. It contains an '='. --^> >>%target%
echo    ^<!-- ^<iconUrl^>https://download-codeplex.sec.s-msft.com/Download?ProjectName=managedesent^&DownloadId=801231^&Build=20865^</iconUrl^> --^> >>%target%
echo    ^<requireLicenseAcceptance^>true^</requireLicenseAcceptance^> >>%target%
echo    ^<description^> >>%target%
echo      ManagedEsent provides managed access to ESENT, the embeddable database engine native to Windows. ManagedEsent uses the esent.dll that is part of Microsoft Windows so there are no extra unmanaged binaries to download and install. >>%target%
echo    ^</description^> >>%target%
echo    ^<releaseNotes^>Release %version% from %date%. 1.9.4 has some bug fixes.^</releaseNotes^> >>%target%
echo    ^<copyright^>Copyright (c) Microsoft. All Rights Reserved.^</copyright^> >>%target%
echo    ^<tags^>ManagedEsent NoSql ISAM Database Storage DatabaseEngine^</tags^> >>%target%
echo    ^<!-- >>%target%
echo    ^<dependencies^> >>%target%
echo      ^<dependency id="SampleDependency" version="1.0" /^> >>%target%
echo    ^</dependencies^> >>%target%
echo    --^> >>%target%
echo  ^</metadata^> >>%target%
echo ^</package^> >>%target%

@echo =-=-=-=-=-=-=-=
@echo Generating nuget metadata file for PersistentDictionary...

set dest=%~dp0nuget-%version%
set target=%dest%\Microsoft.Database.Collections.Generic\Microsoft.Database.Collections.Generic-%version%.nuspec

del %target%
echo ^<?xml version="1.0"?^> >>%target%
echo ^<package ^> >>%target%
echo  ^<metadata^> >>%target%
echo    ^<id^>Microsoft.Database.Collections.Generic^</id^> >>%target%
echo    ^<version^>%version%^</version^> >>%target%
echo    ^<authors^>Microsoft^</authors^> >>%target%
echo    ^<owners^>Microsoft, nugetese, martinc^</owners^> >>%target%
echo    ^<licenseUrl^>http://managedesent.codeplex.com/license^</licenseUrl^> >>%target%
echo    ^<projectUrl^>http://managedesent.codeplex.com^</projectUrl^> >>%target%
echo    ^<!-- Unfortunately the following URL is not accepted by nuget. It contains an '='. --^> >>%target%
echo    ^<!-- ^<iconUrl^>https://download-codeplex.sec.s-msft.com/Download?ProjectName=managedesent^&DownloadId=801231^&Build=20865^</iconUrl^> --^> >>%target%
echo    ^<requireLicenseAcceptance^>true^</requireLicenseAcceptance^> >>%target%
echo    ^<description^> >>%target%
echo      PersistentDictionary is a simple class that implements IDictionary, and backs the storage to>>%target%
echo      disk. It allows a simple key-value pair store. It supports strings, value-types, and binary>>%target%
echo      blobs. It is built on the ManagedEsent library.>>%target%
echo    ^</description^> >>%target%
echo    ^<releaseNotes^>Release %version% from %date%. 1.9.4 has some bug fixes.^</releaseNotes^> >>%target%
echo    ^<copyright^>Copyright (c) Microsoft. All Rights Reserved.^</copyright^> >>%target%
echo    ^<tags^>ManagedEsent NoSql PersistentDictionary Persistent Dictionary Key-Value Store ^</tags^> >>%target%
echo    ^<dependencies^> >>%target%
echo      ^<dependency id="ManagedEsent" version="%version%" /^> >>%target%
echo      ^<dependency id="Microsoft.Database.Isam" version="%version%" /^> >>%target%
echo    ^</dependencies^> >>%target%
echo  ^</metadata^> >>%target%
echo ^</package^> >>%target%

@echo =-=-=-=-=-=-=-=
@echo Generating nuget metadata file for Microsoft.Database.Isam...

set dest=%~dp0nuget-%version%
set target=%dest%\Microsoft.Database.Isam\Microsoft.Database.Isam-%version%.nuspec

del %target%
echo ^<?xml version="1.0"?^> >>%target%
echo ^<package ^> >>%target%
echo  ^<metadata^> >>%target%
echo    ^<id^>Microsoft.Database.Isam^</id^> >>%target%
echo    ^<version^>%version%^</version^> >>%target%
echo    ^<authors^>Microsoft^</authors^> >>%target%
echo    ^<owners^>Microsoft, nugetese, martinc^</owners^> >>%target%
echo    ^<licenseUrl^>http://managedesent.codeplex.com/license^</licenseUrl^> >>%target%
echo    ^<projectUrl^>http://managedesent.codeplex.com^</projectUrl^> >>%target%
echo    ^<!-- Unfortunately the following URL is not accepted by nuget. It contains an '='. --^> >>%target%
echo    ^<!-- ^<iconUrl^>https://download-codeplex.sec.s-msft.com/Download?ProjectName=managedesent^&DownloadId=801231^&Build=20865^</iconUrl^> --^> >>%target%
echo    ^<requireLicenseAcceptance^>true^</requireLicenseAcceptance^> >>%target%
echo    ^<description^> >>%target%
echo      The ManagedEsentIsam project provides a simpler object model interface to create and access databases, using ManagedEsent (esent.dll). >>%target%
echo      As of mid-2014, the interface is still under development (for example, we should combine the 'Instance' and 'Database' classes), and may be broken
echo      in the future. We are releasing it to see if>>%target%
echo      anyone finds it useful. Please keep the feedback coming!>>%target%
echo    ^</description^> >>%target%
echo    ^<releaseNotes^>Release %version% from %date%. 1.9.4 has some bug fixes.^</releaseNotes^> >>%target%
echo    ^<copyright^>Copyright (c) Microsoft. All Rights Reserved.^</copyright^> >>%target%
echo    ^<tags^>ManagedEsent NoSql ISAM Database Storage DatabaseEngine^</tags^> >>%target%
echo    ^<dependencies^> >>%target%
echo      ^<dependency id="ManagedEsent" version="%version%" /^> >>%target%
echo    ^</dependencies^> >>%target%
echo  ^</metadata^> >>%target%
echo ^</package^> >>%target%


@echo =-=-=-=-=-=-=-=
@echo Copying files for zip...
set dest=%~dp0signed-%version%

for %%i in ( esent.collections.dll esent.interop.dll esent.interop.wsa.dll esent.isam.dll ) do (
  xcopy /d %~dp0signed-%version%\%%i %dest%\
)

for %%i in ( esent.collections.pdb esent.collections.xml esent.interop.pdb esent.interop.xml esent.interop.wsa.pdb esent.interop.wsa.xml esent.isam.pdb esent.isam.xml esedb.py esedbshelve.py ) do (
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

@set managedesentdestdir=.\nuget-%version%\ManagedEsent
@set collectionsdestdir=.\nuget-%version%\Microsoft.Database.Collections.Generic
@set isamdestdir=.\nuget-%version%\Microsoft.Database.Isam

@echo =-=-=-=-=-=-=-=
@echo.
@echo When ready to upload to nuget, run:
@echo nuget_pack-%version%.bat
@echo.
@echo nuget pack %managedesentdestdir%\ManagedEsent-%version%.nuspec -OutputDirectory %managedesentdestdir% > nuget_pack-%version%.bat
@echo nuget pack %collectionsdestdir%\Microsoft.Database.Collections.Generic-%version%.nuspec -OutputDirectory %collectionsdestdir% >> nuget_pack-%version%.bat
@echo nuget pack %isamdestdir%\Microsoft.Database.Isam-%version%.nuspec -OutputDirectory %isamdestdir% >> nuget_pack-%version%.bat
@echo.
@echo And if that was successful (verify with nuget package explorer, http://nuget.codeplex.com/downloads/get/clickOnce/NuGetPackageExplorer.application )
@echo run nuget_push-%version%.bat
@echo nuget push %managedesentdestdir%\ManagedEsent.%version%.nupkg > nuget_push-%version%.bat
@echo nuget push %collectionsdestdir%\Microsoft.Database.Collections.Generic.%version%.nupkg >> nuget_push-%version%.bat
@echo nuget push %isamdestdir%\Microsoft.Database.Isam.%version%.nupkg >> nuget_push-%version%.bat
@echo.
@echo You should also upload %~dp0signed-%version%\ManagedEsent%version%.zip to http://managedesent.codeplex.com/releases
@goto :eof

:usage
@echo Usage: %0 [version-number]
@echo   e.g. %0 1.8.1.0
