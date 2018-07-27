using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using SFB;

public class ObjExporterScript : MonoBehaviour {

	private MapHandler m_mapHandlerScript;
	private CaveManager m_caveManagerScript;

	private static string _path;

	private static int vertexOffset = 0;
	private static int normalOffset = 0;
	private static int uvOffset = 0;

	void Start () {
		m_mapHandlerScript = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ();
		m_caveManagerScript = GameObject.FindGameObjectWithTag ("CaveManager").GetComponent<CaveManager> ();
	}

	void ExportWholeSelectionToSingle()
	{
		Transform[] caves = m_caveManagerScript.GetCaveTransforms ();
		Transform[] chunks = m_mapHandlerScript.GetChunkTransforms ();

		int exportedObjects = 0;

		ArrayList mfList = new ArrayList();

		for (int i = 0; i < caves.Length; i++)
		{
			Component[] meshfilter = caves[i].GetComponentsInChildren(typeof(MeshFilter));

			for (int m = 0; m < meshfilter.Length; m++)
			{
				exportedObjects++;
				mfList.Add(meshfilter[m]);
			}
		}

		for (int i = 0; i < chunks.Length; i++)
		{
			Component[] meshfilter = chunks[i].GetComponentsInChildren(typeof(MeshFilter));

			for (int m = 0; m < meshfilter.Length; m++)
			{
				exportedObjects++;
				mfList.Add(meshfilter[m]);
			}
		}

		if (exportedObjects > 0)
		{
			MeshFilter[] mf = new MeshFilter[mfList.Count];

			for (int i = 0; i < mfList.Count; i++)
			{
				mf[i] = (MeshFilter)mfList[i];
			}

			MeshesToFile(mf);
		}
	}
		
	public static string MeshToString(MeshFilter mf) {
		Mesh m = mf.sharedMesh;
		Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

		StringBuilder sb = new StringBuilder();

		sb.Append("g ").Append(mf.name).Append("\n");
		foreach(Vector3 lv in m.vertices) 
		{
			Vector3 wv = mf.transform.TransformPoint(lv);

			//This is sort of ugly - inverting x-component since we're in
			//a different coordinate system than "everyone" is "used to".
			sb.Append(string.Format("v {0} {1} {2}\n",-wv.x,wv.y,wv.z));
		}
		sb.Append("\n");

		foreach(Vector3 lv in m.normals) 
		{
			Vector3 wv = mf.transform.TransformDirection(lv);

			sb.Append(string.Format("vn {0} {1} {2}\n",-wv.x,wv.y,wv.z));
		}
		sb.Append("\n");

		foreach(Vector3 v in m.uv) 
		{
			sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
		}


		for (int material=0; material < m.subMeshCount; material ++) {
			sb.Append("\n");
			sb.Append("usemtl ").Append(mats[material].name).Append("\n");

			int[] triangles = m.GetTriangles(material);
			for (int i=0;i<triangles.Length;i+=3) 
			{
				//Because we inverted the x-component, we also needed to alter the triangle winding.
				sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n", 
					triangles[i]+1 + vertexOffset, triangles[i+1]+1 + normalOffset, triangles[i+2]+1 + uvOffset));
			}
		}

		vertexOffset += m.vertices.Length;
		normalOffset += m.normals.Length;
		uvOffset += m.uv.Length;

		return sb.ToString();
	}

	private static void MeshesToFile(MeshFilter[] mf) 
	{
		using (StreamWriter sw = new StreamWriter(_path)) 
		{
			for (int i = 0; i < mf.Length; i++)
			{
				sw.Write(MeshToString(mf[i]));
			}
		}
	}


	// Single file
	public void WriteResult(string path) {
		_path = path;
	}

	public void DoFileDialogue() {
		WriteResult(StandaloneFileBrowser.SaveFilePanel("Save File", "", "", "obj"));


		if (_path == null)
			return;

		ExportWholeSelectionToSingle ();
	}
}