using WeiZed.Core;
using WeiZed.MySQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GTANetworkAPI;

namespace WeiZed.Bussines
{
    class OilCompanyBase
    {
        public static Dictionary<int, OilCompanyBase> AllOilCompaniesDictionary = new Dictionary<int, OilCompanyBase>();  // Exmpl: <autoroom_ID, autoroomInstance>
        private static readonly Vector3[] officeInteriorsArray =
        {
            new Vector3(-774.0349, 342.0296, 196.6862), // Mansion (Aqua 3 Apartment	apa_v_mp_h_08_b	new mp.Vector3(-774.0349, 342.0296, 196.6862);)
            new Vector3(152.2605, -1004.471, -98.99999), // Motel
            new Vector3(-1908.024, -573.4244, 19.09722) // House on the beach
        };
        private int oilCompany_ID { get; set; }
        private string ownerRPName { get; set; }
        private string ownerSocialName { get; set; }
        private string type { get; set; } = null;
        private Vector3 officePosition { get; set; } = new Vector3();
        private int price { get; set; } = 0;
        private Blip blip;
        public ColShape shape;
        private TextLabel textLabel;





        public static void CreateNewCompany(Player sender, string type, int price)
        {
            OilCompanyBase newCompany = new OilCompanyBase();

            //Проверка на существование щаписи в базе для присвоения ID дому
            string DBReturn = MySQLStatic.QueryRead($"SELECT `oilCompany_ID` FROM `oil_companies` WHERE `oilCompany_ID`=0");
            bool isTableExist = (DBReturn.Length != 0) ? false : true;
            if (isTableExist)
                newCompany.oilCompany_ID = 1;
            else
                newCompany.oilCompany_ID = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `oilCompany_ID` FROM `oil_companies` ORDER BY `oilCompany_ID` DESC LIMIT 1")) + 1;
            //Проверка на существование щаписи в базе для присвоения ID дому

            newCompany.type = type;
            newCompany.officePosition = (sender.Position - new Vector3(0, 0, 1.33));
            newCompany.price = price;
            newCompany.ownerRPName = null;
            newCompany.ownerSocialName = null;

            #region MySQL Operations
            MySQLStatic.Query($"INSERT INTO `oil_companies` (`id`, `oilCompany_ID`, `ownerRPName`, `ownerSocialName`, `type`, `officePosition`, `price`)" +
                    $" VALUES(NULL, " +
                    $"'{newCompany.oilCompany_ID}', " +
                    $"'{newCompany.ownerRPName}', " +
                    $"'{newCompany.ownerSocialName}'," +
                    $"'{newCompany.type}'," +
                    $"'{JsonConvert.SerializeObject(newCompany.officePosition)}', " +
                    $"'{newCompany.price}')");
            NAPI.Util.ConsoleOutput("Created oilCompany record in db");
            #endregion MySQL Operations
            /////////////////////////////////////////////
            int DictKey = newCompany.oilCompany_ID;
            OilCompanyBase.AllOilCompaniesDictionary.Add(DictKey, newCompany);
            /////////////////////////////////////////////
            newCompany.CreateVisualInfoForNewCompany();
        }//Для создания через команду
        public void CreateVisualInfoForNewCompany()
        {
            blip = NAPI.Blip.CreateBlip(officePosition);
            textLabel = NAPI.TextLabel.CreateTextLabel($"Нефтяное предприятие #{oilCompany_ID}", officePosition + new Vector3(0, 0, 1.5), 5f, 0.4f, 0, new Color(255, 255, 255), false, 0);
            #region Creating Blip // метка компании
            if (string.IsNullOrEmpty(ownerSocialName))
            {
                blip.Sprite = GetBlipID(type);
                blip.Color = 2;
            }
            else
            {
                blip.Sprite = GetBlipID(type);
                blip.Color = 49;
            }

            blip.Scale = 1f;
            blip.ShortRange = true;
            #endregion

            #region Creating Marker & Colshape
            shape = NAPI.ColShape.CreateCylinderColShape(officePosition, 1, 2, 0);
            shape.OnEntityEnterColShape += (colshape, player) =>
            {
                player.SetData("oilCompany_ID", oilCompany_ID);
                shape.SetSharedData("colshapeType", "oilcompany");
                shape.SetSharedData("object_ID", oilCompany_ID);
            };
            shape.OnEntityExitColShape += (colshape, player) =>
            {
                player.ResetData("oilCompany_ID");
                shape.ResetSharedData("colshapeType");
                shape.ResetSharedData("object_ID");
            };
            #endregion

            #region TextLabel
            string text = $"~w~Нефтяное предприятие ~b~#{oilCompany_ID}\n\n~w~Тип: ~b~{type}\n";
            if (string.IsNullOrEmpty(ownerSocialName))
            {
                text += $"~w~Цена: ~b~{price}$\n";
            }
            else
            {
                text += $"~w~Владелец: ~b~{ownerRPName}\n";
            }

            textLabel.Text = text;
            #endregion TextLabel
        }//Для создания через команду



