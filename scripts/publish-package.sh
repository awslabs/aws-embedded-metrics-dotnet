#!/usr/bin/env bash
#
# Run integration tests against a CW Agent.
# 
# usage:
#   export AWS_ACCESS_KEY_ID=
#   export AWS_SECRET_ACCESS_KEY=
#   export AWS_REGION=us-west-2
#   ./start-agent.sh

source ./utils.sh

# publish <package-name>
function publish() {
    rootdir=$(git rev-parse --show-toplevel)
    rootdir=${rootdir:-$(pwd)} # in case we are not in a git repository (Code Pipelines)

    package_dir="$rootdir/src/$1"
    output_dir="$package_dir/bin/Release"

    pushd $package_dir
        dotnet pack -c Release --version-suffix "alpha-$CODEBUILD_BUILD_NUMBER"

        pushd $output_dir
            dotnet nuget push *.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
        popd
    popd
}

validate "$NUGET_API_KEY" "NUGET_API_KEY"
validate "$CODEBUILD_BUILD_NUMBER" "CODEBUILD_BUILD_NUMBER"

publish Amazon.CloudWatch.EMF
publish Amazon.CloudWatch.EMF.Web
