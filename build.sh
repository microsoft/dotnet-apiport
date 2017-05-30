#!/usr/bin/env bash
CONFIGURATION=Debug

usage() { echo "Usage: build.sh [-c|--configuration <Debug|Release>]"; }

build() {
    echo "Building ApiPort... Configuration: "$CONFIGURATION
    pushd src/ApiPort > /dev/null
    dotnet restore
    dotnet build -f netcoreapp1.0 -c $CONFIGURATION
    popd > /dev/null
}
    popd
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

echo "Finished!"
