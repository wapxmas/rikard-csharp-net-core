using RikardWeb.Lib.Db;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using RikardWeb.Lib.Adverts.DbModels;
using System.Threading.Tasks;
using RikardLib.Text;
using System.Linq;
using System.Globalization;
using RikardLib.Parallel;
using RikardLib.Log;
using Microsoft.Extensions.Options;
using RikardWeb.Lib.Adverts.Options;
using System.IO;
using System.Net;
using RikardLib.Web;
using RikardWeb.Lib.Adverts.Data;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.WebUtilities;
using HeyRed.Mime;
using System.Net.Http;
using System.Text.RegularExpressions;
using RikardLib.Geo;
using MongoDB.Bson;
using RikardLib.Maths;
using MongoDB.Driver.GeoJsonObjectModel;

namespace RikardWeb.Lib.Adverts
{
    public class AdvertsDatabase
    {
        private readonly Logger logger = new Logger();

        private static volatile object _instanceLock = new object();

        private static AdvertsDatabase _instance;

        private readonly DatabaseFactory databaseFactory;
        private readonly IMongoDatabase advertsDatabase;

        private const string addrHouseCollectionName = "addrhouse";
        private const string addrStreetCollectionName = "addrstreet";
        private const string geoAoCollectionName = "geo_ao";
        private const string geoMoCollectionName = "geo_mo";
        private const string geoRingsCollectionName = "geo_rings";
        private const string metroEntrancesCollectionName = "metro_entrances";
        private const string stStationsCollectionName = "st_stations";
        private const string parkingAutomatesCollectionName = "parking_automates";
        private const string metroParkingLotsCollectionName = "metro_parking_lots";
        private const string mfcOfficesCollectionName = "mfc_offices";
        private const string postOfficesCollectionName = "post_offices";

        private const string adRentLotCollectionName = "adrentlot";
        private const string adRentLotArchCollectionName = "adrentlot-arch";

        private const string DownloadDocumentUrl = "http://torgi.gov.ru/resources/org.apache.wicket.Application/downloadableResource?class=Document";
        private const string DownloadLotPhotoUrl = "http://torgi.gov.ru/resources/org.apache.wicket.Application/downloadableResource?class=LotPhoto";
        private const string DownloadRosimActUrl = "http://torgi.gov.ru/resources/org.apache.wicket.Application/downloadableResource?class=RosimActs";
        private const string DownloadFasDataActUrl = "http://torgi.gov.ru/resources/org.apache.wicket.Application/downloadableResource?class=FasData";
        private const string DownloadConsultDocumentsUrl = "http://torgi.gov.ru/resources/org.apache.wicket.Application/downloadableResource?class=ConsultDocuments";

        private const string TorgiUrlRent = "http://torgi.gov.ru/opendata/7710349494-torgi/data-1-{DATE2}T{TIME2}-{DATE}T{TIME}-structure-20130401T0000.xml";
        private readonly string TorgiRentList;
        private readonly string TorgiRentNfs;
        private readonly string TorgiRentFiles;

        private readonly ParallelWorker worker;
        private readonly IOptions<AdvertsOptions> advertsOptions;

        public static AdvertsDatabase Instance
        {
            get
            {
                return _instance;
            }
        }

        private AdvertsDatabase(DatabaseFactory databaseFactory, IOptions<AdvertsOptions> advertsOptions)
        {
            this.databaseFactory = databaseFactory;
            this.advertsDatabase = databaseFactory.MongoDatabase;
            this.advertsOptions = advertsOptions;

            TorgiRentList = $"{advertsOptions.Value.RootDirectory}{Path.DirectorySeparatorChar}{advertsOptions.Value.RentDirectory}.xml";
            TorgiRentNfs = $"{advertsOptions.Value.RootDirectory}{Path.DirectorySeparatorChar}{advertsOptions.Value.NfsDirectory}{Path.DirectorySeparatorChar}{advertsOptions.Value.RentDirectory}";
            TorgiRentFiles = $"{advertsOptions.Value.RootDirectory}{Path.DirectorySeparatorChar}{advertsOptions.Value.FilesDirectory}{Path.DirectorySeparatorChar}{advertsOptions.Value.RentDirectory}";

            worker = new ParallelWorker(1);
        }

        public void RunAdvertsUpdater()
        {
            worker.AddWork(() => Task.Run(async () => await RunAdvertsUpdaterAsync()).GetAwaiter().GetResult());
        }

        public async Task<List<AdRentLotModel>> GetSimilarAdRentLot(AdRentLotModel adv, int limit = 3)
        {
            var adRentLotCollection = advertsDatabase.GetCollection<AdRentLotModel>(adRentLotCollectionName);

            var adverts = await adRentLotCollection.Find(Builders<AdRentLotModel>.Filter.Empty).ToListAsync();

            var moscowCenter = GeoJson.Point(GeoJson.Geographic(37.609218, 55.753559));

            double minArea = adverts.Select(_ => _.Area).Min();
            double maxArea = adverts.Select(_ => _.Area).Max();
            double minPricePerYear = adverts.Select(_ => _.PricePerYear).Min();
            double maxPricePerYear = adverts.Select(_ => _.PricePerYear).Max();
            double minDistanceTowardsMetroEntrance = adverts.Select(_ => _.DistanceTowardsMetroEntrance).Min();
            double maxDistanceTowardsMetroEntrance = adverts.Select(_ => _.DistanceTowardsMetroEntrance).Max();
            double minDistanceTowardsStation = adverts.Select(_ => _.DistanceTowardsStation).Min();
            double maxDistanceTowardsStation = adverts.Select(_ => _.DistanceTowardsStation).Max();
            double minDistanceTowardsCenter = adverts.Select(_ => GeoHelpers.CalcDistance(_.GeoPoint, moscowCenter)).Min();
            double maxDistanceTowardsCenter = adverts.Select(_ => GeoHelpers.CalcDistance(_.GeoPoint, moscowCenter)).Max();
            double minDegreeBearingTowardsCenter = adverts.Select(_ => GeoHelpers.DegreeBearing(_.GeoPoint, moscowCenter)).Min();
            double maxDegreeBearingTowardsCenter = adverts.Select(_ => GeoHelpers.DegreeBearing(_.GeoPoint, moscowCenter)).Max();

            return adverts.OrderByDescending(_ => MathUtils.CosineSimilarity(
                adv.GetWeights(minArea, maxArea, minPricePerYear, maxPricePerYear, 
                minDistanceTowardsMetroEntrance, maxDistanceTowardsMetroEntrance,
                minDistanceTowardsStation, maxDistanceTowardsStation,
                minDistanceTowardsCenter, maxDistanceTowardsCenter,
                minDegreeBearingTowardsCenter, maxDegreeBearingTowardsCenter,
                moscowCenter), 
                _.GetWeights(minArea, maxArea, minPricePerYear, maxPricePerYear,
                minDistanceTowardsMetroEntrance, maxDistanceTowardsMetroEntrance,
                minDistanceTowardsStation, maxDistanceTowardsStation,
                minDistanceTowardsCenter, maxDistanceTowardsCenter,
                minDegreeBearingTowardsCenter, maxDegreeBearingTowardsCenter,
                moscowCenter)))
                .GroupBy(_ => _.StreetId).Select(_ => _.First()).Where(_ => _.Id != adv.Id).Take(limit).ToList();
        }

        public async Task<List<AdRentLotModel>> GetNearestAdRentLot(AdRentLotModel adv, int limit = 3)
        {
            var adRentLotCollection = advertsDatabase.GetCollection<AdRentLotModel>(adRentLotCollectionName);

            var filterAdverts = 
                Builders<AdRentLotModel>.Filter.NearSphere(_ => _.GeoPoint, adv.GeoPoint) &
                Builders<AdRentLotModel>.Filter.Ne(_ => _.Id, adv.Id) &
                Builders<AdRentLotModel>.Filter.Ne(_ => _.HouseId, adv.HouseId);

            var list = new List<AdRentLotModel>();
            var listSet = new HashSet<string>();

            using (var cursor = await adRentLotCollection.FindAsync(filterAdverts))
            {
                while(await cursor.MoveNextAsync())
                {
                    foreach(var a in cursor.Current)
                    {
                        if(!listSet.Contains(a.StreetId))
                        {
                            list.Add(a);
                            listSet.Add(a.StreetId);

                            if (list.Count >= limit)
                            {
                                break;
                            }
                        }
                    }

                    if (list.Count >= limit)
                    {
                        break;
                    }
                }
            }

            return list;
        }

        public Task<AdRentLotModel> GetAdRentLotById(string id)
        {
            var adRentLotCollection = advertsDatabase.GetCollection<AdRentLotModel>(adRentLotCollectionName);
            return adRentLotCollection.Find(_ => _.Id == id).FirstOrDefaultAsync();
        }

