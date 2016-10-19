@echo off

if exist Binaries (
    rd Binaries /S /Q
)

echo =====================================================================
echo Installing NuGet packages...
echo =====================================================================

if not exist "./Dependencies/NuGet.exe" (
	powershell -Command "Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile ./Dependencies/NuGet.exe"
)

"Dependencies/NuGet.exe" restore "Orbs Havoc.sln" -OutputDirectory "Dependencies/Packages"
"Dependencies/NuGet.exe" install ILRepack -OutputDirectory "Dependencies/Packages" -Version 2.0.10

echo =====================================================================
echo Compiling solution...
echo =====================================================================
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" "Orbs Havoc.sln" /t:rebuild /p:Configuration=Release /nr:false /nologo /v:minimal /p:Platform=x64

mkdir Binaries

echo =====================================================================
echo Merging assemblies...
echo =====================================================================
cd Build\Release
"..\..\Dependencies\Packages\ILRepack.2.0.10\tools\ILRepack.exe" /out:"Orbs Havoc.exe" /internalize "Orbs Havoc.exe" "Orbs Havoc.IL.dll"
cd ..\..\Binaries

echo =====================================================================
echo Copying files...
echo =====================================================================
copy "..\Build\Release\Assets.pak" 
copy "..\Build\Release\Orbs Havoc.exe"
copy "..\Build\Release\SDL2.dll"

cd..