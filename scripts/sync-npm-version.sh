#!/usr/bin/env bash
set -euo pipefail

# Sync the npm package version to the git tag that triggered the release.
# Called from the release workflow after dotnet-releaser finishes.
#
# Expects GITHUB_REF_NAME to be set (e.g., "v1.2.3").

VERSION="${GITHUB_REF_NAME#v}"

if [ -z "$VERSION" ]; then
  echo "ERROR: Could not determine version from GITHUB_REF_NAME=$GITHUB_REF_NAME"
  exit 1
fi

echo "Syncing npm version to $VERSION"

cd js/metano-runtime

# Update package.json version field
jq --arg v "$VERSION" '.version = $v' package.json > tmp.json && mv tmp.json package.json
echo "Updated package.json to version $VERSION"

# Publish to npm (--access public for unscoped packages)
npm publish --access public
echo "Published metano-runtime@$VERSION to npm"
