using Common;
using GameData;
using Simulation;
using System.Collections.Generic;
using UnityEngine;

namespace Faculty
{
    public class FacultyGenerator
    {
        private readonly GameAccessor _accessor;
        private readonly FacultyData _config;
        private readonly Sprite _defaultHeadshot;
        private readonly List<string> _firstNamesMen;
        private readonly List<string> _firstNamesWomen;
        private readonly List<string> _lastNames;

        public int NextFacultyId { get; private set; } = 0;

        public FacultyGenerator(GameAccessor accessor, FacultyData data)
        {
            _accessor = accessor;
            _config = data;

            _defaultHeadshot = data.DefaultHeadshot;
            _firstNamesMen = data.FirstNamesMen.Names;
            _firstNamesWomen = data.FirstNamesWomen.Names;
            _lastNames = data.LastNames.Names;
        }

        public void SetNextFacultyId(int id)
        {
            if (id < NextFacultyId)
            {
                GameLogger.Error("Unexpected next faculty id set! Current = {0}; New = {1};", NextFacultyId, id);
            }

            NextFacultyId = id;
        }

        public GeneratedFaculty GenerateNext()
        {
            // Generate Faculty Name
            bool isWoman = Random.Range(0, 2) % 2 == 0;
            List<string> firstNames = isWoman ? _firstNamesWomen : _firstNamesMen;
            int firstIndex = Random.Range(0, firstNames.Count);
            int lastIndex = Random.Range(0, _lastNames.Count);
            string name = $"{firstNames[firstIndex]} {_lastNames[lastIndex]}";

            // Generate Faculty Scores
            int ap = _accessor.Simulation.Score.AcademicPrestige;
            double rp = _accessor.Simulation.Score.ResearchPrestige;

            int minTeachingInput = 0; // bug: these values are not connected to the config...
            int maxTeachingInput = 100 + (int)System.Math.Round(100 * _config.ResearchPrestigeImpactOnTeachingScore);
            double meanTeachingScore = SimulationUtils.LinearMapping(
                ap + (rp * _config.ResearchPrestigeImpactOnTeachingScore),
                minTeachingInput,
                maxTeachingInput,
                _config.MinDefaultTeachingScore,
                _config.TeachingScore.MaxValue);

            int minResearchInput = 0; // bug: these values are not connected to the config...
            int maxResearchInput = 100 + (int)System.Math.Round(100 * _config.TeachingPrestigeImpactOnResearchScore);
            double meanResearchScore = SimulationUtils.LinearMapping(
                rp + (ap * _config.TeachingPrestigeImpactOnResearchScore),
                minResearchInput,
                maxResearchInput,
                _config.MinDefaultResearchScore,
                _config.ResearchScore.MaxValue);

            double teachingScore = SimulationUtils.GenerateNormal(meanTeachingScore, _config.TeachingScoreStdDev);
            double researchScore = SimulationUtils.GenerateNormal(meanResearchScore, _config.ResearchScoreStdDev);

            int intTeachingMean = (int)System.Math.Round(meanTeachingScore);
            int teachingThreeSigma = (int)System.Math.Round(_config.TeachingScoreStdDev * 3);
            teachingScore = Utils.Clamp(teachingScore, intTeachingMean - teachingThreeSigma, intTeachingMean + teachingThreeSigma);
            teachingScore = Utils.Clamp(teachingScore, _config.TeachingScore.MinValue, _config.TeachingScore.MaxValue);

            int intResearchMean = (int)System.Math.Round(meanResearchScore);
            int researchThreeSigma = (int)System.Math.Round(_config.ResearchScoreStdDev * 3);
            researchScore = Utils.Clamp(researchScore, intResearchMean - researchThreeSigma, intResearchMean + researchThreeSigma);
            researchScore = Utils.Clamp(researchScore, _config.ResearchScore.MinValue, _config.ResearchScore.MaxValue);

            int maximumSlots = Random.Range(_config.FacultySlots.MinValue, _config.FacultySlots.MaxValue);

            double salaryPerSlot =
                (teachingScore - _config.TeachingScore.MinValue) * _config.AverageDollarsPerTeachingPoint +
                (researchScore - _config.ResearchScore.MinValue) * _config.AverageDollarsPerResearchPoint;

            double teachingOverTheMean = Utils.Clamp(teachingScore - meanTeachingScore, 0, 99999);
            double researchOverTheMean = Utils.Clamp(researchScore - meanResearchScore, 0, 99999);
            double salaryPremium =
                SimulationUtils.ExponentialMapping(
                    teachingOverTheMean + researchOverTheMean,
                    0,
                    teachingThreeSigma + researchThreeSigma,
                    0,
                    _config.FacultyOverTheMeanPremiumMaxDollars,
                    _config.FacultyOverTheMeanPremiumExponentialFactor);

            int salary =
                _config.MinimumBaseSalary +
                (int)System.Math.Round(salaryPremium) +
                (int)System.Math.Round(salaryPerSlot * maximumSlots);

            int roundedSalary = (salary / 5000) * 5000;

            var newFaculty = new GeneratedFaculty(
                NextFacultyId++,
                name,
                roundedSalary,
                (int)System.Math.Round(teachingScore),
                (int)System.Math.Round(researchScore),
                maximumSlots);

            GameLogger.Info("Generated new faculty = {0}; MeanTeaching = {1:0.00}; MeanResearch = {2:0.00}; SalaryPremium = ${3:n0};",
                newFaculty,
                meanTeachingScore,
                meanResearchScore,
                salaryPremium);

            return newFaculty;
        }

        public Sprite GetHeadshotForFaculty(int id)
        {
            // TODO: I want to generate random headshots for each faculty. Mr. Potato Head style.
            return _defaultHeadshot;
        }
    }
}
