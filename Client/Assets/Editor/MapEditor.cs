using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
//���� ����� ����Ǵ� ������ �ƴ� �ܼ��� ����Ƽ �� ���� �ڵ�
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor
{
    
//#if UNITY_EDITOR


    [MenuItem("Tools/GenerateMap %#g")] //����Ƽ UI ���� Tool �� ���� �� ������ GenerateMap �� ����
    private static void GenerateMap()
    {
        GenerateByPath("Assets/Resources/Map");
        GenerateByPath("../Common/MapData");
    }

    private static void GenerateByPath(string pathPrefix)
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach (GameObject go in gameObjects)
        {
            Tilemap tmBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);
            /*
            List<Vector3Int> blocked = new List<Vector3Int>();
            foreach (Vector3Int pos in tm.cellBounds.allPositionsWithin)  //��ü ���� Ž���ϸ� ��ֹ��� �ִ� ��ġ�� blocked ����Ʈ�� pos��ǥ�߰�
            {
                TileBase tile = tm.GetTile(pos);
                if (tile != null)
                    blocked.Add(pos);
            }*/

            // �� ũ�� ���� �޾ƿ� -> �� �� �ִ� ���� 0 ������ ���� 1 �� ǥ��
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt"))
            {
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax);

                // ��� ��ġ�� ��ֹ��� �ִ��� �� ���������� ���ʷ� Ž��
                for (int y = tmBase.cellBounds.yMax; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null) // ��ֹ��� ����
                            writer.Write("1");
                        else
                            writer.Write("0");
                    }
                    writer.WriteLine();
                }
            }
        }
    }

    //#endif
}

/*
// %(==Ctrl),  #(==Shift), &(==Alt) //����Ű ����
[MenuItem("Tools/GenerateMap %#g")] //����Ƽ UI ���� Tool �� ���� �� ������ GenerateMap �� ����
private static void HelloWorld()
{
    if(EditorUtility.DisplayDialog("Hello World", "Create?", "Create", "Cancel")) //"HelloWorld"��� �̸��� ������Ʈ�� Create? ������? Create �� Cancel �ƴϿ�
    {
        new GameObject("Hello World"); //Create �� ���ý� HelloWorld ������Ʈ ����
    }
}*/