#!/usr/bin/env bash
set -e
Configuration=Debug

usage() { echo "Usage: build.sh [-c|--configuration <Debug|Release>]"; }

build() {
    echo "Building ApiPort... Configuration: "$Configuration
    pushd src/ApiPort > /dev/null
    dotnet restore
    dotnet build -f netcoreapp1.0 -c $Configuration
    popd > /dev/null
}

runTest() {
    ls $1/*.csproj | while read file
    do
        if awk -F: '/<TargetFramework>netcoreapp1\.[0-9]<\/TargetFramework>/ { found = 1 } END { if (found == 1) { exit 0 } else { exit 1 } }' $file; then
            echo "Testing " $file
            dotnet restore
            dotnet test $file -c $Configuration --logger trx
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

if ! hash dotnet 2>/dev/null; then
    echo "ERROR: Please install dotnet SDK from https://microsoft.com/net/core."
    exit 2
fi

build

find tests/ -type d -name "*\.Tests" | while read file
do
    runTest $file
done

echo "Finished!"
