#!/bin/bash

PLATFORM="UNKNOWN"

for i in "$@"
do
case $i in
    -w|--windows)
	PLATFORM="WINDOWS"
    shift # past argument=value
    ;;
    -l|--linus)
	PLATFORM="LINUX"
    shift # past argument with no value
    ;;
    -m|--mac)
	PLATFORM="MAC"
    shift # past argument with no value
    ;;
    *)
            # unknown option
    ;;
esac
done

if [[ ${PLATFORM} == "UNKNOWN" ]]; then
	echo "Please give -w, -l or -m to specify the platform (for Windows, Linux or Mac)";
	exit 1;
fi

# Remove platform-specific files:
git rm -r Assets/ThirdParty/PlatformSpecific/Windows/ 2> /dev/null
git rm Assets/ThirdParty/PlatformSpecific/Windows.meta 2> /dev/null
git rm -r Assets/ThirdParty/PlatformSpecific/Linux 2> /dev/null
git rm Assets/ThirdParty/PlatformSpecific/Linux.meta 2> /dev/null
git rm -r Assets/ThirdParty/PlatformSpecific/MaxOS 2> /dev/null
git rm Assets/ThirdParty/PlatformSpecific/MaxOS.meta 2> /dev/null

# Copy the platform-specific folders to the Assets directory,
# depending on the OS you're on:
mkdir -p Assets/ThirdParty/PlatformSpecific/
if [[ ${PLATFORM} == "LINUX" ]]; then
	echo "Setting up files for: Linux"
	echo "	Copying Linux libraries to Asset folder."
	cp -r PlatformSpecificPlugins/Linux* Assets/ThirdParty/PlatformSpecific/
elif [[ ${PLATFORM} == "MAC" ]]; then
	echo "Setting up files for: MacOS"
	echo " WARNING: MacOS is currently not supported!"
else
	echo "Setting up files for: Windows"
	echo "	Copying Windows libraries to Asset folder."
	cp -r PlatformSpecificPlugins/Windows* Assets/ThirdParty/PlatformSpecific/
fi

git add Assets/ThirdParty/PlatformSpecific/*
git commit -m "Swapped platform specific DLLs to ${PLATFORM}"
