using Entity.DbSet;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Protocols.DTOs;

namespace Entity
{
    [Table("Account")]
    public class AccountDb
    {
        [Key]  // Primary Key
        public long account_id { get; set; }

        [Required]  // Not null
        [MaxLength(100)]
        public string account_name { get; set; }

        [Required]
        [MaxLength(100)]
        public string password { get; set; }
        public int level_code { get; set; }
        public int current_exp { get; set; }
        public int gold { get; set; } = 0;
        public int cash { get; set; } = 0;
        public int diamonds { get; set; } = 0;
        public ICollection<ItemDb>? items { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public AccountDto ToDto => new AccountDto
        {
            UserName = account_name,
            LevelCode = level_code,
            CurrentExp = current_exp,
        };

        [JsonIgnore]
        public CurrencyInfoDto ToCurrencyDto => new CurrencyInfoDto
        {
            Gold = gold,
            Cash = cash,
            Diamonds = diamonds,
        };
    }
}
