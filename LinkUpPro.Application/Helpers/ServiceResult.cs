using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Application.Helpers
{
    public class ServiceResult
    {
        public bool Succeeded { get; set; }

        public string Message { get; set; }

        public string UserId { get; set; }

        public string Token { get; set; }

        public bool RequiresReLogin { get; set; }

        public int? EntityId { get; set; }
    }
}
