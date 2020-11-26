using System;
using System.Collections.Generic;
using System.Text;
using Tokket.Tokkepedia;

namespace Tokkepedia.Shared.Models
{
    public class GameSetModel
    {
        public int IdImageGame { get; set; }
        //
        // Summary:
        //     Game names: "tokboom", "tokblast"
        public string GameTitle { get; set; }
        public string GameDescription { get; set; }

        public string Category { get; set; }
        public string CategoryId { get; set; }
        public List<Tok> Toks { get; set; }

    }
}
