using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RikardWeb.Data;

namespace RikardWeb.Models
{
    public class SendLetterModel
    {
        [Display(Name = "Ваш телефон:", Prompt = "телефон")]
        [Required(ErrorMessage = "Укажите телефон")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Display(Name = "Ваш Email:", Prompt = "email")]
        [Required(ErrorMessage = "Укажите e-mail адрес")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Текст письма:", Prompt = "текст письма")]
        [Required(ErrorMessage = "Введите текст письма")]
        public string Text { get; set; }

        public EmailLetter GenEmailLetter(string newBody = null)
        {
            return new EmailLetter
            {
                To = "yashin.sergey@gmail.com",
                Subject = "Письмо",
                Body = newBody ?? $"E-Mail: {Email}\nТелефон: {Phone}\n\n{Text}"
            };
        }
    }
}
