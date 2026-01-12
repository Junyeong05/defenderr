using UnityEngine;
using System.Collections.Generic;

// Building 데이터를 저장하는 ScriptableObject (데이터 컨테이너)
[CreateAssetMenu(fileName = "BuildingCatalog", menuName = "MergeDefender/BuildingCatalog")]
public class BuildingCatalog : ScriptableObject
{
    [SerializeField]
    private List<BuildBookVO> buildings = new List<BuildBookVO>();

    public List<BuildBookVO> Buildings => buildings;

    // 에디터에서만 사용 - Google Sheet Importer에서 데이터 설정
    public void SetBuildings(List<BuildBookVO> newBuildings)
    {
        buildings = newBuildings;
    }
}
