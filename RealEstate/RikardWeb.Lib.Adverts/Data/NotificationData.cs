using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace RikardWeb.Lib.Adverts.Data
{
    [Serializable]
    [XmlRoot(ElementName = "fullNotification", Namespace = "http://torgi.gov.ru/opendata")]
    [XmlInclude(typeof(NotificationData))]
    [XmlInclude(typeof(Protocol))]
    public class Notification
    {
        [XmlElement("notification")]
        public NotificationData Data;

        [XmlElement("protocol")]
        public List<Protocol> Protocols;

        public bool CheckNotificationFresh(int minDays = 5)
        {
            if (!string.IsNullOrWhiteSpace(Data?.Common?.ExpireDate))
            {
                DateTime expireDate;

                if (DateTime.TryParseExact(Data?.Common?.ExpireDate, "yyyy-MM-dd", null, DateTimeStyles.None, out expireDate) ||
                    DateTime.TryParseExact(Data?.Common?.ExpireDate, "yyyy-MM-ddTHH:mm:ss", null, DateTimeStyles.None, out expireDate))
                {
                    var currentDate = DateTime.Now;
                    var diff = expireDate - currentDate;

                    return expireDate > currentDate && diff.Days > minDays;
                }
            }

            return false;
        }
    }

    [Serializable]
    [XmlType(TypeName = "notification")]
    [XmlInclude(typeof(BidOrganization))]
    [XmlInclude(typeof(Common))]
    [XmlInclude(typeof(Document))]
    [XmlInclude(typeof(Lot))]
    public class NotificationData
    {
        [XmlElement("bidNumber")]
        public string BidNumber;

        [XmlElement("bidOrganization")]
        public BidOrganization BidOrganization;

        [XmlElement("common")]
        public Common Common;

        [XmlArray("documents")]
        [XmlArrayItem(typeof(Document))]
        public List<Document> Documents;

        [XmlElement("lot")]
        public List<Lot> Lots;
    }

    [Serializable]
    [XmlType(TypeName = "protocol")]
    [XmlInclude(typeof(Committee))]
    [XmlInclude(typeof(ProtocolLot))]
    public class Protocol
    {
        [XmlElement("protocolType")]
        public string ProtocolType;

        [XmlElement("protocolNum")]
        public string ProtocolNum;

        [XmlElement("protocolTown")]
        public string ProtocolTown;

        [XmlElement("protocolDate")]
        public string ProtocolDate;

        [XmlElement("protocolPlace")]
        public string ProtocolPlace;

        [XmlElement("committee")]
        public Committee Committee;

        [XmlElement("lot")]
        public List<ProtocolLot> Lots;
    }

    [Serializable]
    [XmlType(TypeName = "protocolLot")]
    [XmlInclude(typeof(BidMember))]
    [XmlInclude(typeof(Document))]
    public class ProtocolLot
    {
        [XmlElement("lotNum")]
        public string LotNum;

        [XmlElement("cancelReason")]
        public string CancelReason;

        [XmlElement("decision")]
        public string Decision;

        [XmlElement("bidMember")]
        public List<BidMember> BidMembers;

        [XmlArray("documents")]
        [XmlArrayItem(typeof(Document))]
        public List<Document> Documents;
    }

    [Serializable]
    [XmlType(TypeName = "committee")]
    [XmlInclude(typeof(Member))]
    public class Committee
    {
        [XmlElement("member")]
        public List<Member> Members;

        [XmlElement("committePercent")]
        public string CommittePercent;
    }

    [Serializable]
    [XmlType(TypeName = "member")]
    public class Member
    {
        [XmlElement("name")]
        public string Name;

        [XmlElement("role")]
        public string Role;
    }

    [Serializable]
    [XmlType(TypeName = "lot")]
    [XmlInclude(typeof(BidStatus))]
    [XmlInclude(typeof(BidType))]
    [XmlInclude(typeof(PropertyType))]
    [XmlInclude(typeof(PropKind))]
    [XmlInclude(typeof(KladrLocation))]
    [XmlInclude(typeof(Unit))]
    [XmlInclude(typeof(Article))]
    [XmlInclude(typeof(Winner))]
    [XmlInclude(typeof(PaymentRequisites))]
    [XmlInclude(typeof(Document))]
    [XmlInclude(typeof(BidMember))]
    public class Lot
    {
        [XmlElement("lotNum")]
        public string LotNum;

        [XmlElement("id")]
        public string Id;

        [XmlElement("bidStatus")]
        public BidStatus BidStatus;

        [XmlElement("suspendDate")]
        public string SuspendDate;

        [XmlElement("suspendReason")]
        public string SuspendReason;

        [XmlElement("cancelDate")]
        public string CancelDate;

        [XmlElement("cancelReason")]
        public string CancelReason;

        [XmlElement("bidType")]
        public BidType BidType;

        [XmlElement("propertyType")]
        public PropertyType PropertyType;

        [XmlElement("propKind")]
        public PropKind PropKind;

        [XmlElement("reqDecision")]
        public string ReqDecision;

        [XmlElement("cadastralNum")]
        public string CadastralNum;

        [XmlElement("torgReason")]
        public string TorgReason;

        [XmlElement("orgFullName")]
        public string OrgFullName;

        [XmlElement("propDesc")]
        public string PropDesc;

        [XmlElement("objectCode")]
        public string ObjectCode;

        [XmlElement("description")]
        public string Description;

        [XmlElement("mission")]
        public string Mission;

        [XmlElement("kladrLocation")]
        public KladrLocation KladrLocation;

        [XmlElement("postAddress")]
        public string PostAddress;

        [XmlElement("fundSize")]
        public string FundSize;

        [XmlElement("orgNominalValue")]
        public string OrgNominalValue;

        [XmlElement("acsPart")]
        public string AcsPart;

        [XmlElement("location")]
        public string Location;

        [XmlElement("unit")]
        public Unit Unit;

        [XmlElement("area")]
        public string Area;

        [XmlElement("startSalePrice")]
        public string StartSalePrice;

        [XmlElement("condition")]
        public string Condition;

        [XmlElement("federalStockPercent")]
        public string FederalStockPercent;

        [XmlElement("stockNum")]
        public string StockNum;

        [XmlElement("stockPercentSale")]
        public string StockPercentSale;

        [XmlElement("minPrice")]
        public string MinPrice;

        [XmlElement("priceStep")]
        public string PriceStep;

        [XmlElement("stepNegative")]
        public string StepNegative;

        [XmlElement("workList")]
        public string WorkList;

        [XmlElement("marketPartDesc")]
        public string MarketPartDesc;

        [XmlElement("areaUnmovable")]
        public string AreaUnmovable;

        [XmlElement("objectsList")]
        public string ObjectsList;

        [XmlElement("emplNum")]
        public string EmplNum;

        [XmlElement("docsList")]
        public string DocsList;

        [XmlElement("areaMeters")]
        public string AreaMeters;

        [XmlElement("termYear")]
        public int TermYear;

        [XmlElement("termMonth")]
        public int TermMonth;

        [XmlElement("termDay")]
        public int TermDay;

        [XmlElement("article")]
        public Article Article;

        [XmlElement("pricePerMonth")]
        public string PricePerMonth;

        [XmlElement("pricePerYear")]
        public string PricePerYear;

        [XmlElement("startPrice")]
        public string StartPrice;

        [XmlElement("startPriceAreaSm")]
        public string StartPriceAreaSm;

        [XmlElement("step")]
        public string Step;

        [XmlElement("monthPrice")]
        public string MonthPrice;

        [XmlElement("yearPrice")]
        public string YearPrice;

        [XmlElement("dealFee")]
        public string DealFee;

        [XmlElement("contractFee")]
        public string ContractFee;

        [XmlElement("startPricePerMonth")]
        public string StartPricePerMonth;

        [XmlElement("isOverLimitDeal")]
        public string IsOverLimitDeal;

        [XmlElement("depositSize")]
        public string DepositSize;

        [XmlElement("depositDesc")]
        public string DepositDesc;

        [XmlElement("maintenanceSize")]
        public string MaintenanceSize;

        [XmlElement("buildConditions")]
        public string BuildConditions;

        [XmlElement("techConditions")]
        public string TechConditions;

        [XmlElement("isBurdened")]
        public string IsBurdened;

        [XmlElement("burdenDescription")]
        public string BurdenDescription;

        [XmlElement("contractDesc")]
        public string ContractDesc;

        [XmlElement("limit")]
        public string Limit;

        [XmlElement("winnerDefineDesc")]
        public string WinnerDefineDesc;

        [XmlElement("privateConditions")]
        public string PrivateConditions;

        [XmlElement("lastInfo")]
        public string LastInfo;

        [XmlElement("singlePrice")]
        public string SinglePrice;

        [XmlElement("finalPrice")]
        public string FinalPrice;

        [XmlElement("result")]
        public string Result;

        [XmlElement("isSubrent")]
        public string IsSubrent;

        [XmlElement("lotPhotosExist")]
        public string LotPhotosExist;

        [XmlElement("groundViewPlace")]
        public string GroundViewPlace;

        [XmlElement("articleVal")]
        public string ArticleVal;

        [XmlElement("resultStartPriceAreaSm")]
        public string ResultStartPriceAreaSm;

        [XmlElement("bidResults")]
        public string BidResults;

        [XmlElement("contractNum")]
        public string ContractNum;

        [XmlElement("contractDate")]
        public string ContractDate;

        [XmlElement("contractPayment")]
        public string ContractPayment;

        [XmlElement("contractPriceYear")]
        public string ContractPriceYear;

        [XmlElement("contractPriceMonth")]
        public string ContractPriceMonth;

        [XmlElement("contractPriceHour")]
        public string ContractPriceHour;

        [XmlElement("winner")]
        public Winner Winner;

        [XmlElement("currency")]
        public string Currency;

        [XmlElement("currencyPercent")]
        public string CurrencyPercent;

        [XmlElement("paymentRequisites")]
        public PaymentRequisites PaymentRequisites;

        [XmlArray("documents")]
        [XmlArrayItem(typeof(Document))]
        public List<Document> Documents;

        [XmlArray("results")]
        [XmlArrayItem(typeof(BidMember))]
        public List<BidMember> BidMembers;
    }

    [Serializable]
    [XmlType(TypeName = "bidMember")]
    public class BidMember
    {
        [XmlElement("regNum")]
        public string RegNum;

        [XmlElement("name")]
        public string Name;

        [XmlElement("inn")]
        public string Inn;

        [XmlElement("kpp")]
        public string Kpp;

        [XmlElement("ogrnip")]
        public string Ogrnip;

        [XmlElement("ogrn")]
        public string Ogrn;

        [XmlElement("location")]
        public string Location;

        [XmlElement("phone")]
        public string Phone;

        [XmlElement("isRemoved")]
        public string IsRemoved;

        [XmlElement("isSelected")]
        public string IsSelected;

        [XmlElement("refuseReason")]
        public string RefuseReason;

        [XmlElement("tenderPosition")]
        public string TenderPosition;

        [XmlElement("contractTearms")]
        public string ContractTearms;

        [XmlElement("offer")]
        public string Offer;
    }

    [Serializable]
    [XmlType(TypeName = "winner")]
    public class Winner
    {
        [XmlElement("name")]
        public string Name;

        [XmlElement("inn")]
        public string Inn;

        [XmlElement("kpp")]
        public string Kpp;

        [XmlElement("ogrnip")]
        public string Ogrnip;

        [XmlElement("ogrn")]
        public string Ogrn;

        [XmlElement("location")]
        public string Location;

        [XmlElement("phone")]
        public string Phone;
    }

    [Serializable]
    [XmlType(TypeName = "paymentRequisites")]
    public class PaymentRequisites
    {
        [XmlElement("bik")]
        public string Bik;

        [XmlElement("bankName")]
        public string BankName;

        [XmlElement("ks")]
        public string Ks;

        [XmlElement("rs")]
        public string Rs;

        [XmlElement("ps")]
        public string Ps;
    }

    [Serializable]
    [XmlType(TypeName = "article")]
    public class Article
    {
        [XmlElement("id")]
        public int Id;

        [XmlElement("name")]
        public string Name;
    }

    [Serializable]
    [XmlType(TypeName = "unit")]
    public class Unit
    {
        [XmlElement("id")]
        public string Id;

        [XmlElement("name")]
        public string Name;
    }

    [Serializable]
    [XmlType(TypeName = "kladrLocation")]
    public class KladrLocation
    {
        [XmlElement("id")]
        public string Id;

        [XmlElement("name")]
        public string Name;
    }

    [Serializable]
    [XmlType(TypeName = "propKind")]
    public class PropKind
    {
        [XmlElement("id")]
        public string Id;

        [XmlElement("name")]
        public string Name;
    }

    [Serializable]
    [XmlType(TypeName = "propertyType")]
    public class PropertyType
    {
        [XmlElement("id")]
        public string Id;

        [XmlElement("name")]
        public string Name;
    }

    [Serializable]
    [XmlType(TypeName = "bidType")]
    public class BidType
    {
        [XmlElement("id")]
        public string Id;

        [XmlElement("name")]
        public string Name;
    }

    [Serializable]
    [XmlType(TypeName = "bidStatus")]
    public class BidStatus
    {
        [XmlElement("id")]
        public string Id;

        [XmlElement("name")]
        public string Name;
    }

    [Serializable]
    [XmlType(TypeName = "doc")]
    public class Document
    {
        [XmlElement("docType")]
        public string DocType;

        [XmlElement("docDate")]
        public string DocDate;

        [XmlElement("docNum")]
        public string DocNum;

        [XmlElement("description")]
        public string Description;

        [XmlElement("created")]
        public string Created;

        [XmlElement("docUrl")]
        public string DocUrl;
    }

    [Serializable]
    [XmlType(TypeName = "common")]
    [XmlInclude(typeof(BidKind))]
    [XmlInclude(typeof(BidForm))]
    public class Common
    {
        [XmlElement("bidKind")]
        public BidKind BidKind;

        [XmlElement("bidForm")]
        public BidForm BidForm;

        [XmlElement("bidUrl")]
        public string BidUrl;

        [XmlElement("fio")]
        public string Fio;

        [XmlElement("lotNum")]
        public string LotNum;

        [XmlElement("published")]
        public string Published;

        [XmlElement("lastChanged")]
        public string LastChanged;

        [XmlElement("timeOut")]
        public string TimeOut;

        [XmlElement("notificationUrl")]
        public string NotificationUrl;

        [XmlElement("isFas")]
        public string IsFas;

        [XmlElement("startDateRequest")]
        public string StartDateRequest;

        [XmlElement("isOnlySmps")]
        public string IsOnlySmps;

        [XmlElement("docProvide")]
        public string DocProvide;

        [XmlElement("docChargeRateRur")]
        public string DocChargeRateRur;

        [XmlElement("expireDate")]
        public string ExpireDate;

        [XmlElement("appReceiptDetails")]
        public string AppReceiptDetails;

        [XmlElement("appRequirement")]
        public string AppRequirement;

        [XmlElement("appWithdraw")]
        public string AppWithdraw;

        [XmlElement("condition")]
        public string Condition;

        [XmlElement("winnerDefineDate")]
        public string WinnerDefineDate;

        [XmlElement("winnerDefinePlace")]
        public string WinnerDefinePlace;

        [XmlElement("appChange")]
        public string AppChange;

        [XmlElement("placeRequest")]
        public string PlaceRequest;

        [XmlElement("bidAuctionDate")]
        public string BidAuctionDate;

        [XmlElement("bidAuctionPlace")]
        public string BidAuctionPlace;

        [XmlElement("summationDate")]
        public string SummationDate;

        [XmlElement("summationPlace")]
        public string SummationPlace;

        [XmlElement("winnerDefineDescr")]
        public string WinnerDefineDescr;

        [XmlElement("bulletinNumber")]
        public string BulletinNumber;

        [XmlElement("processingDate")]
        public string ProcessingDate;
    }

    [Serializable]
    [XmlType(TypeName = "bidForm")]
    public class BidForm
    {
        [XmlElement("id")]
        public string Id;

        [XmlElement("name")]
        public string Name;
    }

    [Serializable]
    [XmlType(TypeName = "bidKind")]
    public class BidKind
    {
        [XmlElement("id")]
        public string Id;

        [XmlElement("name")]
        public string Name;
    }

    [Serializable]
    [XmlType(TypeName = "bidOrganization")]
    public class BidOrganization
    {
        [XmlElement("bidOrgKind")]
        public string BidOrgKind;

        [XmlElement("organizationId")]
        public string OrganizationId;

        [XmlElement("fullName")]
        public string FullName;

        [XmlElement("headOrg")]
        public string HeadOrg;

        [XmlElement("limitBidDeal")]
        public string LimitBidDeal;

        [XmlElement("inn")]
        public string Inn;

        [XmlElement("kpp")]
        public string Kpp;

        [XmlElement("okato")]
        public string Okato;

        [XmlElement("okpo")]
        public string Okpo;

        [XmlElement("okved")]
        public string Okved;

        [XmlElement("ogrn")]
        public string Ogrn;

        [XmlElement("address")]
        public string Address;

        [XmlElement("phone")]
        public string Phone;

        [XmlElement("fax")]
        public string Fax;

        [XmlElement("location")]
        public string Location;

        [XmlElement("url")]
        public string Url;
    }
}
