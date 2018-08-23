using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.Data
{
    public class AddrPlace
    {
        public AddrPlace(string house, string build, string struc)
        {
            AddrHouseNum = house;
            AddrBuildNum = build;
            AddrStrucNum = struc;
        }

        public override string ToString()
        {
            if (IsEmpty())
            {
                return "<Empty>";
            }
            else
            {
                var l = new List<string>();

                if (!string.IsNullOrWhiteSpace(AddrHouseNum))
                {
                    l.Add($"д. {AddrHouseNum}");
                }

                if (!string.IsNullOrWhiteSpace(AddrBuildNum))
                {
                    l.Add($"к. {AddrBuildNum}");
                }

                if (!string.IsNullOrWhiteSpace(AddrStrucNum))
                {
                    l.Add($"стр. {AddrStrucNum}");
                }

                return string.Join(", ", l);
            }
        }

        public bool IsEmpty()
        {
            return
                string.IsNullOrWhiteSpace(AddrHouseNum) &&
                string.IsNullOrWhiteSpace(AddrBuildNum) &&
                string.IsNullOrWhiteSpace(AddrStrucNum);
        }

        public string AddrHouseNum { get; private set; }
        public string AddrBuildNum { get; private set; }
        public string AddrStrucNum { get; private set; }
    }
}
