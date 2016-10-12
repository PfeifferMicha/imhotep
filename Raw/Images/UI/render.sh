#!/bin/bash
# dieses script rendert alle svg-dateien als *.png

resolution=100;

shopt -s nullglob
for infile in *.svg; do
	echo $infile
	outfile=../../../Assets/UI/Images/${infile%.*}.png
	echo out $outfile
	inkscape -f $infile -C -d $resolution --export-background=#000000 --export-background-opacity=0 -e $outfile
done

mkdir ../../../Assets/UI/Images/Icons -p
for infile in ./Icons/*.svg; do
	echo $infile
	outfile=../../../Assets/UI/Images/Icons/$(basename ${infile%.*}.png)
	echo out $outfile
	#outfile=../../../Assets/UI/Images/${infile%.*}.png
	inkscape -f $infile -C -d $resolution --export-background=#000000 --export-background-opacity=0 -e $outfile
done
