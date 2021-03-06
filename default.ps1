Framework "4.0"

Properties {
	$build_dir = Split-Path $psake.build_script_file
    $build_cfg = "Release"
}

Task default -Depends Test

Task Deps {
	Exec { & "$build_dir\tools\nuget\nuget.exe" install NUnit.Runners -Version "2.6.0.12051" -ExcludeVersion -OutputDirectory "$build_dir\tools" }
    
    Get-ChildItem "$build_dir\src\*\packages.config" -Exclude .nuget | ForEach-Object {
    	Write-Host "Downloading packages defined in $_"
    	Exec { & "$build_dir\tools\nuget\nuget.exe" install $_ -OutputDirectory "$build_dir\lib" }
    }
}

Task AssemblyInfo {
    $version = Get-Content $build_dir\version.txt
    $year = (Get-Date).Year
    @(
        "[assembly: System.Reflection.AssemblyTitle(`"NuGetCheck`")]",
        "[assembly: System.Reflection.AssemblyProduct(`"NuGetCheck`")]",
        "[assembly: System.Reflection.AssemblyDescription(`"Command line check tool for NuGet`")]",
        "[assembly: System.Reflection.AssemblyCopyright(`"Copyright � Gian Marco Gherardi $year`")]",
        "[assembly: System.Reflection.AssemblyTrademark(`"`")]",
        "[assembly: System.Reflection.AssemblyCompany(`"Gian Marco Gherardi`")]",
        "[assembly: System.Reflection.AssemblyConfiguration(`"$build_cfg`")]",
        "[assembly: System.Reflection.AssemblyVersion(`"$version.0`")]",
        "[assembly: System.Reflection.AssemblyFileVersion(`"$version.0`")]",
        "[assembly: System.Reflection.AssemblyInformationalVersion(`"$version`")]"
    ) | Out-File "$build_dir\src\SharedAssemblyInfo.cs" -Encoding 'utf8'
}

Task Compile -Depends Deps, AssemblyInfo {
    Write-Host "Compiling in $build_cfg configuration"
	Exec { msbuild "$build_dir\src\NuGetCheck.sln" /t:Build /p:Configuration=$build_cfg /v:quiet /nologo }
}

Task Test -Depends Compile {
    $TestDlls = Get-ChildItem "$build_dir\src\*\bin\$build_cfg\*.Tests.dll"
    Exec { & "$build_dir\tools\NUnit.Runners\tools\nunit-console.exe" /nologo /noresult /framework=4.0.30319 @TestDlls }
}

Task Publish -Depends Test {
    $version = Get-Content $build_dir\version.txt
    
    if(Test-Path $build_dir\build) {
        Remove-Item $build_dir\build -Force -Recurse
    }
    New-Item $build_dir\build -type directory
    New-Item $build_dir\build\tools -type directory

    @(
        "<?xml version=`"1.0`"?>",
        "<package>",
        "  <metadata>",
        "    <id>NuGetCheck</id>",
        "    <version>$version</version>",
        "    <authors>gimmi</authors>",
        "    <owners>gimmi</owners>",
        "    <licenseUrl>https://raw.github.com/gimmi/NuGetCheck/master/LICENSE</licenseUrl>",
        "    <projectUrl>https://github.com/gimmi/NuGetCheck</projectUrl>",
        "    <requireLicenseAcceptance>false</requireLicenseAcceptance>",
        "    <description>Command line check tool for NuGet</description>",
        "    <copyright>Gian Marco Gherardi 2013</copyright>",
        "    <tags>nuget tool commandline check</tags>",
        "  </metadata>",
        "</package>"
    ) | Out-File "$build_dir\build\Package.nuspec" -Encoding 'utf8'

    Copy-Item $build_dir\src\NuGetCheck\bin\$build_cfg\* $build_dir\build\tools

   	Exec { & "$build_dir\tools\nuget\nuget.exe" pack "$build_dir\build\Package.nuspec" -OutputDirectory $build_dir\build }
	Exec { & "$build_dir\tools\nuget\nuget.exe" push "$build_dir\build\NuGetCheck.$version.nupkg" }

    $version = $version -split "\."
    $version[2] =  [System.Int32]::Parse($version[2]) + 1
    $version -join "." | Out-File  $build_dir\version.txt -Encoding 'utf8'
}

Task Clean {
	Exec { git --git-dir="$build_dir\.git" --work-tree="$build_dir" clean -d -x -f }
}
