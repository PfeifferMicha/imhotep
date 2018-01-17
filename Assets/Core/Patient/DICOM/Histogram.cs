using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Histogram {

	List<double> values = new List<double>();
	long[] bins;

	bool textureNeedsToBeRegenerated = true;
	bool needsToBeResorted = true;
	Texture2D texture;

	double binSize;
	double minValue = 0;
	double maxValue = 1;
	int numOfBins;
	long max = 0;

	public Histogram() {
	}

	public void addValue( double val )
	{
		values.Add (val);
		textureNeedsToBeRegenerated = true;	// Texture needs to be re-generated.
		needsToBeResorted = true;	// Texture needs to be re-generated.
	}

	public Texture2D asTexture()
	{
		if (textureNeedsToBeRegenerated) {
			generateTexture ();
		}
		return texture;
	}

	public void setMinMaxPixelValues( float min, float max )
	{
		minValue = min;
		maxValue = max;
	}

	public void sortIntoBins( int numOfBins )
	{
		bins = new long[numOfBins];
		binSize = (maxValue - minValue)/(double)numOfBins;
		this.numOfBins = numOfBins;

		for (int i = 1; i < values.Count; i++) {
			if (values[i] < minValue || values[i] > maxValue)
				continue;
			int binIndex = (int)Math.Floor ((values[i] - minValue) / binSize + 0.5);
			if (binIndex >= 0 && binIndex < numOfBins)
				bins [binIndex]++;
		}

		// search the maximum value:
		max = 1;
		for (int i = 0; i < numOfBins; i++) {
			if (bins [i] > max)
				max = bins [i];
		}

		needsToBeResorted = false;
	}

	public void generateTexture()
	{
		if (needsToBeResorted)
			sortIntoBins (200);		// Default values

		int texHeight = 100;

		texture = new Texture2D (numOfBins, texHeight, TextureFormat.ARGB32, false, true);

		for (int i = 0; i < numOfBins; i++) {
			long height = (long)(texHeight-1) * bins [i] / max;
			//Debug.Log (i + " "  + bins [i]);
			if (bins [i] > 0 && height == 0)
				height = 1;
			for (int y = 0; y < height; y++)
				texture.SetPixel (i, y, Color.white);
			for (int y = (int)height; y < texHeight; y++) {
				texture.SetPixel (i, y, Color.black);
			}
		}
		texture.Apply ();
		textureNeedsToBeRegenerated = false;

		// Save to file for debugging purposes:
		/*byte[] bytes = texture.EncodeToPNG ();
		System.IO.File.WriteAllBytes ("histogram.png", bytes);*/
	}

}
