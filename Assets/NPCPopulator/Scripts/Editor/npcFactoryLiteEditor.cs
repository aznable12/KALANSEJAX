using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(npcFactory))]
public class npcFactoryLiteEditor : Editor {

    npcFactory myFactory;

    int numberToCreate;

    void OnEnable()
    {
        myFactory = (npcFactory)target;
    }

    public override void OnInspectorGUI()
    {
        myFactory.center = (Transform)EditorGUILayout.ObjectField("Center:", myFactory.center, typeof(Transform), true);
        myFactory.envRange = EditorGUILayout.FloatField("Range from Center:", myFactory.envRange);
        myFactory.NPCparent = (Transform)EditorGUILayout.ObjectField("NPC Parent:", myFactory.NPCparent, typeof(Transform), true);
        myFactory.usualSpeed = EditorGUILayout.FloatField("NPC Speed Average:", myFactory.usualSpeed);
        myFactory.usualSpeedVariation = EditorGUILayout.FloatField("NPC Speed Variation:", myFactory.usualSpeedVariation);

        if (GUILayout.Button("Create NPC"))
        {
            if (myFactory.NPCModels.Count == 0)
            {
                Debug.LogError("You must specify at least one NPC Template.");
            }
            else
            {
                myFactory.CreateNPC();
            }
        }
        numberToCreate = EditorGUILayout.IntField("Number To Create:", numberToCreate);
        if (GUILayout.Button("Create n NPCs"))
        {
            if (myFactory.NPCModels.Count == 0)
            {
                Debug.LogError("You must specify at least one NPC Template.");
            }
            else
            {
                for (int i = 0; i < numberToCreate; i++)
                {
                    myFactory.CreateNPC();
                }
            }
        }

        if (GUILayout.Button("Add New NPC Template"))
        {
            myFactory.AddNPCtemplate();
        }

        for (int i = 0; i < myFactory.NPCModels.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("NPC Model:", GUILayout.MaxWidth(72));
            myFactory.NPCModels[i] = (GameObject)EditorGUILayout.ObjectField(myFactory.NPCModels[i], typeof(GameObject), true, GUILayout.Width(148));
            if (GUILayout.Button("-"))
            {
                myFactory.RemoveNPCtemplate(i);
            }
            EditorGUILayout.EndHorizontal();
        }

    }
}

