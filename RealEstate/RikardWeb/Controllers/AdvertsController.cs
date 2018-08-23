using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RikardWeb.Options;
using Microsoft.Extensions.Options;
using RikardWeb.Lib.Adverts.Options;
using RikardWeb.Lib.Adverts.Service;
using System.IO;
using RikardWeb.Lib.Adverts;
using HeyRed.Mime;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace RikardWeb.Controllers
{
    public class AdvertsController : Controller
    {
        private readonly IOptions<AdvertsOptions> advertsOptions;
        private readonly IAdvertsDatabaseService advertsService;

        public AdvertsController(IOptions<AdvertsOptions> advertsOptions, IAdvertsDatabaseService advertsService)
        {
            this.advertsOptions = advertsOptions;
            this.advertsService = advertsService;
        }

        [Authorize("Paid")]
        public IActionResult RentListMap()
        {
            return View();
        }

        public IActionResult RentList()
        {
            return View();
        }

        public async Task<IActionResult> RentAdvert(string id)
        {
            var adv = await advertsService.GetAdRentLotByIdTryArch(id);

            if(adv != null)
            {
                return View(adv);
            }

            return NotFound();
        }

        public IActionResult PropertyList()
        {
            return View();
        }

        public IActionResult PropertyListMap()
        {
            return View();
        }

        public IActionResult LandList()
        {
            return View();
        }

        public IActionResult LandListMap()
        {
            return View();
        }

        [Authorize("Paid")]
        public async Task<IActionResult> DownloadRentFile(string id, string advid, int lotnum, bool lotfile)
        {
            var adv = await advertsService.GetAdRentLotByIdTryArch(advid);

            if (adv != null)
            {
                var file = adv.Files.Where(_ => _.Salt == id).FirstOrDefault();

                if (file != null)
                {
                    var nId = AdvHelpers.GetDirectoryId(adv.NotificationUrl);

                    var filePath = $"{advertsOptions.Value.RootDirectory}{Path.DirectorySeparatorChar}{advertsOptions.Value.FilesDirectory}{Path.DirectorySeparatorChar}{advertsOptions.Value.RentDirectory}{Path.DirectorySeparatorChar}{nId.DirectoryName}{Path.DirectorySeparatorChar}{nId.Id}";

                    if (lotfile)
                    {
                        filePath = $"{filePath}{Path.DirectorySeparatorChar}{lotnum}";
                    }

                    filePath = $"{filePath}{Path.DirectorySeparatorChar}{file.Filename}";

                    var mime = MimeTypesMap.GetMimeType(file.Filename);

                    if(System.IO.File.Exists(filePath))
                    {
                        return File(System.IO.File.ReadAllBytes(filePath), mime, file.Name);
                    }
                }
            }

            return new NotFoundResult();
        }
    }
}
