using BuildingBlocks.SharedEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.FullEntities.Audit_Service
{
    public class AuditLog : BaseEntity
    {
        public string UserId { get; set; } // مين اللي عمل الحركة
        public string ServiceName { get; set; } // Catalog, Ordering, Identity...
        public string Action { get; set; } // "UpdatePrice", "PlaceOrder", "Login"
        public string EntityId { get; set; } // رقم الاوردر او المنتج
        public string OldValues { get; set; } // JSON (السعر كان 100)
        public string NewValues { get; set; } // JSON (السعر بقى 120)

    }
}