        public static void LoadOilCompaniesFromBD()
        {
            int allCompanies = 0;
            string result = Convert.ToString(MySQLStatic.QueryRead($"SELECT `oilCompany_ID` FROM `oil_companies` ORDER BY `oilCompany_ID` DESC LIMIT 1"));

            if (result != "")
            {
                allCompanies = Convert.ToInt32(result);
                NAPI.Util.ConsoleOutput("Last oilCompanies id in base - " + allCompanies);
                for (int i = 1; i <= allCompanies; i++)
                {
                    int DictKey = i;
                    OilCompanyBase.AllOilCompaniesDictionary.Add(DictKey, CreateOilCompanyFromBD(i));
                }
                NAPI.Util.ConsoleOutput("oilCompanies added to AllOilCompaniesDict - " + OilCompanyBase.AllOilCompaniesDictionary.Count.ToString());
            }
            else
            {
                NAPI.Util.ConsoleOutput("Not found oilCompany in base!");
            }

        }//Для создания из бд
        public static OilCompanyBase CreateOilCompanyFromBD(int oilCompany_ID)//Для создания из бд
        {
            OilCompanyBase oilCompany = new OilCompanyBase();
            oilCompany.oilCompany_ID = oilCompany_ID;
            oilCompany.ownerRPName = MySQLStatic.QueryRead($"SELECT `ownerRPName` FROM `oil_companies` WHERE `oilCompany_ID` = {oilCompany_ID}");
            oilCompany.ownerSocialName = MySQLStatic.QueryRead($"SELECT `ownerSocialName` FROM `oil_companies` WHERE `oilCompany_ID` = {oilCompany_ID}");
            oilCompany.type = MySQLStatic.QueryRead($"SELECT `type` FROM `oil_companies` WHERE `oilCompany_ID` = {oilCompany_ID}");
            oilCompany.officePosition = JsonConvert.DeserializeObject<Vector3>(MySQLStatic.QueryRead($"SELECT `officePosition` FROM `oil_companies` WHERE `oilCompany_ID` = {oilCompany_ID}"));
            oilCompany.price = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `price` FROM `oil_companies` WHERE `oilCompany_ID` = {oilCompany_ID}"));

            oilCompany.CreateVisualInfoForDBCompany();
            return oilCompany;
        }
        public void CreateVisualInfoForDBCompany()
        {
            blip = NAPI.Blip.CreateBlip(officePosition);
            textLabel = NAPI.TextLabel.CreateTextLabel($"Нефтяное предприятие #{oilCompany_ID}", officePosition + new Vector3(0, 0, 1.5), 5f, 0.4f, 0, new Color(255, 255, 255), false, 0);
            #region Creating Blip // метка company
            if (string.IsNullOrEmpty(ownerSocialName))
            {
                blip.Sprite = GetBlipID(type);
                blip.Color = 2;
            }
            else
            {
                blip.Sprite = GetBlipID(type);
                blip.Color = 49;
            }

            blip.Scale = 1f;
            blip.ShortRange = true;
            #endregion

            #region Creating Marker & Colshape
            shape = NAPI.ColShape.CreateCylinderColShape(officePosition, 1, 2, 0);
            shape.OnEntityEnterColShape += (colshape, player) =>
            {
                player.SetData("oilCompany_ID", oilCompany_ID);
                shape.SetSharedData("colshapeType", "oilcompany");
                shape.SetSharedData("object_ID", oilCompany_ID);
            };
            shape.OnEntityExitColShape += (colshape, player) =>
            {
                player.ResetData("oilCompany_ID");
                shape.ResetSharedData("colshapeType");
                shape.ResetSharedData("object_ID");
            };
            #endregion

            #region TextLabel
            string text = $"~w~Нефтяное предприятие ~b~#{oilCompany_ID}\n\n~w~Тип: ~b~{type}\n";
            if (string.IsNullOrEmpty(ownerSocialName))
            {
                text += $"~w~Цена: ~b~{price}$\n";
            }
            else
            {
                text += $"~w~Владелец: ~b~{ownerRPName}\n";
            }

            textLabel.Text = text;
            #endregion TextLabel
        }//Для создания из бд (разница в услоовиях,ебаные isnull и length = 0)


