param([string][parameter(mandatory)]$NugetApiKey)

pushd $PSScriptRoot
try {
    ls -r -fi JobScheduler.*.nupkg | rm

    msbuild /t:pack /p:Configuration=Release
    if(!$?) {
        throw "Failed to build package."
    }

    $packagePath = ls -r -fi JobScheduler.*.nupkg | select -expand FullName
    Write-Host "Pushing $packagePath..."
    nuget push $packagePath -ApiKey $NugetApiKey -Source https://api.nuget.org/v3/index.json
    if(!$?) {
        throw "Failed to push package."
    }
}
finally {
    popd
}