#!/usr/bin/env bash
#
# Run integration tests against a CW Agent.
# 
# usage:
#   export AWS_ACCESS_KEY_ID=
#   export AWS_SECRET_ACCESS_KEY=
#   export AWS_REGION=us-west-2
#   ./start-agent.sh

RED='\033[0;31m'
NC='\033[0m' # No Color

# validates that the provided parameter is set
function validate() {
    if [[ -z "$1" ]]; then
        echo -e "$RED $2 is not set $1 $NC"
        exit 1
    fi
}

validate "$NUGET_API_KEY" "NUGET_API_KEY"
validate "$CODEBUILD_BUILD_NUMBER" "CODEBUILD_BUILD_NUMBER"

rootdir=$(git rev-parse --show-toplevel)
rootdir=${rootdir:-$(pwd)} # in case we are not in a git repository (Code Pipelines)

package_dir="$rootdir/Amazon.CloudWatch.EMF"
output_dir="$package_dir/bin/Release"

pushd $package_dir
    dotnet pack -c Release --version-suffix "alpha-$CODEBUILD_BUILD_NUMBER"
    pushd $output_dir
    dotnet nuget push *.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
popd