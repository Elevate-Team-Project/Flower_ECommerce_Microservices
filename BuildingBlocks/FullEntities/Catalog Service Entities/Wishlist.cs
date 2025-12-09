using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Catalog_Service_Entities
{
    public class Wishlist : BaseEntity
    {
        public string UserId { get; set; }
        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}
