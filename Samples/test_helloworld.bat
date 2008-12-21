@echo off
rmdir /s /q tests 2> nul
mkdir tests
pushd tests
..\C\HelloWorld\Debug\HelloWorld.exe
del /q *
..\C\HelloWorld\Release\HelloWorld.exe
del /q *
..\Csharp\HelloWorld\bin\Debug\CsHelloWorld.exe
del /q *
..\Csharp\HelloWorld\bin\Release\CsHelloWorld.exe
del /q *
..\Vb\HelloWorld\bin\Debug\VbHelloworld.exe
del /q *
..\Vb\HelloWorld\bin\Release\VbHelloworld.exe
popd
rmdir /s /q tests



