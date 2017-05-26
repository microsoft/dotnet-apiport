#!/usr/bin/env bash
CONFIGURATION=Debug

usage() { echo "Usage: build.sh [-c|--configuration <Debug|Release>]"; }

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

echo "Building ApiPort... Configuration: "$CONFIGURATION

if ! hash dotnet 2>/dev/null; then
    echo "ERROR: Please install dotnet SDK from https://microsoft.com/net/core."
    exit 2
fi

pushd src/ApiPort
dotnet restore
dotnet build -f netcoreapp1.0 -c $CONFIGURATION
popd

echo "Finished!"