        public async Task<AdRentLotModel> GetAdRentLotByIdTryArch(string id)
        {
            var adRentLotCollection = advertsDatabase.GetCollection<AdRentLotModel>(adRentLotCollectionName);
            var adRentLotArchCollection = advertsDatabase.GetCollection<AdRentLotModel>(adRentLotArchCollectionName);

            var adv = await adRentLotCollection.Find(_ => _.Id == id).FirstOrDefaultAsync();

            if (adv == null)
            {
                adv = await adRentLotArchCollection.Find(_ => _.Id == id).FirstOrDefaultAsync();
            }

            return adv;
        }

        public async Task<List<AdRentLotModel>> GetAdRentLots(AdvRentSearchModel searchModel, int skip = 0, int limit = 0)
        {
            var adRentLotCollection = advertsDatabase.GetCollection<AdRentLotModel>(adRentLotCollectionName);

            int? _limit = null;

            if(limit > 0)
            {
                _limit = limit;
            }

            int? _skip = null;

            if (skip > 0)
            {
                _skip = skip;
            }

            var filter = Builders<AdRentLotModel>.Filter.Empty;

            if(!string.IsNullOrWhiteSpace(searchModel.Disctrict))
            {
                filter = filter & Builders<AdRentLotModel>.Filter.Eq(_ => _.RegionAbbr, searchModel.Disctrict);
            }

            if(!string.IsNullOrWhiteSpace(searchModel.Area))
            {
                double from, to = 0;

                var fts = searchModel.Area.SplitNoEmptyTrim(new char[] { '-' });

                if(fts.Count() == 2)
                {
                    double.TryParse(fts.First(), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out from);
                    double.TryParse(fts.Last(), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out to);

                    filter = filter & Builders<AdRentLotModel>.Filter.Where(_ => _.Area >= from && _.Area < to);
                }
            }

            if(!string.IsNullOrWhiteSpace(searchModel.Purpose))
            {
                if(AdvConstants.InversePurposesMap.TryGetValue(searchModel.Purpose, out string purpose))
                {
                    filter = filter & Builders<AdRentLotModel>.Filter.AnyEq(_ => _.Missions, purpose);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Floor))
            {
                filter = filter & Builders<AdRentLotModel>.Filter.AnyEq(_ => _.Floors, searchModel.Floor);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Location))
            {
                switch(searchModel.Location)
                {
                    case "within-mkad":
                        filter = filter & Builders<AdRentLotModel>.Filter.AnyEq(_ => _.Rings, "mkad");
                        break;
                    case "outside-mkad":
                        filter = filter & Builders<AdRentLotModel>.Filter.Size(_ => _.Rings, 0);
                        break;
                    case "sadovoe":
                    case "ttk":
                        filter = filter & Builders<AdRentLotModel>.Filter.AnyEq(_ => _.Rings, searchModel.Location);
                        break;
                }
            }

            return await adRentLotCollection.Find(filter).Skip(_skip).Limit(_limit).SortByDescending(_ => _.CreatedDate).ToListAsync();
        }

