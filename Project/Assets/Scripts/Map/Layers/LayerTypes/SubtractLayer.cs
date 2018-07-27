using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubtractLayer : LayerFunction {
	public override float ProcessCoordinate (LayerChunk layerChunk, float layerOpacity, int x, int y, float weight)
	{
		// Simple addition
		return weight - (layerChunk.GetHeight(x, y) * layerOpacity);
	}
}
