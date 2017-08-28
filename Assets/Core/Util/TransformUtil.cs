using UnityEngine;
using System.Collections;

public class TransformUtil {

	/*! Transform a bounding box from the transform's local space to the world space.
	 * \note Often, you'll need to transform.parent as the first parameter to this function,
	 * 		for example when you try to calculate a mesh's bounding box in the local coordinate
	 *		system of another transform.*/
	public static Bounds TransformBounds( Transform trans, Bounds localBounds )
	{
		Vector3 center = trans.TransformPoint (localBounds.center);
		Vector3 extents = trans.TransformVector (localBounds.extents);
		return new Bounds (center, extents * 2f);
	}
	/*! Transform a bounding box from the transform's local space to the world space.
	 * \note Often, you'll need to transform.parent as the first parameter to this function,
	 * 		for example when you try to calculate a mesh's bounding box in the local coordinate
	 *		system of another transform.*/
	public static Bounds InverseTransformBounds( Transform trans, Bounds worldBounds )
	{
		Vector3 center = trans.InverseTransformPoint (worldBounds.center);
		Vector3 extents = trans.InverseTransformVector (worldBounds.extents);
		return new Bounds (center, extents * 2f);
	}
}
