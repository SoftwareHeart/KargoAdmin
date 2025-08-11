using System.Collections.Generic;
using KargoAdmin.Models;

namespace KargoAdmin.Models.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; } = string.Empty;

        public List<Blog> BlogResults { get; set; } = new List<Blog>();
        public int BlogTotal { get; set; }

        public List<Blog> UsefulInfoResults { get; set; } = new List<Blog>();
        public int UsefulInfoTotal { get; set; }

        public List<PageResult> Pages { get; set; } = new List<PageResult>();

        public class PageResult
        {
            public string Title { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
        }
    }
}


