using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildingBlocks.SharedEntities;

namespace Delivery_Service.Entities
{
    public class DeliveryHeroes : BaseEntity<int>
    {



        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(500)]
        public string PhotoUrl { get; set; }

        [Range(0, 5)]
        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; } = 5.0m;

        // Capacity & Availability
        public int MaxConcurrentDeliveries { get; set; } = 5;
        public int CurrentDeliveriesCount { get; set; } = 0;
        public bool IsAvailable { get; set; } = true;

    }
}
