using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class TokenType
    {
        public TokenType()
        {
            ActiveTokens = new HashSet<ActiveToken>();
        }

        public string TokenTypeName { get; set; } = null!;

        public virtual ICollection<ActiveToken> ActiveTokens { get; set; }
    }
}
