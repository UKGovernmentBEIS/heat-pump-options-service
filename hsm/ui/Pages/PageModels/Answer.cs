using Microsoft.AspNetCore.Mvc;

namespace OCC.HSM.UI.Pages.PageModels
{
    public class Answer
    {
        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public bool IsSelected { get; set; }

        [BindProperty]
        public string Prompt { get; set; }
    }
}
