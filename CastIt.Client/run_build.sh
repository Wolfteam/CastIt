#!/bin/bash
set -e

sh run_build_android.sh
sh run_build_ios.sh
sh run_build_macos.sh