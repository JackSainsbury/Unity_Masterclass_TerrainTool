using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

	// Cache of map handler
	private MapHandler m_mapHandlerScript;
	// Chunk dimensions for array
	private int m_chunkDimension;

	// A heightmap to handle the working chunk vert heights
	private float[,] m_chunkHeights;

	public void initChunk(){
		// Cache the map handler script
		m_mapHandlerScript = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ();
		// get the chunk dimensions
		m_chunkDimension = m_mapHandlerScript.GetComponent<Widget>().GetAttribute <int> ("ChunkDimensions");
		// Initialize the array heights
		m_chunkHeights = new float[m_chunkDimension,m_chunkDimension];
	}

	// Set the chunk up for the 1st time (flat vert/tri grid) (even if the piece is imported, INIT then change heights)
	public void Awake () {
		initChunk ();

		gameObject.tag = "Terrain";
		// If I am awake, I have been added as a component and therefore should be drawn
		GenerateMesh ();
	}


	public void GenerateMesh(){
		// Add the now set mesh to the game object as a meshfilter
		gameObject.AddComponent<MeshFilter> ().mesh = new Mesh ();

		// Add and set up the new mesh renderer
		gameObject.AddComponent<MeshRenderer>().material = m_mapHandlerScript.m_basicTerrainMaterial;

		// The new array of verts we are generating
		int[] triangles = new int[(m_chunkDimension-1) * (m_chunkDimension-1) * 6];

		// Add a mesh collider used for raycast testing
		gameObject.AddComponent<MeshCollider>();

		UpdateMesh (new LayerChunk(m_chunkDimension));

		// Populate verts array
		for (int i = 0; i < m_chunkDimension; i++) {
			for (int j = 0; j < m_chunkDimension; j++) {
				if (i > 0 && j > 0) {
					int count = (((i - 1) * (m_chunkDimension - 1)) + (j - 1)) * 6;
					int countRaw = (i * m_chunkDimension) + j;
					// Tri 1
					triangles [count] = countRaw - (m_chunkDimension + 1);
					triangles [count + 1] = countRaw;
					triangles [count + 2] = countRaw - 1;
					// Tri 2
					triangles [count + 3] = countRaw - (m_chunkDimension + 1);
					triangles [count + 4] = countRaw - m_chunkDimension;
					triangles [count + 5] = countRaw;

				}
			}
		}

		// Set the verts on our new mesh, (HOPEFULLY WON'T CHANGE?) so no need to do this every tick
		gameObject.GetComponent<MeshFilter> ().mesh.triangles = triangles;

		// Add a mesh collider used for raycast testing
		gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter> ().mesh;

	}

	// Call at the end of a mod tick, now array values have been changed
	public void UpdateMesh (LayerChunk baseChunk) {
		//Push the final weights and update the mesh
		m_chunkHeights = baseChunk.m_chunkHeights;

		// The new array of verts we are generating
		Vector3[] vertices = new Vector3[m_chunkDimension * m_chunkDimension];
		// The new array of verts we are generating
		Vector3[] normals = new Vector3[m_chunkDimension * m_chunkDimension];


		// Populate verts array
		for (int i = 0; i < m_chunkDimension; ++i) {
			for (int j = 0; j < m_chunkDimension; ++j) {
				// Set the heights from the height map, with the basic grid layout
				vertices [(i * m_chunkDimension) + j] = new Vector3 (i, m_chunkHeights[i,j], j);
				normals  [(i * m_chunkDimension) + j] = Vector3.up;
			}
		}


		Vector2[] uvs = new Vector2[vertices.Length];

		for (int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = new Vector2(vertices[i].x / 10, vertices[i].z / 10);
		}
			
		// Set the verts on our new mesh
		gameObject.GetComponent<MeshFilter> ().mesh.vertices = vertices;
		// Set the uvs on our new mesh
		gameObject.GetComponent<MeshFilter> ().mesh.uv = uvs;
		// Set the verts on our new mesh
		gameObject.GetComponent<MeshFilter> ().mesh.normals = normals;
		// Update the mesh collider at tick time
		gameObject.GetComponent<MeshCollider> ().sharedMesh = gameObject.GetComponent<MeshFilter> ().mesh;
		// Recalculate the chunk bounds, for culling
		gameObject.GetComponent<MeshFilter> ().mesh.RecalculateBounds ();
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
