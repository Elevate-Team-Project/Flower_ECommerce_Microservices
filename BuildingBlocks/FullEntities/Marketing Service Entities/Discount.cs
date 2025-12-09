using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Marketing_Service_Entities
{
    public class Discount : BaseEntity
    {
        public string Code { get; set; }
        public string DiscountType { get; set; } // Consider using Enum later
        public decimal Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<ProductDiscount> ProductDiscounts { get; set; }
    }
}