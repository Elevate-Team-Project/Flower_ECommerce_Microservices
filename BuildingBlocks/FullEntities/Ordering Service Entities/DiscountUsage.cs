using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Ordering_Service_Entities
{
    public class DiscountUsage : BaseEntity
    {
        public int OrderId { get; set; }
        public string DiscountCode { get; set; }
        public string UserId { get; set; }
        public DateTime UsedAt { get; set; }

        public virtual Order Order { get; set; }
    }
}
