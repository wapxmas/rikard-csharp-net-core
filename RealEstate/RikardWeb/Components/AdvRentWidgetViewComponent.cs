using Microsoft.AspNetCore.Mvc;
using RikardWeb.Lib.Adverts.DbModels;
using RikardWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Components
{
    public class AdvRentWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(AdRentLotModel lotModel, bool isPaid)
        {
            return View(new RentLotWidgetModel
            {
                LotModel = lotModel,
                IsPaid = isPaid
            });
        }
    }
}
