#!/usr/bin/env bash
export EnableNuGetPackageRestore="true"
#under some systems, keypairs don't have the right
#file attributes, causing cryptographic exceptions. 
#This sets them properly.
chmod 600 $HOME/.config/.mono/keypairs/
xbuild Build/Build.proj /p:Configuration="Mono Release" /t:GoMono
