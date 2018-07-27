using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivideLayer : LayerFunction {
	public override float ProcessCoordinate (LayerChunk layerChunk, float layerOpacity, int x, int y, float weight)
	{
		float getWeight = layerChunk.GetHeight (x, y);

		// Div weight by layer weight, check for 0 weight or 0 opacity to avoid division by 0
		return ((getWeight != 0 && layerOpacity != 0) ? weight / (getWeight * layerOpacity) : 0);
	}
}
