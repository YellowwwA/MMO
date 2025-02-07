using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
//게임 실행시 실행되는 파일이 아닌 단순히 유니티 툴 생성 코드
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor
{
    
//#if UNITY_EDITOR


    [MenuItem("Tools/GenerateMap %#g")] //유니티 UI 내에 Tool 탭 생성 및 하위에 GenerateMap 탭 생성
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
            foreach (Vector3Int pos in tm.cellBounds.allPositionsWithin)  //전체 맵을 탐색하며 장애물이 있는 위치를 blocked 리스트에 pos좌표추가
            {
                TileBase tile = tm.GetTile(pos);
                if (tile != null)
                    blocked.Add(pos);
            }*/

            // 맵 크기 먼저 받아옴 -> 갈 수 있는 영역 0 못가는 영역 1 로 표기
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt"))
            {
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax);

                // 어느 위치에 장애물이 있는지 각 점에서부터 차례로 탐색
                for (int y = tmBase.cellBounds.yMax; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null) // 장애물이 있음
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
// %(==Ctrl),  #(==Shift), &(==Alt) //단축키 지정
[MenuItem("Tools/GenerateMap %#g")] //유니티 UI 내에 Tool 탭 생성 및 하위에 GenerateMap 탭 생성
private static void HelloWorld()
{
    if(EditorUtility.DisplayDialog("Hello World", "Create?", "Create", "Cancel")) //"HelloWorld"라는 이름의 오브젝트를 Create? 만들까요? Create 네 Cancel 아니오
    {
        new GameObject("Hello World"); //Create 예 선택시 HelloWorld 오브젝트 생성
    }
}*/