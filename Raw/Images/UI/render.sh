#!/bin/bash
# Renders all *.svg files as *.png.

resolution=150;

shopt -s nullglob
containsElement () {
	local e
	for e in "${@:2}"; do [[ "$e" == "$1" ]] && return 1; done
	return 0
}

echo -----------------------------------------------------
echo Saving images:
echo -----------------------------------------------------

for infile in *.svg; do
	containsElement $infile $@ 
	if [[ $# == 0 || $? == 1 ]]; then
		outfile=../../../Assets/UI/Images/${infile%.*}.png
		inkscape -f $infile -C -d $resolution --export-background=#000000 --export-background-opacity=0 -e $outfile
	fi
done

mkdir -p ../../../Assets/UI/Images/Icons
cd Icons
for infile in *.svg; do
	containsElement $infile $@ 
	if [[ $# == 0 || $? == 1 ]]; then
		outfile=../../../../Assets/UI/Images/Icons/${infile%.*}.png
		echo $infile "=>" $outfile
		inkscape -f $infile -C -d $resolution --export-background=#000000 --export-background-opacity=0 -e $outfile
	fi
done
cd ..