        public int GetInterior(string type)
        {
            switch (type)
            {
                case "Начинающее":
                    return 0;
                case "Среднее":
                    return 1;
                case "Крупное":
                    return 2;
                default:
                    return -1;
            }
        }
        public uint GetBlipID(string type)
        {
            switch (type)
            {
                case "Начинающее":
                    return 805;
                case "Среднее":
                    return 805;
                case "Крупное":
                    return 805;
                default:
                    return 66;
            }
        }
        public static void UpdateVisualInfo(int oilCompany_ID)
        {
            OilCompanyBase targetCompany;
            OilCompanyBase.AllOilCompaniesDictionary.TryGetValue(oilCompany_ID, out targetCompany);
            #region textLabel
            string text = $"~w~Нефтяное предприятие ~b~#{oilCompany_ID}\n\n~w~Тип: ~b~{targetCompany.type}\n";
            if (string.IsNullOrEmpty(targetCompany.ownerSocialName))
            {
                text += $"~w~Цена: ~b~{targetCompany.price}$\n";
            }
            else
            {
                text += $"~w~Владелец: ~b~{targetCompany.ownerRPName}\n";
            }

            targetCompany.textLabel.Text = text;
            #endregion textLabel

            #region Blip
            if ((MySQLStatic.QueryRead($"SELECT `ownerSocialName` FROM `oil_companies` WHERE `oilCompany_ID` = {targetCompany.oilCompany_ID}")).Length == 0)
            {
                targetCompany.blip.Sprite = targetCompany.GetBlipID(targetCompany.type);
                targetCompany.blip.Color = 2;
            }
            else
            {
                targetCompany.blip.Sprite = targetCompany.GetBlipID(targetCompany.type);
                targetCompany.blip.Color = 49;
            }

            targetCompany.blip.Scale = 1f;
            targetCompany.blip.ShortRange = true;
            #endregion Blip
        }//Для отрисовки после покупки/продажи



