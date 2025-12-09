using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Marketing_Service_Entities
{
    public class ProductDiscount : BaseEntity
    {
        public int DiscountId { get; set; }
        public int ProductId { get; set; }

        public virtual Discount Discount { get; set; }
        // Note: We don't necessarily nav to Product here if Product is in another context/service,
        // but since these are Shared Entities, you can add it if you reference Catalog namespace.
    }
}
