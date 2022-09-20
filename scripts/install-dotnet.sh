#!/usr/bin/env bash
# print command before they are executed without expanding variables
set -v

# install the latest patch version of 6.0
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -c 6.0
dotnet --version
dotnet nuget --version
