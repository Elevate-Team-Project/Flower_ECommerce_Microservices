using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Catalog_Service_Entities
{
    public class ProductReview : BaseEntity
    {
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

        public virtual Product Product { get; set; }
    }
}