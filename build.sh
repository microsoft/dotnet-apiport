#!/usr/bin/env bash
set -e
CONFIGURATION=Debug

usage() { echo "Usage: build.sh [-c|--configuration <Debug|Release>]"; }

build() {
    echo "Building ApiPort... Configuration: "$CONFIGURATION
    pushd src/ApiPort > /dev/null
    dotnet restore
    dotnet build -f netcoreapp1.0 -c $CONFIGURATION
    popd > /dev/null
}

runTest() {
    local PROJECT=$1

    pushd $PROJECT > /dev/null

    ls *.csproj | while read file
    do
        if awk -F: '/<TargetFramework>netcoreapp1\.[0-9]<\/TargetFramework>/ { found = 1 } END { if (found == 1) { exit 0 } else { exit 1 } }' $file; then
            echo "Testing " $file
            dotnet restore
            dotnet test -c $CONFIGURATION
        else
            # Can remove this when: https://github.com/dotnet/sdk/issues/335 is resolved
            echo "Skipping " $file
            echo "--- Desktop .NET Framework testing is not currently supported on Unix."
        fi
    done

    popd > /dev/null
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
        CONFIGURATION="$2"
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

if [[ "$CONFIGURATION" != "Debug" && "$CONFIGURATION" != "Release" ]]; then
    echo "ERROR: Supported configuration types are Debug or Release.  Invalid configuration: "$CONFIGURATION
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
