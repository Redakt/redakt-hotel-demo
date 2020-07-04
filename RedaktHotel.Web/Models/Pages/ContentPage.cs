﻿using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.Collections.Generic;
using RedaktHotel.Web.Models.Embedded;

namespace RedaktHotel.Web.Models.Pages
{
    [Page]
    [Icon(ContentIcons.FileText)]
    [AllowChildren(typeof(ContentPage))]
    [AllowView("ContentPage", "RoomList", "FacilitiesList")]
    public class ContentPage : SimplePage
    {
        [Section("Modules")]
        [HideLabel]
        [HelpText("This list is language-dependent, therefore every language will have its separate set of modules.")]
        public IReadOnlyList<ModuleBase> Modules { get; set; }
    }
}