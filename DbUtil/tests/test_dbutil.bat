@echo off

REM A simple test script for the Dbutil executable. This batch
REM file runs the commands and compares the output with the 
REM expected output.

set DATABASE=testing.db
set DBDIR=test
rmdir /s /q bin\debug\%DBDIR%\ 2> nul
mkdir bin\debug\test\
pushd bin\debug\test

..\DbUtil createsample %DATABASE%
if NOT %ERRORLEVEL%==0 goto :Fail

..\Dbutil dumpmetadata %DATABASE% > metadata.txt
if NOT %ERRORLEVEL%==0 goto :Fail

echo n | comp /a metadata.txt ..\..\..\tests\expected_metadata.txt  1>nul 2>nul
if NOT %ERRORLEVEL%==0 goto :Fail

..\Dbutil dumptocsv %DATABASE% table

popd
rmdir /s /q bin\debug\%DBDIR%\

echo **********************************
echo Test Passed!
echo **********************************

goto :EOF

:Fail
echo.
echo **********************************
echo Test failed!
echo **********************************

