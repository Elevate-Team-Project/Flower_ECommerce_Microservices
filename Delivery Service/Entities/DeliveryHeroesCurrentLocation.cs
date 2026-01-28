using System.ComponentModel.DataAnnotations.Schema;

namespace Delivery_Service.Entities
{
    public class DeliveryHeroesCurrentLocation
    {
        public int Id { get; set; }
        public int DeliveryHeroId { get; set; }

        [Column(TypeName = "decimal(10,8)")]
        public double? CurrentLatitude { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public double? CurrentLongitude { get; set; }
        [ForeignKey("DeliveryHeroId")]
        public virtual DeliveryHeroes Hero { get; set; }

    } 
}
