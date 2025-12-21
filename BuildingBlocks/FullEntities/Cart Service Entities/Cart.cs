using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Cart_Service_Entities
{
    public class Cart : BaseEntity
    {
        public string UserId { get; set; }
        public string CouponCode { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<CartItem> Items { get; set; }
    }
}
