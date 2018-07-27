using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerFunction {
	// For an x, y coordinate in a modifier array, do processing
	public virtual float ProcessCoordinate (LayerChunk layer, float layerOpacity, int x, int y, float weight) {
		// If base is called, just bomb 0
		Debug.LogError("Base LayerFunction, process coordinate is virtual base method.");
		return 0;
	}
}
