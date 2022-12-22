using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BookCommerceCustom1.Models
{
    [Table("Produkti")]
    public class Produkti
    {
        public int Id { get; set; }
        [Required]
        public string Emri { get; set; }
        public string Pershkrimi { get; set; }
        [Required(ErrorMessage = "Specifikoni isbn")]
        public string Isbn { get; set; }
        [Required(ErrorMessage = "Specifikoni autorin")]
        public string Autori { get; set; }
        [Required(ErrorMessage = "Specifikoni cmimin baze")]
        [Range(1, 10000)]
        [Display(Name = "Cmimi baze")]
        public double CmimiBaze { get; set; }
        [Required(ErrorMessage = "Specifikoni cmimin per 50")]
        [Range(1, 10000)]
        [Display(Name = "Cmimi per 50")]
        public double Cmimi { get; set; }

        [Required(ErrorMessage = "Specifikoni cmimin per 51 deri 100")]
        [Range(1, 10000)]
        [Display(Name = "Cmimi per 51-100")]
        public double Cmimi50 { get; set; }

        [Required]
        [Display(Name = "Price for 100+")]
        [Range(1, 10000)]
        public double Cmimi100 { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }

        [Required]
        [Display(Name = "Kategoria")]
        public int KategoriaId { get; set; }
        [ForeignKey("KategoriaId")]
        [ValidateNever]
        public Kategoria Kategoria { get; set; }

        [Required]
        [Display(Name = "Mbulesa")]
        public int MbulesaId { get; set; }
        [ValidateNever]
        public Mbulesa Mbulesa { get; set; }
    }
}
