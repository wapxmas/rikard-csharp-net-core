using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Models
{
    public class ConfirmPhoneCheckModel
    {
        [Display(Name = "Код:", Prompt = "код")]
        [Required(ErrorMessage = "Укажите код")]
        public string Code { get; set; }

        public string Id { get; set; }
    }
}
