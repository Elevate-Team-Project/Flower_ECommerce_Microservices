using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Catalog_Service_Entities
{
    public class PriceHistory : BaseEntity
    {
        public int ProductId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string ChangeReason { get; set; }
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public string ChangedByUserId { get; set; }

        public virtual Product Product { get; set; }
    }
}
