using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using RikardLib.Geo;
using RikardLib.Text;
using RikardWeb.Lib.Adverts.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.Adverts.DbModels
{
    public class AdRentLotModel
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string NotificationId;
        public string NotificationUrl;
        public string LotNum;
        public string OrganizationId;
        public string OrganizationName;
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ExpireDate;
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime AuctionDate;
        [BsonIgnoreIfNull]
        public string AuctionPlace;
        public int TermYears;
        public int TermMonths;
        public int TermDays;
        [BsonIgnoreIfNull]
        public string RegionAbbr;
        [BsonIgnoreIfNull]
        public string RegionFullName;
        [BsonIgnoreIfNull]
        public string DistrictName;
        [BsonIgnoreIfNull]
        public List<string> Rings;
        public string StreetName;
        public string StreetId;
        public string HouseNum;
        public string BuildNum;
        public string StrucNum;
        public string HouseId;
        public string MetroName;
        public string MetroLine;
        public string MetroEntrance;
        public double DistanceTowardsMetroEntrance;
        public string StationName;
        public string StationDirection;
        public string StationRoutes;
        public double DistanceTowardsStation;
        public double Area;
        public string AddrString;
        public double PricePerMonth;
        public double PricePerYear;
        public double ContractFee;
        [BsonIgnoreIfNull]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> GeoPoint { get; set; }
        [BsonIgnoreIfNull]
        public List<string> Floors;
        [BsonIgnoreIfNull]
        public List<string> Missions;
        [BsonIgnoreIfNull]
        public string MissionText;
        [BsonIgnoreIfNull]
        public int? Rooms;
        [BsonIgnoreIfNull]
        public List<AdRentLotFile> Files;
        //Additional
        public int RenewAuctionCount;
        [BsonIgnoreIfNull]
        public HouseInfo HouseInfo;
        [BsonIgnoreIfNull]
        public EnvironmentInfo EnvironmentInfo;

        public double[] GetWeights(
            double minArea, double maxArea, 
            double minPricePerYear, double maxPricePerYear,
            double minDistanceTowardsMetroEntrance, double maxDistanceTowardsMetroEntrance,
            double minDistanceTowardsStation, double maxDistanceTowardsStation,
            double minDistanceTowardsCenter, double maxDistanceTowardsCenter,
            double minDegreeBearingTowardsCenter, double maxDegreeBearingTowardsCenter,
            GeoJsonPoint<GeoJson2DGeographicCoordinates> moscowCenter)
        {
            var weights = new List<double>
            {
                (Area - minArea) / (maxArea - minArea),
                (PricePerYear - minPricePerYear) / (maxPricePerYear - minPricePerYear),
                (DistanceTowardsMetroEntrance - minDistanceTowardsMetroEntrance) /
                (maxDistanceTowardsMetroEntrance - minDistanceTowardsMetroEntrance),
                (DistanceTowardsStation - minDistanceTowardsStation) /
                (maxDistanceTowardsStation - minDistanceTowardsStation),
                (GeoHelpers.CalcDistance(GeoPoint, moscowCenter) - minDistanceTowardsCenter) /
                (maxDistanceTowardsCenter - minDistanceTowardsCenter),
                (GeoHelpers.DegreeBearing(GeoPoint, moscowCenter) - minDegreeBearingTowardsCenter)
                / (maxDegreeBearingTowardsCenter - minDegreeBearingTowardsCenter)
            };

            return weights.ToArray();
        }

        public string JustAddrString(bool isPaid = true)
        {
            var l = new List<string>();

            l.Add(StreetName);

            if(isPaid)
            {
                if (!string.IsNullOrWhiteSpace(HouseNum))
                {
                    l.Add($"д. {HouseNum}");
                }

                if (!string.IsNullOrWhiteSpace(BuildNum))
                {
                    l.Add($"к. {BuildNum}");
                }

                if (!string.IsNullOrWhiteSpace(StrucNum))
                {
                    l.Add($"стр. {StrucNum}");
                }
            }

            return string.Join(", ", l);
        }

        public string UnpaidAddr()
        {
            var l = new List<string>();

            if(!string.IsNullOrWhiteSpace(RegionAbbr))
            {
                l.Add(RegionAbbr);
            }

            if (!string.IsNullOrWhiteSpace(DistrictName))
            {
                l.Add(DistrictName);
            }

            if(!string.IsNullOrWhiteSpace(StreetName))
            {
                l.Add(StreetName);
            }

            return string.Join(", ", l);
        }

        public string MakeTermString()
        {
            var termsList = new List<string>();

            if (TermYears > 0)
            {
                termsList.Add($"{TermYears} {TextUtilites.GetDeclension(TermYears, "год", "года", "лет")}");
            }

            if (TermMonths > 0)
            {
                termsList.Add($"{TermMonths} {TextUtilites.GetDeclension(TermMonths, "месяц", "месяца", "месяцев")}");
            }

            if (TermDays > 0)
            {
                termsList.Add($"{TermDays} {TextUtilites.GetDeclension(TermDays, "день", "дня", "дней")}");
            }

            return string.Join(", ", termsList);
        }

        public bool IsAuctionExpired() => DateTime.Now > AuctionDate;
    }

    public class EnvironmentInfo
    {
        public int AmountOfLivingHousesAround1Km;
        public int AmountOfLivingHousesAround500m;
        public int AmountOfLivingApartmentsAround1Km;
        public int AmountOfLivingApartmentsAround500m;
        public int AmountOfStStationsAround1Km;
        public int AmountOfStStationsAround500m;
        public int AmountOfStRoutesAround1Km;
        public int AmountOfStRoutesAround500m;
        public int AmountOfMetroStationAround1km;
        public int AmountOfParkingAutomatesAround500m;
        public int AmountOfMetroParkingLotsAround2km;
        public int AmountOfMFCAround2km;
        public int AmountOfPostOfficesAround2km;
    }

    public class HouseInfo
    {
        [BsonIgnoreIfNull]
        public string ProjectName;
        [BsonIgnoreIfNull]
        public string NameOfAuthority;
        public int Floors;
        public int YearOfBuild;
        public int EntrancesCount;

        public bool IsNull()
        {
            return string.IsNullOrWhiteSpace(NameOfAuthority) && string.IsNullOrWhiteSpace(ProjectName) &&
                        YearOfBuild == 0 && Floors == 0 && EntrancesCount == 0;
        }
    }

    public class AdRentLotFile
    {
        public string Salt { get; set; } = Guid.NewGuid().ToString();
        public string Name;
        public string Filename;
        public bool LotFile;
    }
}
