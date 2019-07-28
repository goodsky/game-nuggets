using Common;
using GameData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Faculty
{
    public class FacultyManager : GameDataLoader<FacultyData>, IGameStateSaver<FacultySaveState>
    {
        private readonly Dictionary<int, HiredFaculty> _hiredFaculty = new Dictionary<int, HiredFaculty>();
        private readonly Dictionary<int, GeneratedFaculty> _generatedFaculty = new Dictionary<int, GeneratedFaculty>();

        private FacultyGenerator _generator;

        public IEnumerable<GeneratedFaculty> AvailableFaculty => _generatedFaculty.Values;

        public IEnumerable<HiredFaculty> HiredFaculty => _hiredFaculty.Values;

        public int AvailableClassroomCount
        {
            get
            {
                return Accessor.CampusManager.GetClassroomInfo()
                    .Where(classroom => classroom.IsConnectedToPaths)
                    .Sum(classroom => classroom.ClassroomCount);
            }
        }

        public int UsedClassroomCount
        {
            get
            {
                return HiredFaculty
                    .Sum(faculty => faculty.TeachingSlots);
            }
        }

        public int UniversityTeachingScore
        {
            get
            {
                return HiredFaculty
                    .Sum(faculty => faculty.TeachingOutput);
            }
        }

        public int AvailableLabsCount
        {
            get
            {
                return Accessor.CampusManager.GetLabInfo()
                    .Where(classroom => classroom.IsConnectedToPaths)
                    .Sum(classroom => classroom.LabCount);
            }
        }

        public int UsedLabsCount
        {
            get
            {
                return HiredFaculty
                    .Sum(faculty => faculty.ResearchSlots);
            }
        }

        public int UniversityResearchScore
        {
            get
            {
                return HiredFaculty
                    .Sum(faculty => faculty.ResearchOutput);
            }
        }

        public void HireFaculty(GeneratedFaculty faculty)
        {
            GameLogger.Info("Hiring faculty! - {0}", faculty);

            _generatedFaculty.Remove(faculty.Id);
            HiredFaculty hired = faculty.Hire();
            _hiredFaculty.Add(hired.Id, hired);
        }

        public void AddTeaching(HiredFaculty faculty)
        {
            if (faculty.UsedSlots < faculty.MaximumSlots)
            {
                ++faculty.TeachingSlots;
            }
        }

        public void RemoveTeaching(HiredFaculty faculty)
        {
            if (faculty.TeachingSlots > 0)
            {
                --faculty.TeachingSlots;
            }
        }

        public void AddResearch(HiredFaculty faculty)
        {
            if (faculty.UsedSlots < faculty.MaximumSlots)
            {
                ++faculty.ResearchSlots;
            }
        }

        public void RemoveResearch(HiredFaculty faculty)
        {
            if (faculty.ResearchSlots > 0)
            {
                --faculty.ResearchSlots;
            }
        }

        public void GenerateNewFaculty(int count)
        {
            _generatedFaculty.Clear();
            for (int i = 0; i < count; ++i)
            {
                GeneratedFaculty newFaculty = _generator.GenerateNext();
                _generatedFaculty.Add(newFaculty.Id, newFaculty);
            }
        }

        public Sprite GetHeadshotForFaculty(int id)
        {
            return _generator.GetHeadshotForFaculty(id);
        }

        public FacultySaveState SaveGameState()
        {
            return new FacultySaveState
            {
                GeneratedFaculty = _generatedFaculty.Values.ToArray(),
                HiredFaculty = _hiredFaculty.Values.ToArray(),
            };
        }

        public void LoadGameState(FacultySaveState state)
        {
            if (state != null)
            {
                _generator.SetNextFacultyId(state.NextFacultyId);

                if (state.GeneratedFaculty != null)
                {
                    _generatedFaculty.Clear();
                    foreach (GeneratedFaculty faculty in state.GeneratedFaculty)
                    {
                        _generatedFaculty.Add(faculty.Id, faculty);
                    }
                }

                if (state.HiredFaculty != null)
                {
                    _hiredFaculty.Clear();
                    foreach (HiredFaculty faculty in state.HiredFaculty)
                    {
                        _hiredFaculty.Add(faculty.Id, faculty);
                    }
                }
            }
        }

        protected override void LoadData(FacultyData gameData)
        {
            _generator = new FacultyGenerator(gameData);

            GenerateNewFaculty(gameData.AvailableFacultyCount);
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
