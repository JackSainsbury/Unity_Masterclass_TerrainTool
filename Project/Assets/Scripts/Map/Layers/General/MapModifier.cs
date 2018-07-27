using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapModifier : MonoBehaviour {

	// The worldspace coordinates of the modifying array
	private Vector2 m_modifierOrigin;

	private Vector2 m_modifierDimensions;

	// The modifier array
	private float[,] m_modifyArea;

	// Initialize the modifier, generates the array with 0s
	public void initModifier (Vector2 ArrayDimensions) {
		// Initialize the array
		m_modifyArea = new float[((int)ArrayDimensions.x), ((int)ArrayDimensions.y)];

		for (int i = 0; i < (int)ArrayDimensions.x; ++i) {
			for (int j = 0; j < (int)ArrayDimensions.y; ++j) {
				m_modifyArea [i, j] = 0;
			}
		}
	}

	// Initialize the modifier, generates the array with supplied array
	public void initModifier (Vector2 ArrayDimensions, float[, ] initArray) {
		if (ArrayDimensions.x * ArrayDimensions.y != initArray.Length) {
			Debug.LogError ("Supplied supposed dimensions !=  Actual supplied modifier array dimensions.");
		} else {

			// Initialize the array
			m_modifyArea = new float[((int)ArrayDimensions.x), ((int)ArrayDimensions.y)];

			for (int i = 0; i < (int)ArrayDimensions.x; ++i) {
				for (int j = 0; j < (int)ArrayDimensions.y; ++j) {
					m_modifyArea [i, j] = initArray [i, j];
				}
			}
		}
	}

	// Set a height value in this modifier array
	public void setModifierArrayValue (int x, int y, float value) {
		m_modifyArea [x, y] = value;
	}

	// Get a value from modifier array heights
	public float getModifierArrayValue (int x, int y) {
		return m_modifyArea [x, y];
	}
}
