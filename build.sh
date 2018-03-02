#!/usr/bin/env bash
set -e

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
export HOME=~
export NUGET_PACKAGES=~/.nuget/packages
export NUGET_HTTP_CACHE_PATH=~/.local/share/NuGet/v3-cache

Configuration=Debug

RootDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
DotNetSDKPath=$RootDir"/.tools/dotnet/"$DotNetSDKVersion
DotNetExe=$DotNetSDKPath"/dotnet"

TestResults=$RootDir"/TestResults"

usage() { echo "Usage: build.sh [-c|--configuration <Debug|Release>]"; }

prebuild() {
    $DotNetExe restore

    local catalog=$RootDir"/.data/catalog.bin"
    local data=$(dirname $catalog)

    if [[ ! -e $data ]]; then
        mkdir $data
    fi

    if [[ ! -e $catalog ]]; then
        echo "Downloading catalog.bin..."
        curl --output $catalog "https://portability.blob.core.windows.net/catalog/catalog.bin"
    fi
}

installSDK() {
    if [[ -e $DotNetExe ]]; then
        echo $DotNetExe" exists.  Skipping install..."
        return 0
    fi

    local DotNetToolsPath=$(dirname $DotNetSDKPath)

    if [ ! -d $DotNetToolsPath ]; then
        mkdir -p $DotNetToolsPath
    fi

    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel Current --install-dir $DotNetSDKPath
}

build() {
    echo "Building ApiPort... Configuration: ["$Configuration"]"

    pushd src/ApiPort/ApiPort > /dev/null
    $DotNetExe build ApiPort.csproj -f netcoreapp2.0 -c $Configuration
    $DotNetExe build ApiPort.Offline.csproj -f netcoreapp2.0 -c $Configuration
    popd > /dev/null
}

runTest() {
    ls $1/*.csproj | while read file
    do
        if awk -F: '/<TargetFramework>netcoreapp[1-9]\.[0-9]<\/TargetFramework>/ { found = 1 } END { if (found == 1) { exit 0 } else { exit 1 } }' $file; then
            echo "Testing "$file
            $DotNetExe test $file -c $Configuration --logger trx
        else
            # Can remove this when: https://github.com/dotnet/sdk/issues/335 is resolved
            echo "Skipping "$file
            echo "--- Desktop .NET Framework testing is not currently supported on Unix."
        fi
    done

    if [ ! -d $TestResults ]; then
        mkdir $TestResults
    fi

    find $RootDir/tests/ -type f -name "*.trx" | while read line
    do
        mv $line $TestResults/
    done
}

while [[ $# -gt 0 ]]
do
    option="$(echo $1 | awk '{print tolower($0)}')"
    case "$option" in
        "-?" | "--help" )
        usage
        exit 1
        ;;
        "-c" | "--configuration")
        Configuration="$2"
        shift 2
        ;;
        *)
        echo "Unknown option: "$option
        usage
        exit 1
        ;;
    esac
done

# Enable insensitive case-matching
shopt -s nocasematch

if [[ "$Configuration" != "Debug" && "$Configuration" != "Release" ]]; then
    echo "ERROR: Supported configuration types are Debug or Release.  Invalid configuration: "$Configuration
    usage
    exit 3
fi

shopt -u nocasematch

installSDK

if [[ ! -e $DotNetExe ]]; then
    echo "ERROR: It should have been installed from build/dotnet-install.sh"
    exit 2
fi

prebuild

build

find tests/ -type d -name "*\.Tests" | while read file
do
    runTest $file
done

echo "Finished!"
