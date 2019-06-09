using GameData;
using UnityEngine;

namespace Campus
{
    public class Building : MonoBehaviour
    {
        public BuildingData Data { get; private set; }

        public void Initialize(BuildingData buildingData)
        {
            Data = buildingData;
        }
    }
}
