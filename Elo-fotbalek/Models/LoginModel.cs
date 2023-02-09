using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class LoginModel : BaseModel
    {
        [Required]
        [Display(Name = "Heslo")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Zapamatuj si mě?")]
        public bool RememberMe { get; set; }
    }
}
