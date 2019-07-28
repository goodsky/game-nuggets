using Faculty;
using System;

namespace GameData
{
    [Serializable]
    public class FacultySaveState
    {
        public int NextFacultyId { get; set; }

        public GeneratedFaculty[] GeneratedFaculty { get; set; }

        public HiredFaculty[] HiredFaculty { get; set; }
    }
}
