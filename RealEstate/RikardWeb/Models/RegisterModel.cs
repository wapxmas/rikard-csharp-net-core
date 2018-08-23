using RikardWeb.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Models
{
    public class RegisterModel
    {
        [Display(Name = "Ваш телефон:", Prompt = "телефон")]
        [Required(ErrorMessage = "Укажите телефон")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Display(Name = "Ваш Email:", Prompt = "email")]
        [Required(ErrorMessage = "Укажите e-mail адрес")]
        [EmailAddress(ErrorMessage = "Неправильный e-mail адрес")]
        public string Email { get; set; }

        [Display(Name = "Пароль:", Prompt = "пароль")]
        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Повторите пароль:", Prompt = "пароль")]
        [Required(ErrorMessage = "Введите пароль повторно")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Введенные пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}
