using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AverageLayer : LayerFunction {
	public override float ProcessCoordinate (LayerChunk layerChunk, float layerOpacity, int x, int y, float weight)
	{
		// Halfway between weights
		return weight + (((weight - layerChunk.GetHeight(x, y)) / 2) * layerOpacity) ;
	}
}
