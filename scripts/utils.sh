#!/usr/bin/env zsh

function check_exit() {
    last_exit_code=$?

    if [ $last_exit_code -ne 0 ]; 
    then 
        echo "Last command failed with exit code: $last_exit_code."
        echo "Exiting."
        exit $last_exit_code; 
    fi
}

RED='\033[0;31m'
NC='\033[0m' # No Color

# validates that the provided parameter is set
function validate() {
    if [[ -z "$1" ]]; then
        echo -e "$RED $2 is not set $1 $NC"
        exit 1
    fi
}