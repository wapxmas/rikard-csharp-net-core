using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Models
{
    public class ResetPasswordModel
    {
        [Display(Name = "Email:", Prompt = "email")]
        [Required(ErrorMessage = "Укажите e-mail адрес")]
        [EmailAddress(ErrorMessage = "Неправильный e-mail адрес")]
        public string Email { get; set; }
    }
}
