using Common;
using Faculty;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A row in the <see cref="FacultyHiringWindow"/>.
    /// </summary>
    public class FacultyHiringRow : MonoBehaviour
    {
        private FacultyHiringWindow _parent;

        private GeneratedFaculty _faculty;

        public Image Headshot;

        public Text NameText;

        public Text SalaryText;

        public Text TeachingText;

        public Text ResearchText;

        public Button HireButton;

        public void Initialize(FacultyHiringWindow parent, GeneratedFaculty faculty, Sprite headshot, GameAccessor accessor)
        {
            _parent = parent;
            _faculty = faculty;
            Headshot.sprite = headshot;
            NameText.text = faculty.Name;
            SalaryText.text = $"${faculty.SalaryPerYear:n0} /yr";
            TeachingText.text = faculty.TeachingScore.ToString();
            ResearchText.text = faculty.ResearchScore.ToString();

            FacultyManager manager = accessor.Faculty;
            HireButton.OnMouseDown = (_) =>
            {
                manager.HireFaculty(_faculty);
                _parent.UpdateList();
            };
        }
    }
}
