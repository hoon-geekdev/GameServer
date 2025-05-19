using Newtonsoft.Json;
using Protocols.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.DbSet
{
    [Table("Item")]
    public class ItemDb
    {
        // auto increment
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long item_id { get; set; }
        public long account_id { get; set; }
        public long? equipped_character_id { get; set; }
        public int item_code { get; set; }
        public int evolve_code { get; set; }
        public int level_code { get; set; }
        public int item_count { get; set; }

        // ignore json serialization
        [JsonIgnore]
        public ItemDto ToDto => new ItemDto
        {
            ItemId = item_id,
            EquippedCharacterId = equipped_character_id,
            ItemCode = item_code,
            ItemCount = item_count,
            EvolveCode = evolve_code,
            LevelCode = level_code,
        };
    }
}
