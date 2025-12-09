using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Catalog_Service_Entities
{
    public class ProductSpecification : BaseEntity
    {
        public int ProductId { get; set; }
        public string SpecKey { get; set; }
        public string SpecValue { get; set; }

        public virtual Product Product { get; set; }
    }
}