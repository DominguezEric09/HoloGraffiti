#!/usr/bin/env bash

set -euo pipefail
export LC_ALL=C

cd "$(git rev-parse --show-toplevel)"

if [ ! -f ProjectSettings/ProjectVersion.txt ]; then
  echo "No Unity project at the repository root yet, skipping."
  exit 0
fi

tracked=$(git ls-files Assets)
ignored='(^|/)\.|~(/|$)'
metas=$(grep '\.meta$' <<<"$tracked" | sed 's/\.meta$//' | grep -Ev "$ignored" | sort -u || true)
assets=$(grep -v '\.meta$' <<<"$tracked" \
  | awk -F/ '{ print; p = $1; for (i = 2; i < NF; i++) { p = p "/" $i; print p } }' \
  | grep -Ev "$ignored" | sort -u || true)
missing=$(comm -23 <(echo "$assets") <(echo "$metas"))
orphaned=$(comm -13 <(echo "$assets") <(echo "$metas"))
status=0

if [ -n "$missing" ]; then
  echo "::error::These assets have no .meta file. Commit the .meta Unity created next to each one:"
  echo "$missing" | sed 's/^/  /'
  status=1
fi

if [ -n "$orphaned" ]; then
  echo "::error::These .meta files have no asset. Delete them, or commit the asset they belong to:"
  echo "$orphaned" | sed 's/^/  /; s/$/.meta/'
  status=1
fi

[ "$status" -eq 0 ] && echo "Every asset under Assets/ has a matching .meta file."
exit "$status"
