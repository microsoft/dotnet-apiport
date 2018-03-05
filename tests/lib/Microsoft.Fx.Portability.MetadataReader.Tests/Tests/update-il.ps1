# This script will recompile all IL test cases since we cannot compile them with Roslyn like the other examples
#
# NOTE: ilasm does not produce deterministic results, so even if no change to the IL occurs, a change in the
# assembly will be seen

if(!(Get-Command ilasm -ErrorAction Ignore))
{
    Write-Warning "Must be run in an environment with ilasm"
    return
}

Get-ChildItem *.il `
    | ForEach-Object { ilasm $_.FullName /dll }