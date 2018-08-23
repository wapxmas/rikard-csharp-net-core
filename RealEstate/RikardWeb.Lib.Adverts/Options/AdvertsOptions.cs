using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.Options
{
    public class AdvertsOptions
    {
        public string RootDirectory { get; set; }
        public string NfsDirectory { get; set; }
        public string FilesDirectory { get; set; }
        public string RentDirectory { get; set; }
        public int AdvertsPerPage { get; set; }
    }
}
