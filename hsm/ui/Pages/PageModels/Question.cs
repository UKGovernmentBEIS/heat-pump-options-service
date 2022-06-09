using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace OCC.HSM.UI.Pages.PageModels
{
    public class Question
    {
        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public int OrderNumber { get; set; }

        [BindProperty]
        public string Prompt { get; set; }

        [BindProperty]
        public QuestionType QuestionType { get; set; }

        [BindProperty]
        public List<Answer> Answers { get; set; }
    }
}
