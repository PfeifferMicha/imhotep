using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class View
{
	public View( ViewJson vj )
	{
		name = vj.name;
		orientation = new Quaternion( (float)vj.orientation[0],(float) vj.orientation[1], (float)vj.orientation[2], (float)vj.orientation[3] );
		scale = new Vector3( (float)vj.scale[0], (float)vj.scale[1], (float)vj.scale[2] );
		opacities = new Dictionary<string, double>();
		if( vj.opacityKeys.Count == vj.opacityValues.Count )
		{
			int numEntries = vj.opacityKeys.Count;
			for( int i = 0; i < numEntries; i ++ )
			{
				string meshName = vj.opacityKeys [i];
				// Remove a possible "ME" at the beginning of the mesh name (for backward compatibility):
				if( meshName.Substring( 0, 2 ) == "ME" )
					meshName = meshName.Substring (2, meshName.Length - 2);
				opacities.Add( meshName, vj.opacityValues[i] );
			}
		} else {
			throw new System.Exception("Number of opacity values incorrect. Number of opacity keys and number of opacities must match!");
		}
	}
	public View() {
		opacities = new Dictionary<string, double>();
	}
	public string name { get; set; }
	public Quaternion orientation { get; set; }
	public Vector3 scale { get; set; }
	public Dictionary<string, double> opacities { get; set; }
}

public class ViewJson
{
	public ViewJson() {
		orientation = new double[4];
		scale = new double[3];
		//opacityKeys = new string[30];
		//opacities = new double[30];
		//opacities = new Dictionary<string, double>();
		opacityKeys = new List<string>();
		opacityValues = new List<double>();
	}
	public ViewJson( View v )
	{
		name = v.name;
		orientation = new double[4];
		orientation[0] = (double)v.orientation.x;
		orientation[1] = (double)v.orientation.y;
		orientation[2] = (double)v.orientation.z;
		orientation[3] = (double)v.orientation.w;
		scale = new double[3];
		scale[0] = (double)v.scale.x;
		scale[1] = (double)v.scale.y;
		scale[2] = (double)v.scale.z;
		opacityKeys = v.opacities.Keys.ToList();
		opacityValues = v.opacities.Values.ToList();
		/*opacityKeys = new string[v.opacities.Count];
			opacities = new double[v.opacities.Count];
			uint i = 0;
			foreach(KeyValuePair<string, float> entry in v.opacities)
			{
				opacityKeys[i] = entry.Key;
				opacities[i] = (double)entry.Value;
				i ++;
			}*/
	}
	public string name { get; set; }
	public double[] orientation { get; set; }
	public double[] scale { get; set; }
	//public string[] opacityKeys;
	//public double[] opacities;
	public List<string> opacityKeys { get; set; }
	public List<double> opacityValues { get; set; }
}