#!/bin/bash

if [ ! -f .paket/paket.bootstrapper.exe ]; then
    curl -L -o .paket/paket.bootstrapper.exe https://github.com/fsprojects/Paket/releases/download/0.35.0/paket.bootstrapper.exe
fi

mono .paket/paket.bootstrapper.exe
exit_code=$?
if [ $exit_code -ne 0 ]; then
exit $exit_code
fi

mono .paket/paket.exe restore
exit_code=$?
if [ $exit_code -ne 0 ]; then
exit $exit_code
fi

mono packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx 
