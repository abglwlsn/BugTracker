using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class CreateEditProjectViewModel
    {
        public Project Project { get; set; }
        public SelectList ProjectManagers { get; set; }
        public MultiSelectList Developers { get; set; }
        public List<string> SelectedDevelopers { get; set; }
        public MultiSelectList Submitters { get; set; }
        public List<string> SelectedSubmitters { get; set; }

    }
}