using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace OCC.HSM.UI.Pages.PageModels
{
    public class QuestionType
    {
        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string Type { get; set; }

        //[BindProperty]
        //public List<Question> Questions { get; set; }
    }
}
