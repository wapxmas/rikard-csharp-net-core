using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RikardWeb.Lib.Adverts.Data;

namespace RikardWeb.Lib.Adverts
{
    static public class AdvConstants
    {
        public static string[] Districts = new string[]
        {
            "ЦАО", "САО", "СВАО", "ВАО", "ЮВАО", "ЮАО", "ЮЗАО", "ЗАО", "СЗАО", "ТиНАО", "ЗелАО"
        };

        public static string[] Areas = new string[]
        {
            "1-50", "50-100", "100-200", "200-500", "500-1000", "1000-10000", "10000-100000"
        };

        public static string[] Floors = new string[]
        {
            "этаж 1", "цоколь", "подвал"
        };

        public static AdvLocation[] Locations = new AdvLocation[]
        {
            new AdvLocation { Name = "в границах Садового кольца", Code = "sadovoe" },
            new AdvLocation { Name = "в границах Третьего кольца", Code = "ttk" },
            new AdvLocation { Name = "в границах МКАД", Code = "within-mkad" },
            new AdvLocation { Name = "за МКАД", Code = "outside-mkad" },
        };

        public static (AdvMission Mission, string Text)[] Purposes = new(AdvMission, string)[]
        {
            (AdvMission.OFIS, "офис"),
            (AdvMission.TORGOVAJA_PLOSHHAD, "торговая площадь"),
            (AdvMission.SKLAD, "склад"),
            (AdvMission.OBSHHEPIT, "общепит"),
            (AdvMission.SVOBODNOE_NAZNACHENIE, "свободное назначение"),
            (AdvMission.AVTOSTOJANKA, "автостоянка"),
            (AdvMission.PROIZVODSTVO, "производство"),
            (AdvMission.AVTOSERVIS, "автосервис"),
            (AdvMission.GOTOVYJ_BIZNES, "готовый бизнес"),
            (AdvMission.OTDELNO_STOJASHHEE_ZDANIE, "отдельно стоящее здание"),
            (AdvMission.BYTOVYE_USLUGI, "бытовые услуги"),
            (AdvMission.MEDICINA, "медицина"),
            (AdvMission.KULTURNOE_NASLEDIE, "культурное наследие"),
            (AdvMission.GOSTINICA, "гостиница"),
            (AdvMission.ZEMELNYJ_UCHASTOK, "земельный участок"),
            (AdvMission.RAZRESHENA_SUBARENDA, "разрешена субаренда"),
            (AdvMission.PRODAZHA_PPA, "продажа ППА"),
            (AdvMission.GARAZH, "гараж"),
            (AdvMission.KINOTEATR, "кинотеатр"),
            (AdvMission.APTEKA, "аптека"),
            (AdvMission.DOKTOR_RJADOM, "доктор рядом"),
            (AdvMission.KAFE, "кафе"),
            (AdvMission.RESTORAN, "ресторан"),
            (AdvMission.STOLOVAJA, "столовая"),
            (AdvMission.MAGAZIN, "магазин"),
            (AdvMission.AVTOSALON, "автосалон"),
            (AdvMission.BANK, "банк"),
            (AdvMission.FOK, "ФОК"),
            (AdvMission.REKLAMNAJA_PLOSHHAD, "рекламная площадь"),
            (AdvMission.DOU, "ДОУ"),
            (AdvMission.OBRAZOVATELNOE, "образовательное"),
            (AdvMission.NEZHILOE, "нежилое"),
            (AdvMission.JEKSKLJUZIV, "эксклюзив"),
            (AdvMission.SOBSTVENNOST, "собственность"),
            (AdvMission.AVTOMOJKA, "автомойка"),
            (AdvMission.NOCHNOJ_KLUB, "ночной клуб"),
            (AdvMission.KVARTIRA, "квартира"),
            (AdvMission.PRJAMAJA_ARENDA, "прямая аренда"),
            (AdvMission.MASHINOMESTO, "машиноместо")
        };

        public static Dictionary<string, string> PurposesMap = new Dictionary<string, string>();
        public static Dictionary<string, string> InversePurposesMap = new Dictionary<string, string>();

        public static string MissionsListToString(List<string> missions)
        {
            var result = new List<string>();

            foreach (var m in missions)
            {
                if(PurposesMap.TryGetValue(m, out string mv))
                {
                    result.Add(mv);
                }
            }

            return string.Join(", ", result);
        }

        static AdvConstants()
        {
            foreach (var p in Purposes)
            {
                if(!PurposesMap.ContainsKey(p.Mission.ToString()))
                {
                    PurposesMap.Add(p.Mission.ToString(), p.Text);
                }

                if(!InversePurposesMap.ContainsKey(p.Text))
                {
                    InversePurposesMap.Add(p.Text, p.Mission.ToString());
                }
            }
        }
    }
}