        public static void BuyCompany(Player player)
        {
            if (player.HasData("oilCompany_ID"))
            {
                int autoroom_ID = player.GetData<int>("oilCompany_ID");
                OilCompanyBase company;
                OilCompanyBase.AllOilCompaniesDictionary.TryGetValue(autoroom_ID, out company);
                if (DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney >= company.price)
                {
                    if (string.IsNullOrEmpty(company.ownerSocialName))
                    {
                        MySQLStatic.Query($"UPDATE `oil_companies` SET `ownerSocialName`='{player.SocialClubName}' WHERE `oilCompany_ID`='{autoroom_ID}'");
                        MySQLStatic.Query($"UPDATE `oil_companies` SET `ownerRPName`='{Functions.GetRPName(player)}' WHERE `oilCompany_ID`='{autoroom_ID}'");
                        MySQLStatic.Query($"UPDATE `character_data` SET `biz_ID`='{autoroom_ID}' WHERE `socialClubName`='{player.SocialClubName}'");
                        company.ownerSocialName = player.SocialClubName;
                        company.ownerRPName = Functions.GetRPName(player);
                        DBFunctions.GetPlayerInstanceFromDictionary(player).biz_ID = autoroom_ID;
                        DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney -= company.price;
                        player.SendChatMessage($"{Colors.ORANGE}Поздравляем с покупкой нового бизнеса!");
                        UpdateVisualInfo(autoroom_ID);
                    }
                    else
                    {
                        player.SendChatMessage($"{Colors.ORANGE}Этот бизнес уже куплен!");
                    }
                }
                else
                {
                    player.SendChatMessage($"{Colors.ORANGE}Недостаточно наличных средств.");
                }
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы должны находиться на метке бизнеса.");
            }
        }
        public static void SellCompany(Player player)
        {
            if (player.HasData("oilCompany_ID"))
            {
                int oilCompany_ID = player.GetData<int>("autoroom_ID");
                OilCompanyBase oilCompany;
                OilCompanyBase.AllOilCompaniesDictionary.TryGetValue(oilCompany_ID, out oilCompany);
                if (oilCompany.ownerSocialName == player.SocialClubName)
                {
                    MySQLStatic.Query($"UPDATE `oil_companise` SET `ownerSocialName`= NULL WHERE `oilCompany_ID`='{oilCompany_ID}'");
                    MySQLStatic.Query($"UPDATE `oil_companise` SET `ownerRPName`= NULL WHERE `oilCompany_ID`='{oilCompany_ID}'");
                    MySQLStatic.Query($"UPDATE `character_data` SET `biz_ID`= -1 WHERE `oilCompany_ID`='{player.SocialClubName}'");
                    oilCompany.ownerSocialName = null;
                    oilCompany.ownerRPName = null;
                    DBFunctions.GetPlayerInstanceFromDictionary(player).biz_ID = -1;
                    DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney += (int)(oilCompany.price * 0.8);
                    player.SendChatMessage($"{Colors.ORANGE}Вы продали бизнес.");
                    UpdateVisualInfo(oilCompany_ID);
                }
                else
                {
                    player.SendChatMessage($"{Colors.ORANGE}Вы не являетесь владельцем этого бизнеса.");
                }
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы должны находиться на метке бизнеса.");
            }
        }



        public static void EnterCompanyOffice(Player player)
        {
            if (player.HasData("oilCompany_ID"))
            {
                int oilCompany_ID = player.GetData<int>("oilCompany_ID");
                OilCompanyBase oilCompany;
                OilCompanyBase.AllOilCompaniesDictionary.TryGetValue(oilCompany_ID, out oilCompany);
                // можно будет добавить условие на проверку гос или куплен
                ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                NAPI.Entity.SetEntityPosition(player, officeInteriorsArray[oilCompany.GetInterior(oilCompany.type)]);
                NAPI.Entity.SetEntityDimension(player, (uint)oilCompany_ID);
                player.SetData("INSIDE_OILCOMPANY_ID", oilCompany_ID);
                player.SetSharedData("INSIDE_TYPE", "INSIDE_OILCOMPANY");
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы должны быть на метке бизнеса.");
            }
        }
        public static void ExitCompanyOffice(Player player)
        {
            if (player.HasData("INSIDE_OILCOMPANY_ID"))
            {
                int oilCompany_ID = player.GetData<int>("INSIDE_OILCOMPANY_ID");
                OilCompanyBase oilCompany;
                OilCompanyBase.AllOilCompaniesDictionary.TryGetValue(oilCompany_ID, out oilCompany);
                ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                player.Position = (oilCompany.officePosition + new Vector3(0, 0, 1));
                NAPI.Entity.SetEntityDimension(player, 0);
                player.ResetData("INSIDE_OILCOMPANY_ID");
                player.ResetSharedData("INSIDE_TYPE");
                return;
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы не в бизнесе.");
            }
        }

    }
}
