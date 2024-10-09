#!/usr/bin/env sh
find . \( -name obj -o -name bin -o -name TestResults -o -name node_modules -o -name dist -o -name .vs \) -exec rm -rvf {} +
