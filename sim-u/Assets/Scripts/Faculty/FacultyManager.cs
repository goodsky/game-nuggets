using Campus;
using Common;
using GameData;
using Simulation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Faculty
{
    /// <summary>
    /// Unity GameObject that manages Faculty State.
    /// </summary>
    public class FacultyManager : GameDataLoader<FacultyData>, IGameStateSaver<FacultySaveState>
    {
        /// <summary>
        /// NB: this lock is used to guard against races while saving, generating or applying faculty student assignments.
        /// </summary>
        private readonly object _facultyLock = new object();

        private readonly Dictionary<int, HiredFaculty> _hiredFaculty = new Dictionary<int, HiredFaculty>();
        private readonly Dictionary<int, GeneratedFaculty> _generatedFaculty = new Dictionary<int, GeneratedFaculty>();

        private FacultyGenerator _generator;
        private HiredFaculty _nullFaculty;

        public IEnumerable<GeneratedFaculty> AvailableFaculty => _generatedFaculty.Values;

        public IEnumerable<HiredFaculty> HiredFaculty => _hiredFaculty.Values;

        public int StaffedClassroomCapacity
        {
            get;
            private set;
        }

        public int TotalClassroomCount
        {
            get
            {
                return Accessor.CampusManager.TotalConnectedClassroomCount;
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

        public int TotalLabsCount
        {
            get
            {
                return Accessor.CampusManager.TotalConnectedLaboratoryCount;
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

        /// <summary>
        /// Divies up classes for the faculty.
        /// This will be called at the start of each new quarter,
        /// or whenever faculty changes are made.
        /// </summary>
        /// <param name="faculty"></param>
        public void AssignFacultyToStudents(StudentBody studentBody)
        {
            lock (_facultyLock)
            {
                // Step 1: Find the available classrooms
                var classroomCapacities = new List<int>();
                foreach (BuildingInfo building in Accessor.CampusManager.GetBuildingInfo(checkConnections: true))
                {
                    if (building.IsConnectedToPaths.Value)
                    {
                        for (int i = 0; i < building.SmallClassroomCount; ++i)
                            classroomCapacities.Add(Accessor.CampusManager.SmallClassroomCapacity);

                        for (int i = 0; i < building.MediumClassroomCount; ++i)
                            classroomCapacities.Add(Accessor.CampusManager.MediumClassroomCapacty);

                        for (int i = 0; i < building.LargeClassroomCount; ++i)
                            classroomCapacities.Add(Accessor.CampusManager.LargeClassroomCapacity);
                    }
                }

                classroomCapacities.Sort((x, y) => y.CompareTo(x)); // sort largest to smallest so we use largest rooms first

                // Step 2: Assign faculty to the available classrooms
                var teachers = _hiredFaculty.Values.ToList();

                var teacherAssignments = new Dictionary<int, StudentHistogram[]>();
                var classroomAssignments = new List<(int capacity, int teacherId, StudentHistogram[] students)>();

                int classroomIndex = 0;
                teachers.Sort((x, y) => y.TeachingScore.CompareTo(x.TeachingScore)); // sort faculty to use best teachers first
                foreach (HiredFaculty teacher in teachers)
                {
                    if (teacher.TeachingSlots > 0)
                    {
                        var assignedStudents = new StudentHistogram[(int)StudentBodyYear.MaxYearsToGraduate];
                        for (int i = 0; i < assignedStudents.Length; ++i)
                        {
                            assignedStudents[i] = Accessor.Simulation.GenerateStudentPopulation();
                        }

                        teacherAssignments[teacher.Id] = assignedStudents;
                        for (int i = 0; i < teacher.TeachingSlots; ++i)
                        {
                            classroomAssignments.Add((classroomCapacities[classroomIndex], teacher.Id, assignedStudents));

                            if (++classroomIndex >= classroomCapacities.Count)
                            {
                                break;
                            }
                        }
                    }

                    if (classroomIndex >= classroomCapacities.Count)
                    {
                        break;
                    }
                }

                // Step 3: Randomly place students into classrooms.
                //         Starting from seniors down. Freshman are the first to miss out on classrooms.
                var untaughtStudents = new StudentHistogram[(int)StudentBodyYear.MaxYearsToGraduate];
                for (int i = 0; i < untaughtStudents.Length; ++i)
                {
                    untaughtStudents[i] = Accessor.Simulation.GenerateStudentPopulation();
                }

                int currentClassroomIndex = 0;
                int currentClassroomStudentCount = 0;
                for (int i = (int)StudentBodyYear.MaxYearsToGraduate - 1; i >= 0; --i)
                {
                    StudentHistogram enrolledStudents = studentBody.GetClassAcademicScores((StudentBodyYear)i);

                    foreach (int score in enrolledStudents.EnumerateStudents())
                    {
                        if (currentClassroomIndex < classroomAssignments.Count)
                        {
                            var classroom = classroomAssignments[currentClassroomIndex];
                            classroom.students[i].AddSingleValue(score);

                            if (++currentClassroomStudentCount >= classroom.capacity)
                            {
                                currentClassroomStudentCount = 0;
                                ++currentClassroomIndex;
                            }
                        }
                        else
                        {
                            // If you can't fit into a classroom you are "untaught"
                            untaughtStudents[i].AddSingleValue(score);
                        }
                    }
                }

                // Step 4: Give teachers their assignments
                foreach (var assignment in teacherAssignments)
                {
                    HiredFaculty faculty = _hiredFaculty[assignment.Key];
                    faculty.AssignStudents(assignment.Value);
                    for (int i = 0; i < assignment.Value.Length; ++i)
                    {
                        if (assignment.Value[i].HasValues)
                        {
                            GameLogger.Debug("[TeachingAssignment - {2}] Faculty: {0} Students: {1}", faculty.Name, assignment.Value[i], ((StudentBodyYear)i).ToString());
                        }
                    }
                }

                _nullFaculty.AssignStudents(untaughtStudents);
                for (int i = 0; i < untaughtStudents.Length; ++i)
                {
                    if (untaughtStudents[i].HasValues)
                    {
                        GameLogger.Debug("[TeachingAssignment - {1}] UNTAUGHT Students: {0};", untaughtStudents[i], ((StudentBodyYear)i).ToString());
                    }
                }

                // Update the "staffed" classroom capacity.
                StaffedClassroomCapacity = classroomAssignments.Sum(t => t.capacity);
            }
        }

        /// <summary>
        /// With the current assigned students, calculate the delta to the
        /// student body's academic scores. The returned value must be applied
        /// to the student body histograms.
        /// </summary>
        /// <param name="classModifier">Arbitrary modifier to punish for having classrooms that are too full.</param>
        /// <returns>The delta to apply to the student body.</returns>
        public StudentHistogram[] ExecuteTeachingStep()
        {
            lock (_facultyLock)
            {
                var delta = new StudentHistogram[(int)StudentBodyYear.MaxYearsToGraduate];
                for (int i = 0; i < delta.Length; ++i)
                {
                    delta[i] = Accessor.Simulation.GenerateStudentPopulation();
                }

                foreach (HiredFaculty faculty in _hiredFaculty.Values)
                {
                    StudentHistogram[] teacherDelta = faculty.Teach();
                    if (teacherDelta != null)
                    {
                        for (int i = 0; i < delta.Length; ++i)
                        {
                            delta[i].Add(teacherDelta[i]);
                        }
                    }
                }

                StudentHistogram[] untaughtDelta = _nullFaculty.Teach();
                if (untaughtDelta != null)
                {
                    for (int i = 0; i < delta.Length; ++i)
                    {
                        delta[i].Add(untaughtDelta[i]);
                    }
                }

                for (int i = 0; i < delta.Length; ++i)
                {
                    if (delta[i].TotalStudentCount != 0)
                    {
                        GameLogger.Error("Calculated a delta that does not sum to 0! [{0}] Delta = {1}", i, delta[i]);
                    }
                }

                return delta;
            }
        }

        public void HireFaculty(GeneratedFaculty faculty)
        {
            lock (_facultyLock)
            {
                GameLogger.Info("Hiring faculty! - {0}", faculty);

                _generatedFaculty.Remove(faculty.Id);
                HiredFaculty hired = faculty.Hire();
                _hiredFaculty.Add(hired.Id, hired);

                AssignFacultyToStudents(Accessor.Simulation.CurrentStudentBody());
            }
        }

        public void DropFaculty(HiredFaculty faculty)
        {
            lock (_facultyLock)
            {
                GameLogger.Info("Dropping faculty! - {0}", faculty);

                _hiredFaculty.Remove(faculty.Id);

                AssignFacultyToStudents(Accessor.Simulation.CurrentStudentBody());
            }
        }

        public void AddTeaching(HiredFaculty faculty)
        {
            lock (_facultyLock)
            {
                if (TotalClassroomCount - UsedClassroomCount > 0 &&
                    faculty.UsedSlots < faculty.MaximumSlots)
                {
                    ++faculty.TeachingSlots;
                    AssignFacultyToStudents(Accessor.Simulation.CurrentStudentBody());
                }
            }
        }

        public void RemoveTeaching(HiredFaculty faculty)
        {
            lock (_facultyLock)
            {
                if (faculty.TeachingSlots > 0)
                {
                    --faculty.TeachingSlots;

                    AssignFacultyToStudents(Accessor.Simulation.CurrentStudentBody());
                }
            }
        }

        public void AddResearch(HiredFaculty faculty)
        {
            lock (_facultyLock)
            {
                if (TotalLabsCount - UsedLabsCount > 0 &&
                    faculty.UsedSlots < faculty.MaximumSlots)
                {
                    ++faculty.ResearchSlots;
                }
            }
        }

        public void RemoveResearch(HiredFaculty faculty)
        {
            lock (_facultyLock)
            {
                if (faculty.ResearchSlots > 0)
                {
                    --faculty.ResearchSlots;
                }
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
            lock (_facultyLock)
            {
                return new FacultySaveState
                {
                    NextFacultyId = _generator.NextFacultyId,
                    GeneratedFaculty = _generatedFaculty.Values.ToArray(),
                    HiredFaculty = _hiredFaculty.Values.ToArray(),
                    NullFaculty = _nullFaculty,
                    StaffedClassroomCapacity = StaffedClassroomCapacity,
                };
            }
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

                if (state.NullFaculty != null)
                {
                    _nullFaculty = state.NullFaculty;
                }

                StaffedClassroomCapacity = state.StaffedClassroomCapacity;
            }
        }

        protected override void LoadData(FacultyData gameData)
        {
            _generator = new FacultyGenerator(gameData);

            // If a student can't fit in a classroom they are taught by the "Street".
            _nullFaculty = new HiredFaculty(-1, "Street", 0, gameData.TeachingScore.DefaultValue, gameData.ResearchScore.DefaultValue, -1);
        }

        protected override void LinkData(FacultyData gameData)
        {
            // The link step runs after all intial data has been loaded.
            // The perfect time to load the saved game data.
            FacultySaveState savedGame = gameData.SavedData?.Faculty;

            if (savedGame != null)
            {
                LoadGameState(savedGame);
            }
            else
            {
                GenerateNewFaculty(gameData.AvailableFacultyCount);
            }
        }
    }
}
