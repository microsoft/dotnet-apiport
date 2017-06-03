#!/usr/bin/env bash
set -e

Configuration=Debug

DotNetSDKChannel="preview"
DotNetSDKVersion="1.0.4"

RootDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
DotNetSDKPath=$RootDir"/.tools/dotnet/"$DotNetSDKVersion
DotNetExe=$DotNetSDKPath"/dotnet"

usage() { echo "Usage: build.sh [-c|--configuration <Debug|Release>]"; }

prebuild() {
    local catalog=$RootDir"/.data/catalog.bin"
    local data=$(dirname $catalog)

    if [[ ! -e $data ]]; then
        mkdir $data
    fi

    if [[ ! -e $catalog ]]; then
        echo "Downloading catalog.bin..."
        curl --output $catalog "https://portabilitystorage.blob.core.windows.net/catalog/catalog.bin?sr=c&sv=2015-02-21&si=Readcatalog&sig=8tOHoX2ZvcSFLol0GI6lxmydNPJbnJdHNLKr06aD7t4%3D"
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

    echo "Installing "$DotNetSDKVersion"from "$DotNetSDKChannel" channel..."

    $RootDir/build/dotnet-install.sh --channel $DotNetSDKChannel --version $DotNetSDKVersion --install-dir $DotNetSDKPath
}

build() {
    echo "Building ApiPort... Configuration: "$Configuration
    pushd src/ApiPort > /dev/null
    $DotNetExe restore
    $DotNetExe build -f netcoreapp1.0 -c $Configuration
    popd > /dev/null
}

runTest() {
    ls $1/*.csproj | while read file
    do
        if awk -F: '/<TargetFramework>netcoreapp1\.[0-9]<\/TargetFramework>/ { found = 1 } END { if (found == 1) { exit 0 } else { exit 1 } }' $file; then
            echo "Testing " $file
            $DotNetExe restore
            $DotNetExe test $file -c $Configuration --logger trx
        else
            # Can remove this when: https://github.com/dotnet/sdk/issues/335 is resolved
            echo "Skipping " $file
            echo "--- Desktop .NET Framework testing is not currently supported on Unix."
        fi
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
