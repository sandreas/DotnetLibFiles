#!/bin/sh
dotnet pack --configuration Release --include-symbols

# dotnet nuget push *.nupkg -s https://api.nuget.org/v3/index.json -k "${NUGET_API_KEY}"