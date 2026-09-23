#!/bin/sh
set -eu
# Isolated build outputs: never mix host and Linux bin/obj directories.
workspace=$(mktemp -d)
cd /repository
tar --exclude=.git --exclude=bin --exclude=obj --exclude=.local --exclude=research/results --exclude=./research/results -cf - . | tar -xf - -C "$workspace"
cd "$workspace"
ln -s /repository/.git .git
mkdir -p research
ln -s /results research/results
exec "$@"
