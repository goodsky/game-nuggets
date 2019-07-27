using GameData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Faculty
{
    public class FacultyManager : GameDataLoader<FacultyData>, IGameStateSaver<FacultySaveState>
    {
        private readonly Dictionary<int, HiredFaculty> _hiredFaculty = new Dictionary<int, HiredFaculty>();

        private Sprite _defaultHeadshot;

        public IEnumerable<HiredFaculty> HiredFaculty
        {
            get
            {
                return _hiredFaculty.Values;
            }
        }

        public Sprite GetHeadshotForFaculty(int id)
        {
            // TODO: I want to generate random headshots for each faculty. Mr. Potato Head style.
            return _defaultHeadshot;
        }

        public FacultySaveState SaveGameState()
        {
            return new FacultySaveState
            {
                Faculty = _hiredFaculty.Values.ToArray(),
            };
        }

        public void LoadGameState(FacultySaveState state)
        {
            if (state != null)
            {
                if (state.Faculty != null)
                {
                    foreach (HiredFaculty faculty in state.Faculty)
                    {
                        _hiredFaculty.Add(faculty.Id, faculty);
                    }
                }
            }
        }

        protected override void LoadData(FacultyData gameData)
        {
            _defaultHeadshot = gameData.DefaultHeadshot;
        }

        protected override void LinkData(FacultyData gameData)
        {
            // The link step runs after all intial data has been loaded.
            // The perfect time to load the saved game data.
            FacultySaveState savedGame = gameData.SavedData?.Faculty;
            LoadGameState(savedGame);
        }
    }
}
