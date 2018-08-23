using RikardLib.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RikardWeb.Lib.Adverts
{
    public static class AdvHelpers
    {
        public static List<(List<AdvMission>, List<string>)> Missions;

        private static string[] ConstantFloorWords = new string[]
        {
            "подвал", "полуподвал", "цоколь", "мезонин", "цокольный", "подвале", "техподполье", "подполье"
        };

        private static string[] ConstantRoomCountWords = new string[]
        {
            "комнаты ", "комната ", "ком ", "комн.", "ком.", "комн ", "к. "
        };

        public static (string DirectoryName, string Id) GetDirectoryId(string notificationUrl)
        {
            var idString = new string(notificationUrl.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());

            if (string.IsNullOrWhiteSpace(idString) || !int.TryParse(idString, out int idNum))
            {
                throw new ArgumentException($"Incorrect notification Id from url {notificationUrl}");
            }

            return ((idNum < 10000 ? 0 : idNum / 10000).ToString(), idString);
        }

        static AdvHelpers()
        {
            Missions = new List<(List<AdvMission>, List<string>)>();

            Missions.Add((
                new List<AdvMission> {
                    AdvMission.MEDICINA
                },
                new List<string> {
                    "оказание медицинских услуг"
                    , "медицинское"
                    , "детская офтальмология"
                    , "под медицинскую деятельность"
                    , "под медицинские услуги"
                    , "для использования под размещение стоматологической клиники"
                    , "объект здравоохранения"
                    , "ветеринарная клиника"
                    , "медицинская деятельность"
                    , "медицинская"
                }));

            Missions.Add((
                new List<AdvMission> {
                    AdvMission.OFIS
                },
                new List<string> {
                    "офис"
                    , "офисное"
                    , "учрежденческие цели"
                    , "учрежденческие"
                    , "под офис"
                    , "под административные цели"
                    , "административное"
                    , "акдминистративное"
                    , "офисное помещение"
                    , "для использования под офис"
                    , "размещение офиса"
                    , "административно-офисное"
                    , "офис класса с"
                    , "под конторские помещения"
                    , "под размещение офиса"
                    , "этажное офисное помещение"
                    , "офисные помещения в строении на территории домовладения"
                    , "административное использование"
                    , "офисные помещения"
                    , "для использование под офисные помещения"
                    , "нежилое помещение для размещения офиса"
                    , "административное и (или) хозяйственное"
                    , "под служебные цели"
                    , "офисная"
                    , "целевое назначение административное (офис)"
                    , "целевое назначение административное"
                    , "офис-продаж"
                    , "учрежденческое"
                    , "административное помещение"
                    , "под офис медицинской организации"
                    , "цель аренды под офис"
                    , "организация офиса"
                    , "для размещения офиса"
                    , "для использования в качестве офиса"
                    , "административные помещения (офис)"
                    , "нежилое помещение офисного назначения"
                    , "для осуществления офисной деятельности"
                    , "для офисной работы"
                    , "офисное назначение"
                }));

            Missions.Add((
                new List<AdvMission> {
                    AdvMission.SVOBODNOE_NAZNACHENIE
                },
                new List<string> {
                    "свободное"
                    , "помещения свободного назначения"
                    , "помещение свободного назначения"
                    , "функциональное назначение свободное"
                    , "свободное назначение"
                    , "нежилое"
                    , "назначение нежилое"
                    , "помещения должны использоваться в качестве нежилого"
                    , "свободного назначения"
                    , "нежилые помещения"
                    , "нежилое помещение"
                    , "свободное в соответствии с действующим законодательством рф"
                    , "свободное (исключения изложены в документации)"
                    , "нежилые здания"
                    , "нежиое помещение"
                    , "нежилые помещения свободного назначения"
                    , "универсальное"
                    , "для осуществления коммерческой деятельности"
                    , "коммерческая деятельность"
                    , "для любой деятельности"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.TORGOVAJA_PLOSHHAD,
                    AdvMission.OFIS
                },
                new List<string>
                {
                    "торгово-офисное"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.BYTOVYE_USLUGI
                },
                new List<string>
                {
                    "бытовое обслуживание"
                    , "социальные услуги"
                    , "оказание услуг населению"
                    , "парикмахерская"
                    , "специализация объекта-бытовые услуги"
                    , "бытовые услуги"
                    , "предоставление бытовых услуг населению"
                    , "услуги"
                    , "оказание бытовых услуг"
                    , "услуги бытового обслуживания"
                    , "под размещение парикмахерской"
                    , "размещение парикмахерской"
                    , "для предоставления бытовых услуг"
                    , "парикмахерские и косметические услуги"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.SKLAD
                },
                new List<string>
                {
                    "склад"
                    , "складское"
                    , "складские"
                    , "под склад"
                    , "размещение склада"
                    , "помещение под склад"
                    , "под размещение склада"
                    , "складское помещение в жилом доме"
                    , "теплый склад"
                    , "склад (неотапливаемый)"
                    , "холодный склад"
                    , "складские помещения"
                    , "складское помещение"
                    , "техническое помещение (склад)"
                    , "складская"
                    , "под cклад"
                    , "для размещения склада"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.DOU,
                    AdvMission.OBRAZOVATELNOE
                },
                new List<string>
                {
                    "объект негосударственного дошкольного образования"
                    , "дошкольное образование"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.OBRAZOVATELNOE
                },
                new List<string>
                {
                    "функциональное назначение начальное общее"
                    , "начальное общее"
                    , "осуществление образовательной деятельности"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.AVTOSTOJANKA
                },
                new List<string>
                {
                    "автостоянка"
                    , "под размещение автотранспорта"
                    , "для стоянки автотранспорта"
                    , "под стоянку автотранспортного средства"
                    , "под стоянку автотранспорта"
                    , "размещение стоянки автотранспорта"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.AVTOSERVIS
                },
                new List<string>
                {
                    "пункт технического обслуживания"
                    , "автосервис"
                    , "техническое обслуживание и ремонт автомобилей"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.AVTOMOJKA
                },
                new List<string>
                {
                    "автомойка"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.GARAZH
                },
                new List<string>
                {
                    "гараж"
                    , "для размещения гаража"
                    , "нежилое помещение (гараж)"
                    , "для использования под гараж"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.PROIZVODSTVO
                },
                new List<string>
                {
                    "легкое производство"
                    , "помещения пригодны для использования в производственных целях"
                    , "производство"
                    , "производственное"
                    , "помещение производственного назначения"
                    , "размещение производства"
                    , "под производство"
                    , "производственное использование"
                    , "производственные помещения"
                    , "научно-производственные помещения"
                    , "производ помещение"
                    , "производственное помещение"
                    , "помещение приветственного назначения"
                    , "производственная"
                    , "под пищевое производство"
                    , "произволство"
                    , "пр-во"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.FOK
                },
                new List<string>
                {
                    "физкультурно-оздоровительный комплекс"
                    , "объект предназначен для оказания услуг тренажерного зала"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.OBSHHEPIT,
                    AdvMission.STOLOVAJA
                },
                new List<string>
                {
                    "организация питания"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.OBSHHEPIT,
                    AdvMission.KAFE
                },
                new List<string>
                {
                    "буфет"
                    , "кафе"
                    , "организация общественного питания (буфет)"
                    , "под размещение буфета"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.OBSHHEPIT
                },
                new List<string>
                {
                    "организация общественного питания"
                    , "общественное питание"
                    , "предприятие общественного питания"
                    , "организация пункта общественного питания"
                    , "нежилое помещение под пункт общественного питания"
                    , "общепит"
                    , "общественное питание (в соответствии с документацией об аукционе)"
                    , "общественное питание-в соответствии с документацией об аукционе"
                    , "пункт общественного питания (ресторан быстрого питания)"
                    , "оказание услуг общественного питания"
                    , "оказание услуг в сфере общественного питания"
                    , "пункт общественного питания"
                    , "для организации питания"
                    , "размещение организации общественного питания"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.STOLOVAJA
                },
                new List<string>
                {
                    "под пищевое производство (столовая)"
                    , "столовая"
                    , "кухня для приготовления пищи для сотрудников института и посетителей"
                    , "организация питания для сотрудников и посетителей больницы"
                    , "под организацию питания сотрудников арендодателя"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.RESTORAN
                },
                new List<string>
                {
                    "ресторан"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.MAGAZIN
                },
                new List<string>
                {
                    "магазин"
                    , "продовольственный магазин с отделом кулинария"
                    , "книжный магазин"
                    , "канцелярский магазин"
                    , "для размещения магазина"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.MAGAZIN,
                    AdvMission.TORGOVAJA_PLOSHHAD
                },
                new List<string>
                {
                    "реализация продуктов питания"
                    , "размещение магазина по продаже продовольственных товаров"
                    , "под размещение магазина"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.TORGOVAJA_PLOSHHAD
                },
                new List<string>
                {
                    "торговое помещение"
                    , "предприятие розничной торговли"
                    , "специализация объекта-галантерейные товары"
                    , "специализация объекта-печать"
                    , "розничная торговля"
                    , "галантерейные товары"
                    , "церковная продукция"
                    , "продовольственные товары"
                    , "торговля изделиями оптики"
                    , "торговое"
                    , "торговый центр"
                    , "торговое (неотапливаемое)"
                    , "для организации торговли продуктами питания и сопутствующими товарами"
                    , "под торговлю"
                    , "хозяйственные товары"
                    , "объект предназначен для торговли товарами спортивного назначения"
                    , "под торговый павильон"
                    , "торговля"
                    , "торговля кондитерскими изделиями"
                    , "торговля книгами"
                    , "розничная торговля алкогольной продукцией"
                    , "торговая точка"
                    , "торговая деятельность"
                    , "осуществление розничной торговли"
                    , "под торговое помещение"
                    , "коммерческая деятельность (торговля)"
                    , "торговая"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.OFIS,
                    AdvMission.SKLAD
                },
                new List<string>
                {
                    "офисное-складское"
                    , "склад и офис"
                    , "офис-склад"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.KVARTIRA
                },
                new List<string>
                {
                    "для проживания сотрудников арендатора (штатных-внештатных) и членов их семей"
                    , "для размещения сотрудников арендатора (штатных-внештатных)"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.MASHINOMESTO
                },
                new List<string>
                {
                    "машиноместо"
                    , "машиноместа"
                    , "машиноместа на подземной охраняемой парковке"
                    , "машиноместо на подземной охраняемой парковке"
                    , "машиномест в многоуровневом отапливаемом гараже"
                    , "машиноместо на уровне подземной охраняемой парковки"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.PROIZVODSTVO,
                    AdvMission.SKLAD
                },
                new List<string>
                {
                    "в производственно-складских целях"
                    , "производственно-складское использование"
                    , "производственно-складские"
                    , "производственно-складское"
                    , "производственно-складская"
                    , "под производственно-складское помещение"
                    , "производственно-складское с административными помещениями"
                    , "производственно-складские помещения"
                    , "производственно-складское класса с"
                    , "под производственные помещения для приготовления пищи"
                    , "под производственно-складские помещения"
                    , "цель аренды под производственно-складские помещения"
                    , "производственно-складское помещение"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.KAFE,
                    AdvMission.STOLOVAJA
                },
                new List<string>
                {
                    "кафе-столовая"
                    , "для размещения буфета"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.TORGOVAJA_PLOSHHAD,
                    AdvMission.BYTOVYE_USLUGI,
                    AdvMission.OFIS
                },
                new List<string>
                {
                    "торгово-офисное-быт обслуживание"
                    , "под осуществление деятельности по приоритетным напрвлениям бытового обслуживания населения"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.APTEKA
                },
                new List<string>
                {
                    "аптека"
                    , "для размещения аптеки"
                    , "размещение аптеки"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.GOSTINICA
                },
                new List<string>
                {
                    "общежитие"
                }));

            Missions.Add((
                new List<AdvMission>
                {
                    AdvMission.OFIS,
                    AdvMission.PROIZVODSTVO
                },
                new List<string>
                {
                    "офисно-производственное"
                }));
        }

        public static int ExtractRoomsCount(string source1, string source2)
        {
            var rmsc = GetRoomsCount(source1);

            return rmsc == 0 ? GetRoomsCount(source2) : rmsc;

            int GetRoomsCount(string source)
            {
                int rCount = 0;

                if (!string.IsNullOrWhiteSpace(source))
                {
                    source = source
                        .ToLowerInvariant()
                        .Map(c => char.IsLetterOrDigit(c) || c == '-' || c == '.' || c == ',' || c == '(' || c == ')' ? c : ' ')
                        .TrimAndCompactWhitespaces();

                    foreach (var rnm in ConstantRoomCountWords)
                    {
                        rCount += GetRoomsCountAll(rnm, source.AllIndexesOf(rnm), source);
                    }

                }

                return rCount;
            }

            int GetRoomsCountAll(string rnm, List<int> occs, string source)
            {
                int rCount = 0;

                foreach (var occ in occs)
                {
                    rCount += GetRoomsCountOne(source.Substring(occ + rnm.Length));
                }

                return rCount;
            }

            int GetRoomsCountOne(string source)
            {
                var rooms = new HashSet<string>();

                var rms = source
                    .SplitNoEmptyTrim(new char[] { ',' })
                    .Select(s => s.RemoveBetween('(', ')'))
                    .Select(s => s.TrimAndCompactWhitespaces())
                    .Where(s => s.Length > 0)
                    .TakeWhile(s => char.IsDigit(s.ElementAt(0)));

                var vms = rms
                    .Select(s => new string(s.TakeWhile(c => char.IsLetterOrDigit(c) || c == '-').ToArray()))
                    .TakeWhile(s => s.SplitNoEmptyTrim(new char[] { ' ' }).Count() == 1).ToList();

                if (vms.Count > 0)
                {
                    if (rms.Count() > vms.Count)
                    {
                        var s = rms.Skip(vms.Count).FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            var ws = s.SplitNoEmptyTrim(new char[] { ' ' });
                            var sw = ws.Skip(1).FirstOrDefault();
                            s = ws.FirstOrDefault();

                            if (!string.IsNullOrWhiteSpace(s) && !sw.Contains("этаж"))
                            {
                                vms.Add(s);
                            }
                        }
                    }

                    vms.ForEach(r => GetRoomsCountCompute(r, rooms));
                }

                return rooms.Count;
            }

            void GetRoomsCountCompute(string source, HashSet<string> rooms)
            {
                var cmp = source.SplitNoEmptyTrim(new char[] { '-' });

                if (cmp.Count() == 1)
                {
                    rooms.Add(source);
                }
                else if (cmp.Count() > 1)
                {
                    var f = cmp.FirstOrDefault();
                    var l = cmp.LastOrDefault();

                    if (!string.IsNullOrWhiteSpace(f) && !string.IsNullOrWhiteSpace(l))
                    {
                        if (int.TryParse(f, out int fint) && int.TryParse(l, out int lint))
                        {
                            var r = lint - fint;

                            if (r > 0)
                            {
                                for (int i = fint; i <= lint; i++)
                                {
                                    rooms.Add(i.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }

        public static List<string> ExtractFloor(List<string> ss)
        {
            var fh = new HashSet<string>();

            foreach (var s in ss)
            {
                ExtractStr(s).ForEach(str => fh.Add(str));
            }

            return fh.ToList();

            List<string> ExtractStr(string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new List<string>();
                }

                return ExtractWordFloor().Concat(ExtractNumFloor()).ToList();

                List<string> ExtractNumFloor()
                {
                    var fnums = new List<string>();

                    var floorStr = str
                        .Replace("в комплексе", ",")
                        .Replace("квартира на", ",")
                        .Replace("в доме", ",")
                        .Replace("в жилом доме на", ",")
                        .Replace("в жилом доме", ",")
                        .Replace("в жилом кирпичном доме", ",")
                        .Replace("жилого дома", ",")
                        .Replace("помещение на", ",")
                        .Replace("-ом", string.Empty)
                        .Replace("-ый", string.Empty)
                        .Replace("-ой", string.Empty)
                        .Replace("-м", string.Empty)
                        .Replace("-й", string.Empty)
                        .Replace("этаже", "этаж")
                        .Replace(" на ", ",")
                        .Replace(" на", ",")
                        .Replace(" эт,", " этаж,")
                        .Replace(" эт ,", " этаж,")
                        .Replace("по документам БТИ", ",")
                        .Replace("расположенные на", ",")
                        .Map(ch => ch == '(' || ch == ')' || ch == '-' || ch == '–' || ch == ';' || ch == ':' ? ',' : ch)
                        .TrimAndCompactWhitespaces()
                        .ToLowerInvariant()
                        .SplitNoEmptyTrim(new char[] { ',' });

                    foreach (var flrs in floorStr)
                    {
                        var flnum = TryGetFloor(flrs);

                        if (!string.IsNullOrWhiteSpace(flnum))
                        {
                            fnums.Add("этаж " + flnum);
                        }
                    }

                    return fnums;

                    string TryGetFloor(string tryStr)
                    {
                        var ws = tryStr.SplitNoEmptyTrim(new char[] { ' ' });

                        if (!ws.Contains("этаж") || ws.Count() != 2)
                        {
                            return string.Empty;
                        }
                        else
                        {
                            var flnum = ws.Where(s => s != "этаж").FirstOrDefault();

                            if (string.IsNullOrWhiteSpace(flnum))
                            {
                                return string.Empty;
                            }

                            if (flnum.All(char.IsDigit))
                            {
                                return flnum;
                            }

                            return string.Empty;
                        }
                    }
                }

                List<string> ExtractWordFloor()
                {
                    var wss = str
                        .SaveLettersAndNumbers()
                        .TrimAndCompactWhitespaces()
                        .ToLowerInvariant()
                        .SplitNoEmptyTrim(new char[] { ' ' });
                    return wss
                        .Where(w => ConstantFloorWords.Contains(w))
                        .Select(FixFloorWords)
                        .ToList();
                }

                string FixFloorWords(string w)
                {
                    switch (w)
                    {
                        case "цокольный":
                            return "цоколь";
                        case "подвале":
                            return "подвал";
                        case "подполье":
                            return "техподполье";
                    }
                    return w;
                }
            }
        }

        public static List<AdvMission> GetMissions(string rawMission)
        {
            if (string.IsNullOrWhiteSpace(rawMission))
            {
                return new List<AdvMission>();
            }

            var missions = new List<List<AdvMission>>();

            var textLines = rawMission.SplitNoEmptyTrim(new char[] { '\r', '\n' });

            foreach (var tl in textLines)
            {
                var text = tl.TrimAndCompactWhitespaces();
                text = text.Replace(" и т.п.", "");
                text = text.Replace(" и др.виды деятельности", "");
                text = text.Replace(" -", "-");
                text = text.Replace("- ", "-");
                text = text.Replace(" - ", "-");
                text = text.Replace("(штатных/внештатных)", "(штатных-внештатных)");
                text = text.Replace("+", "-");
                text = text.Map(c => c == '/' || c == ';' ? ',' : c);
                text = text.Map(c => char.IsLetter(c) || c == '-' || c == ',' || c == ')' || c == '(' ? c : ' ');
                text = text.TrimAndCompactWhitespaces();
                text = text.ToLowerInvariant();

                var tlist = text.SplitNoEmptyTrim(new char[] { ',' });
                tlist = tlist.Select(s => s.TrimAndCompactWhitespaces());

                foreach (var t in tlist)
                {
                    foreach (var (ms, ts) in Missions)
                    {
                        if (MissionExists(t, ts))
                        {
                            missions.Add(ms);
                        }
                    }
                }
            }

            var mr = new HashSet<AdvMission>();

            missions.ForEach(l => l.ForEach(m => mr.Add(m)));

            return mr.ToList();

            bool MissionExists(string t, List<string> ms)
            {
                foreach (var m in ms)
                {
                    if (m == t)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
