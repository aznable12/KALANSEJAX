#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace Puppet3D
{
	[ExecuteInEditMode]
	public class SetBlendShape : MonoBehaviour
	{


		[MenuItem("Window/Set Blend Shape")]
		static public void ReplaceBlendShape()
		{
			//int shapeIndexToChange = SourceSkin.sharedMesh.GetBlendShapeIndex (blendShapeName);
			GameObject sel = Selection.gameObjects[1];
			GameObject sel2 = Selection.gameObjects[0];
			SkinnedMeshRenderer SourceSkin = sel2.GetComponent<SkinnedMeshRenderer>();
			Mesh myMesh = sel.GetComponent<MeshFilter>().sharedMesh;
			string blendShapeName = sel.name;
			Mesh tmpMesh = new Mesh();
			tmpMesh.vertices = myMesh.vertices;
			Vector3[] dVertices = new Vector3[myMesh.vertexCount];
			Vector3[] dNormals = new Vector3[myMesh.vertexCount];
			Vector3[] dTangents = new Vector3[myMesh.vertexCount];
			bool added = false;
			for (int shape = 0; shape < myMesh.blendShapeCount; shape++)
			{
				for (int frame = 0; frame < myMesh.GetBlendShapeFrameCount(shape); frame++)
				{
					string shapeName = myMesh.GetBlendShapeName(shape);
					float frameWeight = myMesh.GetBlendShapeFrameWeight(shape, frame);

					myMesh.GetBlendShapeFrameVertices(shape, frame, dVertices, dNormals, dTangents);

					if (shapeName == blendShapeName)
					{
						Debug.Log("Changing");
						dVertices = myMesh.vertices;
						dNormals = myMesh.normals;
						added = true;
					}



					tmpMesh.AddBlendShapeFrame(shapeName, frameWeight, dVertices, dNormals, dTangents);
					//BlendShapesAll newBlendShape = new BlendShapesAll (shape,frame, dVertices, dNormals, dTangents);
					//allBlendShapes.Add(newBlendShape);
				}
			}

			//SourceSkin.sharedMesh = tmpMesh;

			SourceSkin.sharedMesh.ClearBlendShapes();

			for (int shape = 0; shape < tmpMesh.blendShapeCount; shape++)
			{
				for (int frame = 0; frame < tmpMesh.GetBlendShapeFrameCount(shape); frame++)
				{
					string shapeName = tmpMesh.GetBlendShapeName(shape);
					float frameWeight = tmpMesh.GetBlendShapeFrameWeight(shape, frame);

					tmpMesh.GetBlendShapeFrameVertices(shape, frame, dVertices, dNormals, dTangents);

					myMesh.AddBlendShapeFrame(shapeName, frameWeight, dVertices, dNormals, dTangents);
					//BlendShapesAll newBlendShape = new BlendShapesAll (shape,frame, dVertices, dNormals, dTangents);
					//allBlendShapes.Add(newBlendShape);	
				}
			}
			if (!added)
			{
				SourceSkin.sharedMesh.AddBlendShapeFrame(blendShapeName, 1f, myMesh.vertices, myMesh.normals, myMesh.normals);
			}


		}
	}

}
#endif