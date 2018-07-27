using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapHandler : MonoBehaviour {
	//----------------------------------- MANAGER -----------------------------------
	// I am a manager, widget, attribute container
	private Widget m_allLinkedAttributes;

	//----------------------------------- Public Members -----------------------------------
	public GameObject[,] m_mapChunks;

	// Basic material to apply to the chunks
	public Material m_basicTerrainMaterial;

	// Based on where the camera is, how far offset from the origin am I?
	public Vector2 m_mapOffset;

	// Offset modifier for interal chunk coordinates (forced but little alternative)
	public int oddMod;

	// Offset modifier for interal chunk coordinates (forced but little alternative)
	public int oddModChunk;


	//----------------------------------- Private Members -----------------------------------
	// The stack of modifying arrays, results of tool/input operations. Stored as a dynamic stack
	private MapModifier[] m_modifierHistoryStack;


	//----------------------------------- Methods -----------------------------------
	// Use this for initialization	
	void Start () {
		// New attribute list (widget)
		m_allLinkedAttributes = gameObject.AddComponent<Widget>();
		// Add all used attributes
		m_allLinkedAttributes.AddAttribute <int> ("ChunkDimensions", 10);
		m_allLinkedAttributes.AddAttribute <int> ("ChunksToLoadDimensions", 10);

		//m_allLinkedAttributes.AddAttribute <Vector2> ("NoiseOffset", Vector2.zero);

		SpawnNewTerrain ();
	}

	// Based on the loaded chunks and chunk resolution, what are the final map dimensions (for eas of use)
	public int GetCombinedMapDimensions() {
		return (m_allLinkedAttributes.GetAttribute <int> ("ChunkDimensions") * m_allLinkedAttributes.GetAttribute <int> ("ChunksToLoadDimensions")) - m_allLinkedAttributes.GetAttribute <int> ("ChunksToLoadDimensions");
	}
		

	void SpawnNewTerrain(){
		int cd = m_allLinkedAttributes.GetAttribute <int> ("ChunkDimensions");
		int ctld = m_allLinkedAttributes.GetAttribute <int> ("ChunksToLoadDimensions");

		// if odd, shift
		oddMod = (cd % 2 == 0) ? 1 : 0;
		oddModChunk = (ctld % 2 == 0) ? 0 : 1;

		if (m_mapChunks != null && m_mapChunks.Length > 0) {
			// Clean up old
			foreach (GameObject oldChunk in m_mapChunks) {
				GameObject.Destroy (oldChunk);
			}
		}

		// Initialize chunk array
		m_mapChunks = new GameObject[m_allLinkedAttributes.GetAttribute <int> ("ChunksToLoadDimensions"), m_allLinkedAttributes.GetAttribute <int> ("ChunksToLoadDimensions")];


		// For each new game object in array, generate this chunk
		for (int i = 0; i < ctld; ++i) {
			for (int j = 0; j < ctld; ++j) {

				// Add Chunk script
				GameObject newObject = new GameObject();

				newObject.name = "Chunk:[" + i + ", " + j + "]";

				// Init the new chunk
				newObject.AddComponent<Chunk> ();
				// Add to array
				m_mapChunks [i,j] = newObject;

				newObject.transform.position = new Vector3 (
					(i * (cd - 1)) - (((float)ctld/2) * (cd - 1)), 
					0, 
					(j * (cd - 1)) - (((float)ctld/2) * (cd - 1))
				);
			}
		}
	}

	public Layer PropogateChunks(List<Vector2> ModifiedChunks){
		Layer finalLayer = new Layer (LayerFunctionType.Normal, 0);

		// Temp array to hold working chunks
		LayerChunk[] propogated = new LayerChunk[ModifiedChunks.Count];

		int cd = m_allLinkedAttributes.GetAttribute<int> ("ChunkDimensions");
		int ctld = m_allLinkedAttributes.GetAttribute<int> ("ChunksToLoadDimensions");

		for (int l = 0; l < ModifiedChunks.Count; ++l) {
			propogated [l] = new LayerChunk (cd);
		}
			
		// Now layer write has finished, propogate values down stack
		foreach (Layer layer in GameObject.FindGameObjectWithTag ("LayerManager").GetComponent<Widget>().GetAttribute<List<Layer>>("Layers")) {
			// For each of the modified chunks per layers
			for (int l = 0; l < propogated.Length; ++l) {									
				// For every height in the modified chunks
				for (int i = 0; i < cd; ++ i) {
					for (int j = 0; j < cd; ++ j) {
						// Pass the last working coord through the process function of the corresponding layer
						propogated [l].SetHeight (i, j, layer.m_layerFuntion.ProcessCoordinate (layer.m_layerChunks[(int)ModifiedChunks[l].x, (int)ModifiedChunks[l].y], layer.m_layerOpacity, i, j, propogated [l].GetHeight(i, j)));
					}
				}
			}
		}

		// Now sub brush step has finished (mouse button still down but changes this tick have finished, update all impacted chunks)
		for (int l = 0; l < ModifiedChunks.Count; ++l) {
			m_mapChunks [(int)ModifiedChunks[l].x, (int)ModifiedChunks[l].y].GetComponent<Chunk> ().UpdateMesh (propogated[l]);
		}

		if (ModifiedChunks.Count == ctld * ctld) {
			for (int i = 0; i < ctld; ++i) {
				for (int j = 0; j < ctld; ++j) {
					finalLayer.m_layerChunks [i, j] = propogated [(i * ctld) + j];
				}
			}
		}

		GameObject.FindGameObjectWithTag("LayerManager").GetComponent<LayerManager>().ProjectSelectedLayer ();

		return finalLayer;
	}

	public Transform[] GetChunkTransforms () {
		Transform[] transforms = new Transform[m_mapChunks.Length];

		int count = 0;

		foreach (GameObject chunk in m_mapChunks) {
			transforms[count] = chunk.transform;
			count++;
		}

		return transforms;
	}

	void Update (){
		/*
		int cd = m_allLinkedAttributes.GetAttribute <int> ("ChunkDimensions");
		int ctld = m_allLinkedAttributes.GetAttribute <int> ("ChunksToLoadDimensions");

		Debug.DrawLine (Vector3.zero, Vector3.up, Color.red);

		// For each new game object in array, generate this chunk
		for (int i = 0; i < ctld; ++i) {
			for (int j = 0; j < ctld; ++j) {
				Vector3 newPos = new Vector3 (
					(i * (cd - 1)) - (((float)ctld/2) * (cd - 1)), 
					0, 
					(j * (cd - 1)) - (((float)ctld/2) * (cd - 1))
				);

				Debug.DrawLine (newPos, newPos + Vector3.up, Color.blue);
				Debug.DrawLine (newPos, newPos + Vector3.right * (cd - 1), Color.cyan);
				Debug.DrawLine (newPos, newPos + Vector3.forward * (cd - 1), Color.cyan);
			}
		}
		*/
	}
}
