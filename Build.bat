@echo off

if exist Binaries (
    rd Binaries /S /Q
)

if exist Build (
    rd Build /S /Q
)

echo =====================================================================
echo Restoring packages...
echo =====================================================================
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" "Source/Assets Compiler/Assets Compiler.csproj" /t:restore /p:Configuration=Release /nr:false /nologo /v:minimal /p:Platform=x64
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" "Source/Orbs Havoc/Orbs Havoc.csproj" /t:restore /p:Configuration=Release /nr:false /nologo /v:minimal /p:Platform=x64

echo =====================================================================
echo Compiling solution...
echo =====================================================================
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" "Orbs Havoc.sln" /t:rebuild /p:Configuration=Release /nr:false /nologo /v:minimal /p:Platform=x64

mkdir Binaries

echo =====================================================================
echo Merging assemblies...
echo =====================================================================
cd Build\Release
"..\..\Dependencies\ILRepack.exe" /out:"Orbs Havoc.exe" /internalize "Orbs Havoc.exe" "System.ValueTuple.dll" "System.Runtime.CompilerServices.Unsafe.dll"
cd ..\..\Binaries

echo =====================================================================
echo Copying files...
echo =====================================================================
copy "..\Build\Release\Orbs Havoc.exe"
copy "..\Build\Release\SDL2.dll"

cd..