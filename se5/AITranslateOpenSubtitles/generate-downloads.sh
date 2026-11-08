#!/usr/bin/env bash
# Helper script to generate release download URLs for se5-plugins.json
# Usage: ./generate-downloads.sh v1.0.0

VERSION="${1:-v0.2.2}"
REPO="iceman1010/se5-ai-translator"
BASE_URL="https://github.com/${REPO}/releases/download/${VERSION}"

cat << EOF
"downloads": {
  "win-x64":     "${BASE_URL}/se-ai-translator-windows-x86_64.zip",
  "win-arm64":   "${BASE_URL}/se-ai-translator-windows-arm64.zip",
  "linux-x64":   "${BASE_URL}/se-ai-translator-linux-x86_64.tar.gz",
  "linux-arm64": "${BASE_URL}/se-ai-translator-linux-x86_64.tar.gz",
  "osx-x64":     "${BASE_URL}/se-ai-translator-macos-x86_64.tar.gz",
  "osx-arm64":   "${BASE_URL}/se-ai-translator-macos-aarch64.tar.gz"
}
EOF
