using UnityEditor;
using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using log4net.Util;

public static class SimpleCombineMesh
{
    private const string Menu_ItemName_CombineMesh = "GameObject/Simple Combine Mesh/Combine Mesh";
    private const string NewObjName = "Combined Mesh";

    private const string MSG_NoMeshFilter = "MeshFilter가 없는 오브젝트를 선택했습니다.";
    private const string MSG_NoObject = "선택한 오브젝트가 없습니다.";
    private const string MSG_OneObject = "2개 이상의 오브젝트를 선택해야합니다.";
    private const string MSG_CombinedObject = "{0} - {1}개의 오브젝트가 하나가 되었습니다.";


    [MenuItem(Menu_ItemName_CombineMesh, false, priority:11)]
    public static void Menu_CombineMesh(MenuCommand menuCommand)
    {
        Process(menuCommand);
    }

    private static void Process(MenuCommand menuCommand)
    {
        Object[] objs = Selection.objects;
        if (objs == null || objs.Length <= 0) return;
        if (menuCommand.context != Selection.objects[0]) return;
        if(objs.Length == 1)
        {
            Debug.LogError(MSG_OneObject);
            return;
        }


        List<MeshFilter> meshList = new List<MeshFilter>();
        Material material = null;
        for(int i=0;i< objs.Length;i++)
        {
            Object obj = objs[i];
            if (obj == null) continue;

            MeshFilter mesh = obj.GetComponent<MeshFilter>();
            if(mesh==null)
            {
                Debug.LogError(MSG_NoMeshFilter);
                return;
            }
            meshList.Add(mesh);
            if (material == null)
            {
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                if(renderer) material = renderer.sharedMaterial;
            }
        }

        if(meshList.Count <= 0)
        {
            Debug.LogError(MSG_NoObject);
            return;
        }

        CombineMesh(meshList, material);
    }

    private static void CombineMesh(List<MeshFilter> meshList, Material material)
    {
        int meshCount = meshList.Count;
        CombineInstance[] combineInstances = new CombineInstance[meshCount];

        for (int i = 0; i < meshCount; i++)
        {
            combineInstances[i].mesh = meshList[i].sharedMesh;
            combineInstances[i].transform = meshList[i].transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combineInstances);
        mesh.hideFlags = HideFlags.HideAndDontSave;

        GameObject obj = new GameObject();
        obj.name = GetNewObjectName(NewObjName);

        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = material;

        Selection.activeGameObject = obj;
        
        Debug.Log(string.Format(MSG_CombinedObject, obj.name, meshCount));
    }

    private static string GetNewObjectName(string name)
    {
        int index = 0;
        string countName = name;
        bool loop = false;
        do
        {
            loop = false;
            countName = string.Format("{0}_{1}", name, index);

            GameObject obj = GameObject.Find(countName);
            if (obj == null) break;
            loop = true;
            index++;
        } while (loop);
        return countName;
    }
}