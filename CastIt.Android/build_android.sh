#!/bin/bash
set -e

echo 'Cleaning project...'
flutter clean

echo 'Retrieving packages...'
flutter pub get

echo 'Deleting conflicting outputs...'
flutter pub run build_runner build --delete-conflicting-outputs

echo 'Creating android bundle...'
flutter build appbundle