        public static AdvertsDatabase Init(DatabaseFactory databaseFactory, IOptions<AdvertsOptions> advertsOptions)
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new AdvertsDatabase(databaseFactory, advertsOptions);
                    }
                }
            }

            return _instance;
        }

        private async Task RunAdvertsUpdaterAsync()
        {
            Directory.CreateDirectory(advertsOptions.Value.RootDirectory);
            Directory.CreateDirectory(TorgiRentNfs);
            Directory.CreateDirectory(TorgiRentFiles);

            await DownloadAdvertsInfo(TorgiUrlRent, TorgiRentList, 1);
            await DownloadAdvertsData(TorgiRentList, TorgiRentNfs);
            await DownloadAdvertsFiles(TorgiRentList, TorgiRentNfs, TorgiRentFiles);
            await UpdateRentAdvertsData(TorgiRentList, TorgiRentNfs, TorgiRentFiles);
        }

        private async Task UpdateRentAdvertsData(string advertsListFile, string advertsDataDestDir, string advertsDataFilesDir)
        {
            logger.Info($"Processing {advertsListFile}");

            var addrHouseCollection = advertsDatabase.GetCollection<AddrHouseModel>(addrHouseCollectionName);
            var adRentLotCollection = advertsDatabase.GetCollection<AdRentLotModel>(adRentLotCollectionName);
            var adRentLotArchCollection = advertsDatabase.GetCollection<AdRentLotModel>(adRentLotArchCollectionName);

            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Geo2DSphere(_ => _.GeoPoint));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.NotificationId));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.RegionAbbr));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.DistrictName));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.StreetId));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.MetroName));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.MetroLine));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.Area));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.Missions));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.ExpireDate));
            adRentLotCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.OrganizationId));

            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Geo2DSphere(_ => _.GeoPoint));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.NotificationId));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.RegionAbbr));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.DistrictName));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.StreetId));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.MetroName));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.MetroLine));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.Area));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.Missions));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.ExpireDate));
            adRentLotArchCollection.Indexes.CreateOne(Builders<AdRentLotModel>.IndexKeys.Ascending(_ => _.OrganizationId));

            if (File.Exists(advertsListFile))
            {
                int houseamount = 0;
                int strplaceamount = 0;

                foreach (var n in XmlUtilites.ReadXmlStream<NotificationInfo>(advertsListFile, "notification"))
                {
                    if (!string.IsNullOrWhiteSpace(n.OdDetailedHref))
                    {
                        var nId = AdvHelpers.GetDirectoryId(n.OdDetailedHref);
                        var nDir = $"{advertsDataDestDir}{Path.DirectorySeparatorChar}{nId.DirectoryName}";
                        var nFile = $"{nDir}{Path.DirectorySeparatorChar}{nId.Id}";

                        if (File.Exists(nFile))
                        {
                            try
                            {
                                var prepared = File.ReadAllText(nFile)
                                    .Replace("&#11;", "")
                                    .Replace("&#13;", "")
                                    .Replace("&#31;", "")
                                    .Replace("&#19;", "");

                                var nd = XmlUtilites.DeserializeXmlString<Notification>(prepared);

                                bool isNotificationFresh = nd.CheckNotificationFresh();

                                foreach (var lot in nd?.Data?.Lots)
                                {
                                    if (lot?.KladrLocation?.Id?.StartsWith("77") == false || lot?.KladrLocation?.Id == null)
                                    {
                                        continue;
                                    }

                                    if (lot?.PropertyType?.Name?.StartsWith("Недвижимое имущество;") == false || lot?.PropertyType?.Name == null)
                                    {
                                        continue;
                                    }

                                    var adv = await adRentLotCollection.Find(
                                        Builders<AdRentLotModel>.Filter.Eq(_ => _.NotificationId, nd?.Data?.BidNumber) &
                                        Builders<AdRentLotModel>.Filter.Eq(_ => _.LotNum, lot.LotNum)).FirstOrDefaultAsync();

                                    if (!DateTime.TryParseExact(nd.Data?.Common?.ExpireDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime expireDate))
                                    {
                                        DateTime.TryParseExact(nd.Data?.Common?.ExpireDate, "yyyy-MM-ddTHH:mm:ss", null, DateTimeStyles.None, out expireDate);
                                    }

                                    if (!DateTime.TryParseExact(nd.Data?.Common?.BidAuctionDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime auctionDate))
                                    {
                                        DateTime.TryParseExact(nd.Data?.Common?.BidAuctionDate, "yyyy-MM-ddTHH:mm:ss", null, DateTimeStyles.None, out auctionDate);
                                    }

                                    if (!isNotificationFresh && adv == null)
                                    {
                                        continue;
                                    }

                                    if (auctionDate < expireDate && adv == null)
                                    {
                                        logger.Info($"INCORRDATES! {nd.Data.BidNumber} ### {lot?.Location} ### {lot?.Description}");
                                        continue;
                                    }

                                    var missions = GetNotificationLotMissions(lot);
                                    var addrPlace = GetNotificationLotPlace(lot);
                                    var addrStreet = await GetNotificationStreet(lot);

                                    if (!addrPlace.IsEmpty() && addrStreet != null)
                                    {
                                        var addrHouse = await addrHouseCollection.Find(_ =>
                                            _.StreetId == addrStreet.Id &&
                                            _.HouseNum == addrPlace.AddrHouseNum &&
                                            _.BuildNum == addrPlace.AddrBuildNum &&
                                            _.StrucNum == addrPlace.AddrStrucNum)
                                            .FirstOrDefaultAsync();

                                        if (addrHouse == null && string.IsNullOrWhiteSpace(addrPlace.AddrStrucNum))
                                        {
                                            addrHouse = await addrHouseCollection.Find(_ =>
                                                _.StreetId == addrStreet.Id &&
                                                _.HouseNum == addrPlace.AddrHouseNum &&
                                                _.BuildNum == addrPlace.AddrBuildNum &&
                                                _.StrucNum == "1")
                                                .FirstOrDefaultAsync();
                                        }

                                        if (addrHouse != null)
                                        {
                                            if (addrHouse.Centroid != null)
                                            {
                                                if (!string.IsNullOrWhiteSpace(lot?.LotNum))
                                                {
                                                    var fls = AdvHelpers.ExtractFloor(new List<string>() { lot?.Location, lot?.Description });
                                                    var rms = AdvHelpers.ExtractRoomsCount(lot?.Location, lot?.Description);

                                                    var region = await GetAddrHouseRegion(addrHouse);
                                                    var district = await GetAddrHouseDistrict(addrHouse);
                                                    var rings = await GetAddrHouseRings(addrHouse);
                                                    var metro = await GetAddrHouseMetro(addrHouse);
                                                    var station = await GetAddrHouseSTStation(addrHouse);
                                                    var houseInfo = GetHouseInfo(addrHouse);
                                                    var environmentInfo = await GetEnvironmentInfo(addrHouse);

                                                    double.TryParse(lot.AreaMeters, NumberStyles.Number, CultureInfo.InvariantCulture, out double area);

                                                    var addrString = MakeAddrString();

                                                    int? rooms = null;

                                                    if (rms > 0)
                                                    {
                                                        rooms = rms;
                                                    }

                                                    var files = GetAdLotFiles(n.OdDetailedHref, lot, advertsDataFilesDir);

                                                    double pricePerMonth = 0;
                                                    double pricePerYear = 0;

                                                    if (lot?.Article?.Id == 1)
                                                    {
                                                        double.TryParse(lot.MonthPrice, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out pricePerMonth);
                                                        pricePerYear = pricePerMonth * 12;
                                                    }
                                                    else if (lot?.Article?.Id == 2)
                                                    {
                                                        double.TryParse(lot.YearPrice, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out pricePerYear);
                                                        pricePerMonth = pricePerYear / 12;
                                                    }

                                                    double.TryParse(lot.ContractFee, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out double contractFee);

                                                    var newAdv = new AdRentLotModel
                                                    {
                                                        NotificationId = nd?.Data?.BidNumber,
                                                        NotificationUrl = n?.OdDetailedHref,
                                                        LotNum = lot.LotNum,
                                                        OrganizationId = nd?.Data?.BidOrganization?.OrganizationId,
                                                        OrganizationName = nd?.Data?.BidOrganization?.FullName,
                                                        ExpireDate = expireDate,
                                                        AuctionDate = auctionDate,
                                                        AuctionPlace = nd.Data?.Common?.BidAuctionPlace,
                                                        TermYears = lot.TermYear,
                                                        TermMonths = lot.TermMonth,
                                                        TermDays = lot.TermDay,
                                                        RegionAbbr = region.RegionAbbr,
                                                        RegionFullName = region.RegionName,
                                                        DistrictName = district,
                                                        Rings = rings,
                                                        StreetName = addrStreet.OffName,
                                                        StreetId = addrStreet.Id,
                                                        HouseNum = addrHouse.HouseNum,
                                                        BuildNum = addrHouse.BuildNum,
                                                        StrucNum = addrHouse.StrucNum,
                                                        HouseId = addrHouse.Id,
                                                        MetroName = metro.Name,
                                                        MetroLine = metro.Line,
                                                        MetroEntrance = metro.MetroEntrance,
                                                        DistanceTowardsMetroEntrance = metro.DistanceTowardsMetroEntrance,
                                                        StationName = station.StationName,
                                                        StationDirection = station.StationDirection,
                                                        StationRoutes = station.StationRoutes,
                                                        DistanceTowardsStation = station.DistanceTowardsStation,
                                                        Area = area,
                                                        AddrString = addrString,
                                                        PricePerMonth = pricePerMonth,
                                                        PricePerYear = pricePerYear,
                                                        ContractFee = contractFee,
                                                        GeoPoint = addrHouse.Centroid,
                                                        Floors = fls,
                                                        Missions = missions.Select(_ => _.ToString()).ToList(),
                                                        MissionText = lot.Mission,
                                                        Rooms = rooms,
                                                        Files = files,
                                                        HouseInfo = houseInfo,
                                                        EnvironmentInfo = environmentInfo
                                                    };

                                                    if (adv == null)
                                                    {
                                                        if (isNotificationFresh)
                                                        {
                                                            await adRentLotCollection.InsertOneAsync(newAdv);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (adv.AuctionDate != newAdv.AuctionDate)
                                                        {
                                                            var filter = Builders<AdRentLotModel>.Filter.Eq(_ => _.Id, adv.Id);
                                                            var update = Builders<AdRentLotModel>.Update
                                                                .Set(_ => _.AuctionDate, newAdv.AuctionDate)
                                                                .Set(_ => _.ExpireDate, newAdv.ExpireDate)
                                                                .Set(_ => _.RenewAuctionCount, adv.RenewAuctionCount + 1);

                                                            await adRentLotCollection.UpdateOneAsync(filter, update);

                                                            logger.Info($"{adv.NotificationId}#{adv.LotNum} is renewed.");
                                                        }
                                                        else
                                                        {
                                                            if (IsAdvertOutOfDate(adv.AuctionDate))
                                                            {
                                                                await adRentLotArchCollection.InsertOneAsync(adv);

                                                                await adRentLotCollection.DeleteOneAsync(_ => _.Id == adv.Id);

                                                                logger.Info($"{adv.NotificationId}#{adv.LotNum} is archived.");
                                                            }
                                                        }

                                                        //updates to advert info
                                                        if (string.IsNullOrWhiteSpace(adv.MetroEntrance))
                                                        {
                                                            var filter = Builders<AdRentLotModel>.Filter.Eq(_ => _.Id, adv.Id);
                                                            var update = Builders<AdRentLotModel>.Update
                                                                .Set(_ => _.MetroEntrance, newAdv.MetroEntrance)
                                                                .Set(_ => _.DistanceTowardsMetroEntrance, newAdv.DistanceTowardsMetroEntrance);
                                                            await adRentLotCollection.UpdateOneAsync(filter, update);
                                                        }

                                                        if (string.IsNullOrWhiteSpace(adv.StationName))
                                                        {
                                                            var filter = Builders<AdRentLotModel>.Filter.Eq(_ => _.Id, adv.Id);
                                                            var update = Builders<AdRentLotModel>.Update
                                                                .Set(_ => _.StationName, newAdv.StationName)
                                                                .Set(_ => _.StationDirection, newAdv.StationDirection)
                                                                .Set(_ => _.StationRoutes, newAdv.StationRoutes)
                                                                .Set(_ => _.DistanceTowardsStation, newAdv.DistanceTowardsStation);
                                                            await adRentLotCollection.UpdateOneAsync(filter, update);
                                                        }

                                                        if (adv.HouseInfo == null && newAdv.HouseInfo != null)
                                                        {
                                                            var filter = Builders<AdRentLotModel>.Filter.Eq(_ => _.Id, adv.Id);
                                                            var update = Builders<AdRentLotModel>.Update
                                                                .Set(_ => _.HouseInfo, newAdv.HouseInfo);
                                                            await adRentLotCollection.UpdateOneAsync(filter, update);
                                                        }

                                                        if (string.IsNullOrWhiteSpace(adv.MissionText) &&
                                                            !string.IsNullOrWhiteSpace(newAdv.MissionText))
                                                        {
                                                            var filter = Builders<AdRentLotModel>.Filter.Eq(_ => _.Id, adv.Id);
                                                            var update = Builders<AdRentLotModel>.Update
                                                                .Set(_ => _.MissionText, newAdv.MissionText);
                                                            await adRentLotCollection.UpdateOneAsync(filter, update);
                                                        }

                                                        if (adv.ContractFee < 1 && newAdv.ContractFee > 0)
                                                        {
                                                            var filter = Builders<AdRentLotModel>.Filter.Eq(_ => _.Id, adv.Id);
                                                            var update = Builders<AdRentLotModel>.Update
                                                                .Set(_ => _.ContractFee, newAdv.ContractFee);
                                                            await adRentLotCollection.UpdateOneAsync(filter, update);
                                                        }

                                                        if (adv.EnvironmentInfo == null && newAdv.EnvironmentInfo != null)
                                                        {
                                                            var filter = Builders<AdRentLotModel>.Filter.Eq(_ => _.Id, adv.Id);
                                                            var update = Builders<AdRentLotModel>.Update
                                                                .Set(_ => _.EnvironmentInfo, newAdv.EnvironmentInfo);
                                                            await adRentLotCollection.UpdateOneAsync(filter, update);
                                                        }
                                                    }

                                                    string MakeAddrString()
                                                    {
                                                        var piecesList = new List<string>();

                                                        if (!string.IsNullOrWhiteSpace(region.RegionAbbr))
                                                        {
                                                            piecesList.Add(region.RegionAbbr);
                                                        }

                                                        if (!string.IsNullOrWhiteSpace(district))
                                                        {
                                                            piecesList.Add(district);
                                                        }

                                                        piecesList.Add(addrStreet.OffName);

                                                        piecesList.Add(addrPlace.ToString());

                                                        if (!string.IsNullOrWhiteSpace(metro.Name))
                                                        {
                                                            piecesList.Add($"м. {metro.Name}");
                                                        }

                                                        return string.Join(", ", piecesList);
                                                    }
                                                }
                                                else
                                                {
                                                    logger.Info($"NULLLOTNUM! {nd.Data.BidNumber} ### {lot?.Location} ### {lot?.Description}, {++houseamount}");
                                                }
                                            }
                                            else
                                            {
                                                logger.Info($"NULLCENTROID! {addrHouse.Id} ### {addrStreet.OffName} ### {lot?.Location} ### {lot?.Description}, {++houseamount}");
                                            }
                                        }
                                        else
                                        {
                                            logger.Info($"NULLHOUSE! {lot?.Location} ### {lot?.Description}, {++houseamount}");
                                        }
                                    }
                                    else
                                    {
                                        logger.Info($"NULLSTREET/PLACE! {lot?.Location} ### {lot?.Description}, {++strplaceamount}");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error("While update rent adverts data", e);
                            }
                        }
                    }
                }
            }
            else
            {
                logger.Error($"File {advertsListFile} doesn't exists.");
            }

            logger.Info($"End of updating rent adverts data.");

            logger.Info($"Archiving out of date adverts...");

            var adverts = await adRentLotCollection.Find(Builders<AdRentLotModel>.Filter.Empty).ToListAsync();

            foreach(var adv in adverts)
            {
                if(IsAdvertOutOfDate(adv.AuctionDate))
                {
                    await adRentLotArchCollection.InsertOneAsync(adv);

                    await adRentLotCollection.DeleteOneAsync(_ => _.Id == adv.Id);

                    logger.Info($"{adv.NotificationId}#{adv.LotNum} is post-archived.");
                }
            }

            logger.Info($"End of archiving out of date adverts.");
        }

        private bool IsAdvertOutOfDate(DateTime auctionDate)
        {
            return DateTime.Now > auctionDate && DateTime.Now.Subtract(auctionDate).Days > 3;
        }

        private async Task<EnvironmentInfo> GetEnvironmentInfo(AddrHouseModel addrHouse)
        {
            var addrHouseCollection = advertsDatabase.GetCollection<AddrHouseModel>(addrHouseCollectionName);

            var filterHouses1km = Builders<AddrHouseModel>.Filter.NearSphere(_ => _.Centroid, addrHouse.Centroid, maxDistance: 1000);
            var filterHouses500m = Builders<AddrHouseModel>.Filter.NearSphere(_ => _.Centroid, addrHouse.Centroid, maxDistance: 500);

            var houses1km = await addrHouseCollection.Find(filterHouses1km).ToListAsync();
            var houses500m = await addrHouseCollection.Find(filterHouses500m).ToListAsync();

            var ApartmentsAround1Km = GetLivingApartments(houses1km);
            var ApartmentsAround500m = GetLivingApartments(houses500m);

            var stStationsCollection = advertsDatabase.GetCollection<STStationsModel>(stStationsCollectionName);

            var filterStStations1km = Builders<STStationsModel>.Filter.NearSphere(_ => _.GeoPoint, addrHouse.Centroid, maxDistance: 1000);
            var filterStStations500m = Builders<STStationsModel>.Filter.NearSphere(_ => _.GeoPoint, addrHouse.Centroid, maxDistance: 500);

            var stStations1km = await stStationsCollection.Find(filterStStations1km).ToListAsync();
            var stStations500m = await stStationsCollection.Find(filterStStations500m).ToListAsync();

            var AmountOfStRoutesAround1Km = GetAmountOfStRoutes(stStations1km);
            var AmountOfStRoutesAround500m = GetAmountOfStRoutes(stStations500m);

            var metroEntranceCollection = advertsDatabase.GetCollection<MetroEntranceModel>(metroEntrancesCollectionName);
            var filtermetroEntrances1km = Builders<MetroEntranceModel>.Filter.NearSphere(_ => _.GeoPoint, addrHouse.Centroid, maxDistance: 1000);

            var metroEntrances1km = await metroEntranceCollection.Find(filtermetroEntrances1km).ToListAsync();

            var metroStationsAround1km = new HashSet<string>();

            metroEntrances1km.ForEach(_ => metroStationsAround1km.Add(_.NameOfStation));

            var parkingAutomatesCollection = advertsDatabase.GetCollection<ParkingAutomatesModel>(parkingAutomatesCollectionName);
            var filterParkingAutomates500m = Builders<ParkingAutomatesModel>.Filter.NearSphere(_ => _.GeoPoint, addrHouse.Centroid, maxDistance: 500);
            var parkingAutomates500m = await parkingAutomatesCollection.Find(filterParkingAutomates500m).ToListAsync();

            var metroParkingLotsCollection = advertsDatabase.GetCollection<MetroParkingLotsModel>(metroParkingLotsCollectionName);
            var filterMetroParkingLots2km = Builders<MetroParkingLotsModel>.Filter.NearSphere(_ => _.GeoPoint, addrHouse.Centroid, maxDistance: 2000);
            var metroParkingLots2km = await metroParkingLotsCollection.Find(filterMetroParkingLots2km).ToListAsync();

            var mfcOfficesCollection = advertsDatabase.GetCollection<MFCOfficeModel>(mfcOfficesCollectionName);
            var filterMFCOffices2km = Builders<MFCOfficeModel>.Filter.NearSphere(_ => _.GeoPoint, addrHouse.Centroid, maxDistance: 2000);
            var mfcOffices2km = await mfcOfficesCollection.Find(filterMFCOffices2km).ToListAsync();

            var postOfficesCollection = advertsDatabase.GetCollection<PostOfficesModel>(postOfficesCollectionName);
            var filterPostOffices2km = Builders<PostOfficesModel>.Filter.NearSphere(_ => _.GeoPoint, addrHouse.Centroid, maxDistance: 2000);
            var postOffices2km = await postOfficesCollection.Find(filterPostOffices2km).ToListAsync();

            return new EnvironmentInfo
            {
                AmountOfLivingHousesAround1Km = houses1km.Count,
                AmountOfLivingHousesAround500m = houses500m.Count,
                AmountOfLivingApartmentsAround1Km = ApartmentsAround1Km,
                AmountOfLivingApartmentsAround500m = ApartmentsAround500m,
                AmountOfStStationsAround1Km = stStations1km.Count,
                AmountOfStStationsAround500m = stStations500m.Count,
                AmountOfStRoutesAround1Km = AmountOfStRoutesAround1Km,
                AmountOfStRoutesAround500m = AmountOfStRoutesAround500m,
                AmountOfMetroStationAround1km = metroStationsAround1km.Count,
                AmountOfParkingAutomatesAround500m = parkingAutomates500m.Count,
                AmountOfMetroParkingLotsAround2km = metroParkingLots2km.Count,
                AmountOfMFCAround2km = mfcOffices2km.Count,
                AmountOfPostOfficesAround2km = postOffices2km.Count
            };

            int GetAmountOfStRoutes(List<STStationsModel> stations)
            {
                var routes = new HashSet<string>();

                foreach (var s in stations)
                {
                    if(!string.IsNullOrWhiteSpace(s.RouteNumbers))
                    {
                        foreach(var route in s.RouteNumbers.SplitNoEmptyTrim(new char[] { ' ' }))
                        {
                            routes.Add(route.RemoveFromEnd(";"));
                        }
                    }
                }

                return routes.Count;
            }

            int GetLivingApartments(List<AddrHouseModel> houses)
            {
                int amount = 0;

                foreach(var h in houses)
                {
                    if(h.Details?.Count > 0)
                    {
                        foreach(var details in h.Details)
                        {
                            if (details.Extra != null)
                            {
                                details.Extra.TryGetValue("Количество этажей", out var extra);

                                var entrancesCountStr = extra.Where(_ => _.FirstOrDefault() == "Количество жилых помещений, шт.").FirstOrDefault()?.LastOrDefault();

                                if (int.TryParse(entrancesCountStr, out int ha))
                                {
                                    amount += ha;
                                }
                            }
                        }
                    }
                }

                return amount;
            }
        }

        private HouseInfo GetHouseInfo(AddrHouseModel addrHouse)
        {
            if(addrHouse.Details != null)
            {
                var details = addrHouse.Details?.FirstOrDefault();

                if (details != null)
                {
                    var houseInfo = new HouseInfo();

                    if (details.Main != null)
                    {
                        details.Main.TryGetValue("Серия", out houseInfo.ProjectName);

                        if (details.Main.TryGetValue("Этажность", out string floorsStr))
                        {
                            int.TryParse(floorsStr, out houseInfo.Floors);
                        }

                        if (details.Main.TryGetValue("Год постройки", out string yearOfBuildStr))
                        {
                            int.TryParse(yearOfBuildStr, out houseInfo.YearOfBuild);
                        }
                    }

                    if(details.Extra != null)
                    {
                        details.Extra.TryGetValue("Количество этажей", out var extra);

                        var entrancesCountStr = extra.Where(_ => _.FirstOrDefault() == "Количество подъездов, шт.").FirstOrDefault()?.LastOrDefault();

                        int.TryParse(entrancesCountStr, out houseInfo.EntrancesCount);
                    }

                    if (details.Stewardship != null)
                    {
                        details.Stewardship.TryGetValue("Управляющая организация", out var stewardship);

                        houseInfo.NameOfAuthority = stewardship.FirstOrDefault();
                    }

                    if(!houseInfo.IsNull())
                    {
                        return houseInfo;
                    }
                }
            }

            return null;
        }

        private List<AdRentLotFile> GetAdLotFiles(string odDetailedHref, Lot lot, string advertsDataFilesDir)
        {
            var files = new List<AdRentLotFile>();
            var nId = AdvHelpers.GetDirectoryId(odDetailedHref);

            var nFilesDir = $"{advertsDataFilesDir}{Path.DirectorySeparatorChar}{nId.DirectoryName}{Path.DirectorySeparatorChar}{nId.Id}";
            var lotFilesDir = $"{advertsDataFilesDir}{Path.DirectorySeparatorChar}{nId.DirectoryName}{Path.DirectorySeparatorChar}{nId.Id}{Path.DirectorySeparatorChar}{lot.LotNum}";

            if (Directory.Exists(nFilesDir))
            {
                foreach (var f in Directory.EnumerateFiles(nFilesDir).Where(_ => Path.GetExtension(_) != ".flg").Select(Path.GetFileName))
                {
                    var name = string.Join("-", f.Split(new char[] { '-' }).Skip(1));

                    files.Add(new AdRentLotFile
                    {
                        Filename = f,
                        Name = name.ToUpperFirstLetter(),
                        LotFile = false
                    });
                }
            }

            if (Directory.Exists(lotFilesDir))
            {
                foreach (var f in Directory.EnumerateFiles(lotFilesDir).Where(_ => Path.GetExtension(_) != ".flg").Select(Path.GetFileName))
                {
                    var name = string.Join("-", f.Split(new char[] { '-' }).Skip(1));

                    files.Add(new AdRentLotFile
                    {
                        Filename = f,
                        Name = name.ToUpperFirstLetter(),
                        LotFile = true
                    });
                }
            }

            files = files.OrderBy(_ => _.Name).ToList();

            AdRentLotFile prev = null;
            int count = 0;

            foreach (var f in files)
            {
                if (prev != null)
                {
                    if (f.Name == prev.Name)
                    {
                        f.Name = $"{Path.GetFileNameWithoutExtension(f.Name)}{++count}{Path.GetExtension(f.Name)}";
                    }
                    else
                    {
                        prev = f;
                        count = 0;
                    }
                }
                else
                {
                    prev = f;
                }
            }

            return files;
        }

        private async Task<(string Name, string Line, string MetroEntrance, double DistanceTowardsMetroEntrance)> GetAddrHouseMetro(AddrHouseModel addrHouse)
        {
            var geoMetroCollection = advertsDatabase.GetCollection<MetroEntranceModel>(metroEntrancesCollectionName);

            var filterMetro = Builders<MetroEntranceModel>.Filter.NearSphere(_ => _.GeoPoint, addrHouse.Centroid);

            var metro = await geoMetroCollection.Find(filterMetro).Limit(1).FirstOrDefaultAsync();

            if (metro != null)
            {
                var metroEntrance = string.Join(", ", metro.Name.SplitNoEmptyTrim(new char[] { ',' }).Skip(1));
                return (metro.NameOfStation, metro.Line, metroEntrance, GeoHelpers.CalcDistance(addrHouse.Centroid, metro.GeoPoint));
            }
            else
            {
                return (string.Empty, string.Empty, string.Empty, 0);
            }
        }

        private async Task<(string StationName, string StationRoutes, string StationDirection, double DistanceTowardsStation)> GetAddrHouseSTStation(AddrHouseModel addrHouse)
        {
            var geoStationsCollection = advertsDatabase.GetCollection<STStationsModel>(stStationsCollectionName);

            var filterStations = Builders<STStationsModel>.Filter.NearSphere(_ => _.GeoPoint, addrHouse.Centroid);

            var station = await geoStationsCollection.Find(filterStations).Limit(1).FirstOrDefaultAsync();

            if (station != null)
            {
                return (station.StationName, station.RouteNumbers, station.Direction, GeoHelpers.CalcDistance(addrHouse.Centroid, station.GeoPoint));
            }
            else
            {
                return (string.Empty, string.Empty, string.Empty, 0);
            }
        }

        private async Task<List<string>> GetAddrHouseRings(AddrHouseModel addrHouse)
        {
            var geoRingsCollection = advertsDatabase.GetCollection<GeoRingModel>(geoRingsCollectionName);

            var filterRings = Builders<GeoRingModel>.Filter.GeoIntersects(_ => _.Polygon, addrHouse.Centroid);

            var resRings = await geoRingsCollection.FindSync(filterRings).ToListAsync();

            return resRings.Select(_ => _.Name).ToList();
        }

        private async Task<string> GetAddrHouseDistrict(AddrHouseModel addrHouse)
        {
            var geoMoCollection = advertsDatabase.GetCollection<GeoMoModel>(geoMoCollectionName);

            var filterMo = Builders<GeoMoModel>.Filter.GeoIntersects(_ => _.Polygon, addrHouse.Centroid);

            var resMo = await geoMoCollection.FindSync(filterMo).FirstOrDefaultAsync();

            if (resMo != null)
            {
                return resMo.Name;
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task<(string RegionAbbr, string RegionName)> GetAddrHouseRegion(AddrHouseModel addrHouse)
        {
            var geoAoCollection = advertsDatabase.GetCollection<GeoAoModel>(geoAoCollectionName);

            var filterAo = Builders<GeoAoModel>.Filter.GeoIntersects(_ => _.Polygon, addrHouse.Centroid);

            var resAo = await geoAoCollection.FindSync(filterAo).FirstOrDefaultAsync();

            if (resAo != null)
            {
                return (resAo.Abbr, resAo.Name);
            }
            else
            {
                return (string.Empty, string.Empty);
            }
        }

        private async Task<AddrStreetModel> GetNotificationStreet(Lot lot)
        {
            var kladr = lot?.KladrLocation?.Id;
            var addrStreet = null as AddrStreetModel;

            if (!string.IsNullOrWhiteSpace(kladr))
            {
                var collection = advertsDatabase.GetCollection<AddrStreetModel>(addrStreetCollectionName);
                addrStreet = await collection.Find(_ => _.Code == kladr).FirstOrDefaultAsync();
            }

            if (addrStreet == null)
            {
                addrStreet = await ExtractStreetAddr(lot?.Location, lot?.Description);
            }

            return addrStreet;

            async Task<AddrStreetModel> ExtractStreetAddr(string str1, string str2)
            {
                AddrStreetModel r1 = await ExtractStreetAddrTry(str1);

                return r1 == null ? await ExtractStreetAddrTry(str2) : r1;
            }

            async Task<AddrStreetModel> ExtractStreetAddrTry(string str)
            {
                AddrStreetModel street = null;

                if (string.IsNullOrWhiteSpace(str))
                {
                    return street;
                }

                str = str.Remove(0, str.TakeWhile(c => !char.IsLetterOrDigit(c)).Count());

                var ss =
                    Regex.Replace(str, @"(г|Г|гор|г(о|o)р(о|o)д)?\.?([\s]+)?(М|м)(о|o)скв(а|a)", ",")
                    .Replace(".д.", ". д.")
                    .SplitNoEmptyTrim(new char[] { ',' });
                var _name = ExtractStretName(ss);
                var name = CleanStreet(_name.TrimAndCompactWhitespaces());
                var st = DetectStreetType(name);

                if (!string.IsNullOrWhiteSpace(st.StreetType) && !string.IsNullOrWhiteSpace(st.StreetName))
                {
                    var streetName =
                        FixStreetName(CorrStreetName(st.StreetName));
                    var streetNameNorm = streetName
                        .Replace('ё', 'е')
                        .Replace('c', 'с')
                        .ToLowerInvariant();

                    var collection = advertsDatabase.GetCollection<AddrStreetModel>(addrStreetCollectionName);
                    street = await collection.Find(_ => _.FormalNameNorm == streetNameNorm
                        && _.ShortName == st.StreetType).FirstOrDefaultAsync();
                }

                return street;
            }

            string FixStreetName(string streetName)
            {
                switch (streetName)
                {
                    case "Героев-Панфиловцев":
                        return "Героев Панфиловцев";
                    case "Дм. Ульянова":
                    case "Дм.Ульянова":
                        return "Дмитрия Ульянова";
                    case "Мясницкая.":
                        return "Мясницкая";
                    case "Тимирязевская.":
                        return "Тимирязевская";
                    case "Ак.Пилюгина":
                    case "Ак. Пилюгина":
                        return "Академика Пилюгина";
                    case "Ак. Королева":
                        return "Академика Королева";
                }

                return streetName;
            }

            string CorrStreetName(string streetName)
            {
                var tries = new Func<string, string>[] { Try1, Try2, Try3, Try4, Try5 };

                var r = tries.Select(f => f(streetName)).Where(s => !string.IsNullOrWhiteSpace(s)).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(r))
                {
                    return streetName;
                }

                return r;

                string Try5(string s)
                {
                    var ws = s.SplitNoEmptyTrim(new char[] { ' ' });

                    if (ws.Count() > 1)
                    {
                        var fp = ws.First();
                        var ifs = fp.StartsWith("Средняя") || fp.StartsWith("Средний") || fp.StartsWith("Ср.");

                        if (ifs)
                        {
                            return string.Join(" ", ws.Skip(1).Append("Ср."));
                        }
                    }
                    else
                    {
                        if (s.StartsWith("Ср."))
                        {
                            return $"{s.RemoveFromStart("Ср.")} Ср.";
                        }
                    }

                    return string.Empty;
                }

                string Try4(string s)
                {
                    var ws = s.SplitNoEmptyTrim(new char[] { ' ' });

                    if (ws.Count() > 1)
                    {
                        var fp = ws.First();
                        var ifn = fp.StartsWith("Новая") || fp.StartsWith("Новый");
                        var ifs = fp.StartsWith("Старая") || fp.StartsWith("Старый");

                        if (ifn || ifs)
                        {
                            return string.Join(" ", ws.Skip(1).Append(ifn ? "Нов." : "Стар."));
                        }
                    }

                    return string.Empty;
                }

                string Try3(string s)
                {
                    var ws = s.SplitNoEmptyTrim(new char[] { ' ' });

                    if (ws.Count() > 1)
                    {
                        var fp = ws.First();
                        var ifm = fp.StartsWith("Малый") || fp.StartsWith("Малая") || fp.StartsWith("Мал.");
                        var ifb = fp.StartsWith("Большой") || fp.StartsWith("Большая") || fp.StartsWith("Б.");

                        if (ifm || ifb)
                        {
                            return string.Join(" ", ws.Skip(1).Append(ifm ? "М." : "Б."));
                        }
                    }
                    else
                    {
                        if (s.StartsWith("Б."))
                        {
                            return $"{s.RemoveFromStart("Б.")} Б.";
                        }
                        else if (s.StartsWith("Мал."))
                        {
                            return $"{s.RemoveFromStart("Мал.")} М.";
                        }

                    }

                    return string.Empty;
                }

                string Try2(string s)
                {
                    var ws = s.SplitNoEmptyTrim(new char[] { ' ' });

                    if (ws.Count() > 1)
                    {
                        var fp = ws.First();
                        var ifv = fp.StartsWith("Верхняя") || fp.StartsWith("Верхний");
                        var ifn = fp.StartsWith("Нижняя") || fp.StartsWith("Нижний") || s.StartsWith("Н.");

                        if (ifv || ifn)
                        {
                            return string.Join(" ", ws.Skip(1).Append(ifv ? "Верхн." : "Нижн."));
                        }
                    }
                    else
                    {
                        if (s.StartsWith("Н."))
                        {
                            return $"{s.RemoveFromStart("Н.")} Н.";
                        }
                    }

                    return string.Empty;
                }

                string Try1(string s)
                {
                    var ws = s.SplitNoEmptyTrim(new char[] { ' ' });

                    if (ws.Count() > 1)
                    {
                        var fp = ws.First();

                        if (fp.EndsWith("-й") || fp.EndsWith("-я"))
                        {
                            return string.Join(" ", ws.Skip(1).Append(fp));
                        }
                    }

                    return string.Empty;
                }
            }

            (string StreetType, string StreetName) DetectStreetType(string str)
            {
                var xmlStreetTypes = new string[] { "ул.", "улица", "проезд", "пр.", "пр-д", "проспект",
                    "просп.", "пр-т", "пер.", "бульвар", "переулок", "ш.", "шоссе", "пл.", "площадь",
                    "набережная", "наб.", "проспект.", "Проспект", "г.", "линия", "г ", " г", " пер",
                    "аллея", "б-р", "пр-т.", "ул ", " ул", "пр-кт", "Шоссе", "Ул." };

                var rigidStreet = RigidStreetNameAndType();

                if (!string.IsNullOrEmpty(rigidStreet.StreetName) && !string.IsNullOrEmpty(rigidStreet.StreetType))
                {
                    return rigidStreet;
                }

                var psType = xmlStreetTypes.Where(t => str.StartsWith(t) || str.EndsWith(t)).FirstOrDefault();

                var dbt = XmlToDBStreetType(psType);

                if (string.IsNullOrWhiteSpace(psType) || string.IsNullOrWhiteSpace(dbt))
                {
                    return (string.Empty, string.Empty);
                }

                string _streeNm =
                    str
                    .RemoveAround(psType)
                    .TrimAndCompactWhitespaces();

                return (dbt, _streeNm);

                string XmlToDBStreetType(string s)
                {
                    switch (s)
                    {
                        case "аллея":
                            return "аллея";

                        case "набережная":
                        case "наб.":
                            return "наб";

                        case "ул.":
                        case "улица":
                        case "ул ":
                        case " ул":
                        case "Ул.":
                            return "ул";

                        case "проезд":
                        case "пр.":
                        case "пр-д":
                            return "проезд";

                        case "проспект":
                        case "просп.":
                        case "пр-т":
                        case "проспект.":
                        case "Проспект":
                        case "пр-т.":
                        case "пр-кт":
                            return "пр-кт";

                        case "пер.":
                        case "переулок":
                        case " пер":
                            return "пер";

                        case "бульвар":
                        case "б-р":
                            return "б-р";

                        case "ш.":
                        case "шоссе":
                        case "Шоссе":
                            return "ш";

                        case "пл.":
                        case "площадь":
                            return "пл";

                        case "г.":
                        case " г":
                        case "г ":
                            return "г";

                        case "линия":
                            return "линия";

                        default:
                            return string.Empty;
                    }
                }

                (string StreetType, string StreetName) RigidStreetNameAndType()
                {
                    switch (str.ToLowerInvariant())
                    {
                        case "3-я ул. соколиной горы":
                            return ("ул", "Соколиной Горы 3-я");
                        case "5-я ул. соколиной горы":
                            return ("ул", "Соколиной Горы 5-я");
                        case "8-я ул. соколиной горы":
                            return ("ул", "Соколиной Горы 8-я");
                        case "9-я ул. соколиной горы":
                            return ("ул", "Соколиной Горы 9-я");
                        case "10-я ул. соколиной горы":
                            return ("ул", "Соколиной Горы 10-я");
                        case "зеленоград":
                            return ("г", "Зеленоград");
                        case "старые кузьминки":
                            return ("ул", "Старые Кузьминки");
                        case "олений вал":
                            return ("ул", "Олений Вал");
                        case "1-й лучевой пр.":
                            return ("просек", "Лучевой 1-й");
                        case "2-й лучевой пр.":
                            return ("просек", "Лучевой 2-й");
                        case "3-й лучевой пр.":
                            return ("просек", "Лучевой 3-й");
                        case "4-й лучевой пр.":
                            return ("просек", "Лучевой 4-й");
                        case "5-й лучевой пр.":
                            return ("просек", "Лучевой 5-й");
                        case "6-й лучевой пр.":
                            return ("просек", "Лучевой 6-й");
                        case "серпуховский вал":
                            return ("ул", "Серпуховский вал");
                        case "ленинские горы":
                            return ("ул", "Ленинские Горы");
                        case "б. черемушкинская":
                            return ("ул", "Черёмушкинская Б.");
                        case "крымский вал":
                            return ("ул", "Крымский Вал");
                    }

                    return (string.Empty, string.Empty);
                }
            }

            string CleanStreet(string str)
            {
                var street = new List<string>();
                var chks = new Func<string, bool>[] { Chk1, Chk2, Chk3, Chk4, Chk5, Chk6 };

                foreach (var s in str.SplitNoEmptyTrim(new char[] { ' ' }))
                {
                    if (chks.Any(f => f(s)))
                    {
                        break;
                    }
                    else
                    {
                        street.Add(s);
                    }
                }

                return string.Join(" ", street);

                bool Chk1(string s)
                {
                    return s.StartsWith("д.");
                }

                bool Chk2(string s)
                {
                    return s == "д";
                }

                bool Chk3(string s)
                {
                    return s == "дом";
                }

                bool Chk4(string s)
                {
                    return s == ".д";
                }

                bool Chk5(string s)
                {
                    return s.All(char.IsDigit);
                }

                bool Chk6(string s)
                {
                    return s.StartsWith("дом.");
                }
            }

            string ExtractStretName(IEnumerable<string> ss)
            {
                var disctricts = new string[] { "цао", "сао", "свао", "вао", "ювао", "юао", "юзао", "зао", "сзао" };
                var chks = new Func<string, bool>[] { Chk1, Chk2, Chk3, Chk4, Chk5, Chk6, Chk7 };

                foreach (var s in ss)
                {
                    var __s = s.TrimAndCompactWhitespaces().ToLowerInvariant();

                    if (!chks.Any(f => f(__s)))
                    {
                        return s;
                    }
                }

                return string.Empty;

                bool Chk1(string s)
                {
                    return s.Contains("москва");
                }

                bool Chk2(string s)
                {
                    return s.Contains("россия");
                }

                bool Chk3(string s)
                {
                    return s.All(c => char.IsDigit(c));
                }

                bool Chk4(string s)
                {
                    return s.StartsWith("р-н");
                }

                bool Chk5(string s)
                {
                    return s == "м" || s == "м.";
                }

                bool Chk6(string s)
                {
                    return disctricts.Any(d => d == s);
                }

                bool Chk7(string s)
                {
                    return s == "г." || s == "г";
                }
            }
        }

        private AddrPlace GetNotificationLotPlace(Lot lot)
        {
            var location = lot?.Location;
            var description = lot?.Description;

            var addrPlace = ParseNotificationLotAddr(location);

            if (addrPlace.IsEmpty())
            {
                addrPlace = ParseNotificationLotAddr(description);
            }

            return addrPlace;

            AddrPlace ParseNotificationLotAddr(string addr)
            {
                if (string.IsNullOrWhiteSpace(addr))
                {
                    return new AddrPlace(string.Empty, string.Empty, string.Empty);
                }

                addr = addr.Map(ReplaceChar);
                addr = addr.ToLower();
                addr = addr.HideCRLF();
                addr = addr.TrimAndCompactWhitespaces();

                return new AddrPlace(ParseHouse(addr), ParseBuild(addr), ParseStruc(addr));

                string ParseStruc(string str)
                {
                    string[] houseFeatures = new string[] { "стр.", "строен.", "соор.", "строение", "стр" };

                    var (fVal, fIndex) = str.IndexOfAnyWord(houseFeatures);

                    if (fIndex < 0)
                    {
                        return string.Empty;
                    }

                    var rest = str.Substring(fIndex + fVal.Length).TrimAndCompactWhitespaces();

                    var num1 = new string(rest.TakeWhile(char.IsDigit).ToArray());

                    if (string.IsNullOrEmpty(num1))
                    {
                        return string.Empty;
                    }

                    rest = rest.Substring(num1.Length);

                    var num2 = new string(rest.TakeWhile(c => char.IsLetterOrDigit(c) || c == '/' || c == '-').ToArray());

                    return (num1 + num2).ToUpperInvariant();
                }

                string ParseBuild(string str)
                {
                    string[] houseFeatures = new string[] { "корп.", "кор." };

                    var (fVal, fIndex) = str.IndexOfAnyWord(houseFeatures);

                    if (fIndex < 0)
                    {
                        return string.Empty;
                    }

                    var rest = str.Substring(fIndex + fVal.Length).TrimAndCompactWhitespaces();

                    var num = new string(rest.TakeWhile(c => char.IsLetterOrDigit(c) || c == '/' || c == '-').ToArray());

                    return num.ToUpperInvariant();
                }

                string ParseHouse(string str)
                {
                    string[] houseFeatures = new string[] { "д.", "домовлад.", "домол.", "домовл.",
                        "вл.", "дом", "домовладение", "домовладени", "владение", "д" };

                    var (fVal, fIndex) = str.IndexOfAnyWord(houseFeatures);

                    if (fIndex < 0)
                    {
                        return string.Empty;
                    }

                    var rest = str.Substring(fIndex + fVal.Length).TrimAndCompactWhitespaces();

                    var num1 = new string(rest.TakeWhile(char.IsDigit).ToArray());

                    if (string.IsNullOrEmpty(num1))
                    {
                        return string.Empty;
                    }

                    rest = rest.Substring(num1.Length);

                    var num2 = new string(rest.TakeWhile(c => char.IsLetterOrDigit(c) || c == '/' || c == '-').ToArray());

                    rest = rest.Substring(num2.Length).TrimAndCompactWhitespaces();

                    var c1 = rest.Take(1).FirstOrDefault();
                    var c2 = rest.Skip(1).Take(1).FirstOrDefault();

                    var res = num1 + num2;

                    if (char.IsLetter(c1) && !char.IsLetter(c2))
                    {
                        res += c1;
                    }

                    return res.ToUpperInvariant();
                }

                char ReplaceChar(char ch)
                {
                    switch (ch)
                    {
                        case 'A':
                            return 'А';
                        case 'O':
                            return 'О';
                        case 'E':
                            return 'Е';
                        default:
                            return ch;
                    }
                }
            }
        }

        private List<AdvMission> GetNotificationLotMissions(Lot lot)
        {
            if (!string.IsNullOrWhiteSpace(lot.Mission))
            {
                return AdvHelpers.GetMissions(lot.Mission);
            }
            else
            {
                return new List<AdvMission>();
            }
        }

        private async Task DownloadAdvertsFiles(string advertsListFile, string advertsDataDestDir, string advertsDataFilesDir)
        {
            logger.Info($"Processing {advertsListFile}");

            if (File.Exists(advertsListFile))
            {
                if (!Directory.Exists(advertsDataFilesDir))
                {
                    Directory.CreateDirectory(advertsDataFilesDir);
                }

                foreach (var n in XmlUtilites.ReadXmlStream<NotificationInfo>(advertsListFile, "notification"))
                {
                    if (!string.IsNullOrWhiteSpace(n.OdDetailedHref))
                    {
                        var nId = AdvHelpers.GetDirectoryId(n.OdDetailedHref);
                        var nDir = $"{advertsDataDestDir}{Path.DirectorySeparatorChar}{nId.DirectoryName}";
                        var nFile = $"{nDir}{Path.DirectorySeparatorChar}{nId.Id}";
                        var nFilesDir = $"{advertsDataFilesDir}{Path.DirectorySeparatorChar}{nId.DirectoryName}{Path.DirectorySeparatorChar}{nId.Id}";
                        var lotFilesDir = $"{advertsDataFilesDir}{Path.DirectorySeparatorChar}{nId.DirectoryName}{Path.DirectorySeparatorChar}{nId.Id}{Path.DirectorySeparatorChar}";

                        if (File.Exists(nFile))
                        {
                            if (!Directory.Exists(nFilesDir))
                            {
                                Directory.CreateDirectory(nFilesDir);
                            }

                            try
                            {
                                var prepared = File.ReadAllText(nFile)
                                    .Replace("&#11;", "")
                                    .Replace("&#13;", "")
                                    .Replace("&#31;", "")
                                    .Replace("&#19;", "");

                                var nd = XmlUtilites.DeserializeXmlString<Notification>(prepared);

                                if (!nd.CheckNotificationFresh() || nd?.Data?.Lots?.
                                        Any(lot =>
                                            lot?.KladrLocation?.Id?.StartsWith("77") == true &&
                                            lot?.PropertyType?.Name?.StartsWith("Недвижимое имущество;") == true
                                        ) == false
                                    )
                                {
                                    continue;
                                }

                                logger.Info($"Downloading from {nFile}");

                                foreach (var doc in nd?.Data?.Documents)
                                {
                                    await DownloadDocument(doc, nFilesDir);

                                    logger.Info($"{doc?.DocUrl} to {nFilesDir}");
                                }

                                foreach (var lot in nd?.Data?.Lots)
                                {
                                    if (lot?.KladrLocation?.Id?.StartsWith("77") == true &&
                                        lot?.PropertyType?.Name?.StartsWith("Недвижимое имущество;") == true)
                                    {
                                        var lotDest = lotFilesDir + lot?.LotNum;

                                        if (!Directory.Exists(lotDest))
                                        {
                                            Directory.CreateDirectory(lotDest);
                                        }

                                        foreach (var doc in lot?.Documents)
                                        {
                                            await DownloadDocument(doc, lotDest);

                                            logger.Info($"{doc?.DocUrl} to {lotFilesDir}{lot?.LotNum}");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error($"Error while downloading files from {nFile}", e);
                            }
                        }
                    }
                }
            }
            else
            {
                logger.Error($"File {advertsListFile} doesn't exists.");
            }

            logger.Info($"End of downloading advert files.");
        }

        private async Task<(string Filename, string ContentType, bool OK)> DownloadDocument(Document doc, string destDir)
        {
            string Filename = string.Empty, ContentType = string.Empty;

            bool OK = false;

            if (!string.IsNullOrWhiteSpace(doc?.DocUrl))
            {
                doc.DocUrl = doc.DocUrl.Replace("<![CDATA[", "").Replace("]]>", "");

                if (Uri.TryCreate(doc.DocUrl, UriKind.Absolute, out Uri uri))
                {
                    var vals = QueryHelpers.ParseQuery(uri.Query);

                    if (vals.ContainsKey("id") && vals.TryGetValue("id", out StringValues vs))
                    {
                        var idVal = vs.FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(idVal))
                        {
                            var dwnFlag = $"{destDir}{Path.DirectorySeparatorChar}{idVal}.flg";

                            if (!File.Exists(dwnFlag))
                            {
                                var DownloadUrl = GetDownloadUrl(vals, doc.DocUrl);

                                try
                                {
                                    var rd = await HttpUtilites.DownloadFileToMem($"{DownloadUrl}&id={idVal}");

                                    if (string.IsNullOrWhiteSpace(rd.Filename))
                                    {
                                        try
                                        {
                                            var content = Encoding.UTF8.GetString(await HttpUtilites.GetBytesByUrl(doc.DocUrl, allowAutoRedirect: true));
                                            var lookingFor = "filename=";
                                            int idx = content.IndexOf(lookingFor, StringComparison.CurrentCultureIgnoreCase);
                                            if (idx >= 0)
                                            {
                                                var nm = new string(content.Substring(idx + lookingFor.Length)
                                                    .TakeWhile(c => c != '&' && c != '"').ToArray());
                                                rd.Filename = WebUtility.UrlDecode(nm);
                                            }
                                        }
                                        finally
                                        {
                                            if (string.IsNullOrWhiteSpace(rd.Filename))
                                            {
                                                rd.Filename = doc.Description;
                                            }
                                        }
                                    }

                                    ContentType = rd.ContentType;

                                    Filename = string.IsNullOrWhiteSpace(rd.Filename) ? doc.Description : rd.Filename;

                                    if (string.IsNullOrWhiteSpace(Filename))
                                    {
                                        Filename = RetrieveNameTry1(vals);
                                    }

                                    if (!string.IsNullOrWhiteSpace(Filename))
                                    {
                                        if (!Filename.Contains('.') && !string.IsNullOrWhiteSpace(rd.ContentType))
                                        {
                                            Filename = $"{Filename}.{MimeTypesMap.GetExtension(rd.ContentType)}";
                                        }

                                        if (rd.Data?.Count() > 0 && rd.Status == HttpStatusCode.OK)
                                        {
                                            var filePath = $"{destDir}{Path.DirectorySeparatorChar}{idVal}-{Filename}";

                                            if (!File.Exists(filePath))
                                            {
                                                File.WriteAllBytes(filePath, rd.Data);
                                            }

                                            if (File.Exists(filePath))
                                            {
                                                File.WriteAllText(dwnFlag, string.IsNullOrWhiteSpace(doc.DocDate) ?
                                                    DateTime.Now.ToString("yyyy-MM-dd") : doc.DocDate);
                                            }

                                            OK = true;
                                        }
                                    }

                                    logger.Info($"{Filename} | {rd.Data.Count()} | {rd.Status} | {rd.ContentType}");
                                }
                                catch (HttpRequestException)
                                {
                                    logger.Error($"Error download: {DownloadUrl}&id={idVal}");
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Error($"Error uri format: {doc.DocUrl}");
                    }
                }
            }

            return (Filename, ContentType, OK);

            string GetDownloadUrl(Dictionary<string, StringValues> vals, string url)
            {
                if (GetQueryVal("class", out string typeVal) ||
                    GetQueryVal("section", out typeVal))
                {
                    switch (typeVal.ToLowerInvariant())
                    {
                        case "lotphoto":
                            return DownloadLotPhotoUrl;
                        case "rosimact":
                            return DownloadRosimActUrl;
                        case "fasdata":
                            return DownloadFasDataActUrl;
                        case "consult":
                            return DownloadConsultDocumentsUrl;
                    }
                }

                return DownloadDocumentUrl;

                bool GetQueryVal(string name, out string value)
                {
                    value = string.Empty;

                    if (vals.ContainsKey(name) && vals.TryGetValue(name, out StringValues vls))
                    {
                        var classVal = vls.FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(classVal))
                        {
                            value = classVal;

                            return true;
                        }
                    }

                    return false;
                }
            }

            string RetrieveNameTry1(Dictionary<string, StringValues> vals)
            {
                if (vals.ContainsKey("class") && vals.TryGetValue("class", out StringValues vs))
                {
                    var classVal = vs.FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(classVal))
                    {
                        switch (classVal.ToLowerInvariant())
                        {
                            case "lotphoto":
                                return "Фото";
                        }
                    }
                }

                return string.Empty;
            }
        }

        private async Task DownloadAdvertsData(string advertsListFile, string advertsDataDestDir)
        {
            logger.Info($"Processing {advertsListFile}");

            if (File.Exists(advertsListFile))
            {
                int amount = 0;

                var adRentLotCollection = advertsDatabase.GetCollection<AdRentLotModel>(adRentLotCollectionName);

                foreach (var n in XmlUtilites.ReadXmlStream<NotificationInfo>(advertsListFile, "notification"))
                {
                    amount++;

                    if (!string.IsNullOrWhiteSpace(n.OdDetailedHref))
                    {
                        var nId = AdvHelpers.GetDirectoryId(n.OdDetailedHref);
                        var nDir = $"{advertsDataDestDir}{Path.DirectorySeparatorChar}{nId.DirectoryName}";
                        var nFile = $"{nDir}{Path.DirectorySeparatorChar}{nId.Id}";

                        var adv = await adRentLotCollection.Find(_ => _.NotificationId == n.BidNumber).FirstOrDefaultAsync();

                        if (!File.Exists(nFile) || adv != null)
                        {
                            if (!Directory.Exists(nDir))
                            {
                                Directory.CreateDirectory(nDir);
                            }

                            try
                            {
                                var result = await DownloadNfDetails(n.OdDetailedHref, nFile);

                                logger.Info($"New: {nFile} - {result}, {amount}");
                            }
                            catch (Exception e)
                            {
                                logger.Error($"While downloading {n.OdDetailedHref} to {nFile}", e);
                            }
                        }
                    }
                }

                async Task<HttpStatusCode> DownloadNfDetails(string url, string dest)
                {
                    var result = await HttpUtilites.DownloadAsync(url, dest);

                    if (File.Exists(dest))
                    {
                        if (File.ReadAllText(dest).Contains("<!DOCTYPE html"))
                        {
                            result = HttpStatusCode.NoContent;
                        }
                    }

                    if (result != HttpStatusCode.OK)
                    {
                        File.Delete(dest);
                    }

                    return result;
                }
            }
            else
            {
                logger.Error($"File {advertsListFile} doesn't exists.");
            }

            logger.Info($"End of downloading data.");
        }

        private async Task DownloadAdvertsInfo(string advertsUrl, string advertsDest, int years)
        {
            try
            {
                if (File.Exists(advertsDest))
                {
                    logger.Info($"Removing file {advertsDest}");

                    File.Delete(advertsDest);
                }

                var dt = DateTime.Now;

                advertsUrl = advertsUrl.
                    Replace("{DATE}", dt.ToString("yyyyMMdd")).
                    Replace("{TIME}", dt.ToString("HHmm"));

                advertsUrl = advertsUrl.
                    Replace("{DATE2}", dt.Subtract(TimeSpan.FromDays(365 * years)).ToString("yyyyMMdd")).
                    Replace("{TIME2}", dt.Subtract(TimeSpan.FromDays(365 * years)).ToString("HHmm"));

                logger.Info($"Downloading {advertsUrl} to {advertsDest}");

                var statusCode = HttpStatusCode.NotFound;

                foreach (var i in Enumerable.Range(0, 10))
                {
                    statusCode = await HttpUtilites.DownloadAsync(advertsUrl, advertsDest, 0);

                    if (statusCode != HttpStatusCode.OK)
                    {
                        logger.Info($"Retry downloading, status: {statusCode}");

                        await Task.Delay(1000);

                        continue;
                    }

                    logger.Info("Downloading OK.");

                    break;
                }

                if (statusCode != HttpStatusCode.OK)
                {
                    logger.Error("Downloading failed.");
                }
            }
            catch (Exception e)
            {
                logger.Error("While downloading adverts", e);
            }
        }
    }
}
