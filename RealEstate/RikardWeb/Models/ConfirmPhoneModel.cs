using RikardWeb.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Models
{
    public class ConfirmPhoneModel
    {
        [Display(Name = "Ваш телефон:", Prompt = "телефон")]
        [Required(ErrorMessage = "Укажите телефон")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        public string Id { get; set; }
    }
}
