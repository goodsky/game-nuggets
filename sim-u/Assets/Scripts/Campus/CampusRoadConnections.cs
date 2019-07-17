using Common;
using GameData;

namespace Campus
{
    /// <summary>
    /// Okay, I would call these "Paths" but I already used that word.
    /// Connections between roads, paths and buildings.
    /// </summary>
    public class CampusRoadConnections
    {
        private readonly CampusManager _campusManager;

        public CampusRoadConnections(CampusData campusData, GameAccessor accessor)
        {
            _campusManager = accessor.CampusManager;
        }
    }
}
