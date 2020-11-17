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

validate "$AWS_ACCESS_KEY_ID" "AWS_ACCESS_KEY_ID"
validate "$AWS_SECRET_ACCESS_KEY" "AWS_SECRET_ACCESS_KEY"
validate "$AWS_REGION" "AWS_REGION"

rootdir=$(git rev-parse --show-toplevel)
rootdir=${rootdir:-$(pwd)} # in case we are not in a git repository (Code Pipelines)

cwagent_dir="$rootdir/scripts/cwagent"
tempfile="$cwagent_dir/.temp"

###################################
# Configure and start the agent
###################################

pushd $cwagent_dir
echo "[AmazonCloudWatchAgent]
aws_access_key_id = $AWS_ACCESS_KEY_ID
aws_secret_access_key = $AWS_SECRET_ACCESS_KEY
" > ./.aws/credentials

echo "[profile AmazonCloudWatchAgent]
region = $AWS_REGION
" > ./.aws/config

docker build -t agent:latest .
docker run  -p 25888:25888/udp -p 25888:25888/tcp  \
    -e AWS_REGION=$AWS_REGION \
    -e AWS_ACCESS_KEY_ID=$AWS_ACCESS_KEY_ID \
    -e AWS_SECRET_ACCESS_KEY=$AWS_SECRET_ACCESS_KEY \
    agent:latest &> $tempfile &
popd