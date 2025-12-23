using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Catalog_Service_Entities.Occasions
{
    public class ProductOccasion:BaseEntity
    {
        public int ProductId { get; set; }
        public int OccasionId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Occasion Occasion { get; set; }
    }
}
