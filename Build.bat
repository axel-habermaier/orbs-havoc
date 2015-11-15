@echo off

echo =====================================================================
echo Installing NuGet packages...
echo =====================================================================
"Dependencies/NuGet.exe" restore "Orbs Havoc.sln" -OutputDirectory "Dependencies/Packages"

echo =====================================================================
echo Compiling solution...
echo =====================================================================
"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv" "Orbs Havoc.sln" /Rebuild "Release|x64"

if exist \Binaries (
    rd Binaries /S /Q
)

mkdir Binaries

echo =====================================================================
echo Merging assemblies...
echo =====================================================================
cd Build\Release
..\..\Dependencies\ILRepack.exe /out:"Orbs Havoc.exe" /internalize "Orbs Havoc.exe" "Orbs Havoc.IL.dll"
cd ..\..\Binaries

echo =====================================================================
echo Copying files...
echo =====================================================================
copy "..\Build\Release\*.pak" 
copy "..\Build\Release\Orbs Havoc.exe"
copy "..\Build\Release\SDL2.dll"

cd..