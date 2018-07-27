using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerChunk {

	// Cache of map handler
	private MapHandler m_mapHandlerScript;
	// Chunk dimensions for array
	private int m_chunkDimension;

	// A heightmap to handle the working chunk vert heights
	public float[,] m_chunkHeights;

	public LayerChunk(int dim){
		// Initialize the array heights
		m_chunkHeights = new float[dim, dim];
		for (int i = 0; i < dim; ++i) {
			for (int j = 0; j < dim; ++j) {
				m_chunkHeights [i, j] = 0;
			}
		}
	}

	// Set a value in the heights array
	public void SetHeight (int x, int y, float heightValue) {

		// Set the working height value
		m_chunkHeights [x, y] = heightValue;
	}

	// Set a value in the heights array
	public float GetHeight (int x, int y) {
		return m_chunkHeights [x, y];
	}
}
