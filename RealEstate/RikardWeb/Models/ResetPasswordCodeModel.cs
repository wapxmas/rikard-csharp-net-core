using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Models
{
    public class ResetPasswordCodeModel
    {
        [Display(Name = "Пароль:", Prompt = "пароль")]
        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Повторите пароль:", Prompt = "пароль")]
        [Required(ErrorMessage = "Введите пароль повторно")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Введенные пароли не совпадают")]
        public string ConfirmPassword { get; set; }

        public string Id { get; set; }
        public string Code { get; set; }
    }
}
