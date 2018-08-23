using RikardWeb.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Models
{
    public class LoginModel
    {
        [Display(Name = "Email:", Prompt = "email")]
        [Required(ErrorMessage = "Укажите e-mail адрес")]
        [EmailAddress(ErrorMessage = "Неправильный e-mail адрес")]
        public string Email { get; set; }

        [Display(Name = "Пароль:", Prompt = "пароль")]
        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
