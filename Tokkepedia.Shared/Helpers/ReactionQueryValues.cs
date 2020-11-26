using System;
using System.Collections.Generic;
using System.Text;

namespace Tokkepedia.Shared.Helpers
{
    public class ReactionQueryValues
    {
        public int limit = 20;
        public string kind = "";
        public string item_id = "";
        public string activity_id = "";
        public string user_id = "";
        public string reaction_id = "";
        public string pagination_id = "";
        public int? reaction_total = null;
        public int? detail_number = 0; //Less than 0: All reactions, 0: Tok Level, 1+: Detail Level 
    }
}
