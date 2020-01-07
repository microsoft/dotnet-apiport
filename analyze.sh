#!/bin/sh
curp=$(pwd)
analyzef=$1
help="analyze.sh <dll-or-exe-path-to-analyze> [other-args,see ApiPort docs]"
if [ -z "$analyzef"  ]; then
	echo "Must provide a file to analyze">&2
	echo "$help">&2
	exit 0
fi
if [ ! -f "$analyzef" ]; then
	echo "error, can't find file at: $analyzef">&2
	exit 2
fi
shift
repname=$(basename "$analyzef")
defrep=Json
repfn="$repname.$(echo "$defrep"|tr '[:upper:]' '[:lower:]')"
repp="$curp/$repfn"
cd bin/Debug/ApiPort/netcoreapp2.1/
dotnet ApiPort.dll analyze -f "$analyzef" -o "$repp" -r "$defrep" "$@"
