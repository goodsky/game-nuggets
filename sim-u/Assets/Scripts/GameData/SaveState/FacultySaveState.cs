using Faculty;
using System;

namespace GameData
{
    [Serializable]
    public class FacultySaveState
    {
        public HiredFaculty[] Faculty { get; set; }
    }
}
