#!/usr/bin/env bash
#
# Run integration tests against a CW Agent.
# 
# usage:
#   export AWS_ACCESS_KEY_ID=
#   export AWS_SECRET_ACCESS_KEY=
#   export AWS_REGION=us-west-2
#   ./start-agent.sh

scripts_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" &>/dev/null && pwd -P)
source "$scripts_dir"/utils.sh

NUGET_API_KEY=""

function assume_role_and_get_key() {
  ROLE_ARN=$1
  OUTPUT_PROFILE="publishing-profile"
  echo "Assuming role $ROLE_ARN"
  sts=$(aws sts assume-role \
    --role-arn "$ROLE_ARN" \
    --role-session-name "$OUTPUT_PROFILE" \
    --query 'Credentials.[AccessKeyId,SecretAccessKey,SessionToken]' \
    --output text)
  check_exit
  sts=($sts)
  aws configure set aws_access_key_id "${sts[0]}" --profile "$OUTPUT_PROFILE"
  aws configure set aws_secret_access_key "${sts[1]}" --profile "$OUTPUT_PROFILE"
  aws configure set aws_session_token "${sts[2]}" --profile "$OUTPUT_PROFILE"
  echo "Credentials stored in the profile named $OUTPUT_PROFILE"

  NUGET_API_KEY=$(aws secretsmanager \
    --profile publishing-profile \
    get-secret-value \
    --secret-id "$SECRET_ARN" \
    | jq '.SecretString | fromjson.Key' | tr -d '"')
  check_exit
}

# publish <package-name>
function publish() {
    rootdir=$(git rev-parse --show-toplevel)
    rootdir=${rootdir:-$(pwd)} # in case we are not in a git repository (Code Pipelines)

    package_dir="$rootdir/src/$1"
    output_dir="$package_dir/bin/Release"

    pushd $package_dir
        dotnet pack -c Release
        pushd $output_dir
            dotnet nuget push *.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
        popd
    popd
}

assume_role_and_get_key "$ROLE_ARN"

validate "$NUGET_API_KEY" "NUGET_API_KEY"
validate "$CODEBUILD_BUILD_NUMBER" "CODEBUILD_BUILD_NUMBER"

publish Amazon.CloudWatch.EMF
publish Amazon.CloudWatch.EMF.Web
