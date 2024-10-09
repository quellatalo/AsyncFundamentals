#!/usr/bin/env sh
dotnet tool restore
solution="AsyncFundamentals.sln"
all="--all"

if [ "$1" = "$all" ]; then
  echo "reformat all"
  dotnet tool run jb cleanupcode ${solution}
else
  echo "reformat staged"

  # Find new & modified staged .cs files
  files=$(git diff --cached --name-only --diff-filter=AM  --exclude='[bin,obj,TestResults,dist,_inspectcode,node_modules]/*' | grep '\.cs$')
  if [ -z "$files" ]; then
    echo "no staged .cs files to reformat"
  else
    # shellcheck disable=SC2086 # Double quote to prevent globbing and word splitting
    dotnet tool run jb cleanupcode ${solution} --include="$(echo $files | tr ' ' ';')"
  fi
fi
