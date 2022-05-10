using GTANetworkAPI;
using WeiZed.Core;
using WeiZed.MySQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WeiZed.Systems
{
    class Autorooms
    {
        public static Dictionary<int, Autorooms> AllAutoroomsDictionary = new Dictionary<int, Autorooms>();  // Exmpl: <autoroom_ID, autoroomInstance>


        private int autoroom_ID { get; set; }
        private string ownerRPName { get; set; }
        private string ownerSocialName { get; set; }
        private string type { get; set; } = null;
        private Vector3 position { get; set; } = new Vector3();
        private int price { get; set; } = 0;
        private Blip blip;
        public ColShape shape;
        private TextLabel textLabel;
        public static Vector3[] autoroomInteriors =
        {
            new Vector3(261.4586, -998.8196, -99.00863), // low 
            new Vector3(347.2686, -999.2955, -99.19622), // medium 
            new Vector3(-614.86, 40.6783, 97.60007), // premium
            new Vector3(-774.0349, 342.0296, 196.6862), // prestige
        };





        public static void CreateNewAutoroom(Player sender, string type, int price)
        {
            Vector3 autoroomPos = sender.Position;
            Autorooms newautoroom = new Autorooms();

            //Проверка на существование щаписи в базе для присвоения ID дому
            string DBReturn = MySQLStatic.QueryRead($"SELECT `autoroom_ID` FROM `autorooms` WHERE `autoroom_ID`=0");
            bool isTableExist = (DBReturn.Length != 0) ? false : true;
            if (isTableExist)
                newautoroom.autoroom_ID = 1;
            else
                newautoroom.autoroom_ID = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `autoroom_ID` FROM `autorooms` ORDER BY `autoroom_ID` DESC LIMIT 1")) + 1;
            //Проверка на существование щаписи в базе для присвоения ID дому

            newautoroom.type = type;
            newautoroom.position = (autoroomPos - new Vector3(0, 0, 1.33));
            newautoroom.price = price;
            newautoroom.ownerRPName = null;
            newautoroom.ownerSocialName = null;

            #region MySQL Operations
            MySQLStatic.Query($"INSERT INTO `autorooms` (`id`, `autoroom_ID`, `ownerRPName`, `ownerSocialName`, `type`, `position`, `price`)" +
                    $" VALUES(NULL, " +
                    $"'{newautoroom.autoroom_ID}', " +
                    $"'{newautoroom.ownerRPName}', " +
                    $"'{newautoroom.ownerSocialName}'," +
                    $"'{newautoroom.type}'," +
                    $"'{JsonConvert.SerializeObject(newautoroom.position)}', " +
                    $"'{newautoroom.price}')");
            NAPI.Util.ConsoleOutput("Created autoroom record in db");
            #endregion MySQL Operations
            /////////////////////////////////////////////
            int DictKey = newautoroom.autoroom_ID;
            Autorooms.AllAutoroomsDictionary.Add(DictKey, newautoroom);
            /////////////////////////////////////////////
            newautoroom.CreateInfoForNewAutoroom();
        }//Для создания через команду
        public void CreateInfoForNewAutoroom()
        {
            blip = NAPI.Blip.CreateBlip(position);
            textLabel = NAPI.TextLabel.CreateTextLabel($"Автосалон #{autoroom_ID}", position + new Vector3(0, 0, 1.5), 5f, 0.4f, 0, new Color(255, 255, 255), false, 0);
            #region Creating Blip // метка дома
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
            shape = NAPI.ColShape.CreateCylinderColShape(position, 1, 2, 0);
            shape.OnEntityEnterColShape += (colshape, player) =>
            {
                player.SetData("autoroom_ID", autoroom_ID);
                shape.SetSharedData("colshapeType", "AUTOROOM_COLSHAPE");
                shape.SetSharedData("object_ID", autoroom_ID);
            };
            shape.OnEntityExitColShape += (colshape, player) =>
            {
                player.ResetData("autoroom_ID");
                shape.ResetSharedData("colshapeType");
                shape.ResetSharedData("object_ID");
            };
            #endregion

            #region TextLabel
            string text = $"~w~Автосалон ~b~#{autoroom_ID}\n\n~w~Тип: ~b~{type}\n";
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



        public static void LoadAutoroomsFromBD()
        {
            int allAutorooms = 0;
            string result = Convert.ToString(MySQLStatic.QueryRead($"SELECT `autoroom_ID` FROM `autorooms` ORDER BY `autoroom_ID` DESC LIMIT 1"));

            if (result != "")
            {
                allAutorooms = Convert.ToInt32(result);
                NAPI.Util.ConsoleOutput("Last autoroom id in base - " + allAutorooms);
                for (int i = 1; i <= allAutorooms; i++)
                {
                    int DictKey = i;
                    Autorooms.AllAutoroomsDictionary.Add(DictKey, CreateAutoroomFromBD(i));
                }
                NAPI.Util.ConsoleOutput("Autorooms added to garagesDictionary - " + Autorooms.AllAutoroomsDictionary.Count.ToString());
            }
            else
            {
                NAPI.Util.ConsoleOutput("Not found autorooms in base!");
            }

        }//Для создания из бд
        public static Autorooms CreateAutoroomFromBD(int autoroom_ID)//Для создания из бд
        {
            Autorooms autoroom = new Autorooms();
            autoroom.autoroom_ID = autoroom_ID;
            autoroom.ownerRPName = MySQLStatic.QueryRead($"SELECT `ownerRPName` FROM `autorooms` WHERE `autoroom_ID` = {autoroom_ID}");
            autoroom.ownerSocialName = MySQLStatic.QueryRead($"SELECT `ownerSocialName` FROM `autorooms` WHERE `autoroom_ID` = {autoroom_ID}");
            autoroom.type = MySQLStatic.QueryRead($"SELECT `type` FROM `autorooms` WHERE `autoroom_ID` = {autoroom_ID}");
            autoroom.position = JsonConvert.DeserializeObject<Vector3>(MySQLStatic.QueryRead($"SELECT `position` FROM `autorooms` WHERE `autoroom_ID` = {autoroom_ID}"));
            autoroom.price = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `price` FROM `autorooms` WHERE `autoroom_ID` = {autoroom_ID}"));

            autoroom.CreateInfoForDBAutoroom();
            return autoroom;
        }
        public void CreateInfoForDBAutoroom()
        {
            blip = NAPI.Blip.CreateBlip(position);
            textLabel = NAPI.TextLabel.CreateTextLabel($"Автосалон #{autoroom_ID}", position + new Vector3(0, 0, 1.5), 5f, 0.4f, 0, new Color(255, 255, 255), false, 0);
            #region Creating Blip // метка дома
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
            shape = NAPI.ColShape.CreateCylinderColShape(position, 1, 2, 0);
            shape.OnEntityEnterColShape += (colshape, player) =>
            {
                player.SetData("autoroom_ID", autoroom_ID);
                shape.SetSharedData("colshapeType", "AUTOROOM_COLSHAPE");
                shape.SetSharedData("object_ID", autoroom_ID);
            };
            shape.OnEntityExitColShape += (colshape, player) =>
            {
                player.ResetData("autoroom_ID");
                shape.ResetSharedData("colshapeType");
                shape.ResetSharedData("object_ID");
            };
            #endregion

            #region TextLabel
            string text = $"~w~Автосалон ~b~#{autoroom_ID}\n\n~w~Тип: ~b~{type}\n";
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
                case "Low":
                    return 0;
                case "Medium":
                    return 1;
                case "Premium":
                    return 2;
                case "Prestige":
                    return 3;
                default:
                    return -1;
            }
        }
        public uint GetBlipID(string type)
        {
            switch (type)
            {
                case "Low":
                    return 800;
                case "Medium":
                    return 595;
                case "Premium":
                    return 669;
                case "Prestige":
                    return 668;
                default:
                    return 66;
            }
        }
        public static void UpdateVisualInfo(int autoroom_ID)
        {
            Autorooms targetAutoroom;
            Autorooms.AllAutoroomsDictionary.TryGetValue(autoroom_ID, out targetAutoroom);
            #region textLabel
            string text = $"~w~Автосалон ~b~#{autoroom_ID}\n\n~w~Тип: ~b~{targetAutoroom.type}\n";
            if (string.IsNullOrEmpty(targetAutoroom.ownerSocialName))
            {
                text += $"~w~Цена: ~b~{targetAutoroom.price}$\n";
            }
            else
            {
                text += $"~w~Владелец: ~b~{targetAutoroom.ownerRPName}\n";
            }

            targetAutoroom.textLabel.Text = text;
            #endregion textLabel

            #region Blip
            if ((MySQLStatic.QueryRead($"SELECT `ownerSocialName` FROM `autorooms` WHERE `autoroom_ID` = {targetAutoroom.autoroom_ID}")).Length == 0)
            {
                targetAutoroom.blip.Sprite = targetAutoroom.GetBlipID(targetAutoroom.type);
                targetAutoroom.blip.Color = 2;
            }
            else
            {
                targetAutoroom.blip.Sprite = targetAutoroom.GetBlipID(targetAutoroom.type);
                targetAutoroom.blip.Color = 49;
            }

            targetAutoroom.blip.Scale = 1f;
            targetAutoroom.blip.ShortRange = true;
            #endregion Blip
        }//Для отрисовки после покупки/продажи



        public static void BuyAutoroom(Player player)
        {
            if (player.HasData("autoroom_ID"))
            {
                int autoroom_ID = player.GetData<int>("autoroom_ID");
                Autorooms autoroom;
                Autorooms.AllAutoroomsDictionary.TryGetValue(autoroom_ID, out autoroom);
                if (DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney >= autoroom.price)
                {
                    if (string.IsNullOrEmpty(autoroom.ownerSocialName))
                    {
                        MySQLStatic.Query($"UPDATE `autorooms` SET `ownerSocialName`='{player.SocialClubName}' WHERE `autoroom_ID`='{autoroom_ID}'");
                        MySQLStatic.Query($"UPDATE `autorooms` SET `ownerRPName`='{Functions.GetRPName(player)}' WHERE `autoroom_ID`='{autoroom_ID}'");
                        MySQLStatic.Query($"UPDATE `character_data` SET `biz_ID`='{autoroom_ID}' WHERE `socialClubName`='{player.SocialClubName}'");
                        autoroom.ownerSocialName = player.SocialClubName;
                        autoroom.ownerRPName = Functions.GetRPName(player);
                        DBFunctions.GetPlayerInstanceFromDictionary(player).biz_ID = autoroom_ID;
                        DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney -= autoroom.price;
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
        public static void SellAutoroom(Player player)
        {
            if (player.HasData("autoroom_ID"))
            {
                int autoroom_ID = player.GetData<int>("autoroom_ID");
                Autorooms autoroom;
                Autorooms.AllAutoroomsDictionary.TryGetValue(autoroom_ID, out autoroom);
                if (autoroom.ownerSocialName == player.SocialClubName)
                {
                    MySQLStatic.Query($"UPDATE `autorooms` SET `ownerSocialName`= NULL WHERE `autoroom_ID`='{autoroom_ID}'");
                    MySQLStatic.Query($"UPDATE `autorooms` SET `ownerRPName`= NULL WHERE `autoroom_ID`='{autoroom_ID}'");
                    MySQLStatic.Query($"UPDATE `character_data` SET `biz_ID`= -1 WHERE `socialClubName`='{player.SocialClubName}'");
                    autoroom.ownerSocialName = null;
                    autoroom.ownerRPName = null;
                    DBFunctions.GetPlayerInstanceFromDictionary(player).biz_ID = -1;
                    DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney += (int)(autoroom.price * 0.8);
                    player.SendChatMessage($"{Colors.ORANGE}Вы продали бизнес.");
                    UpdateVisualInfo(autoroom_ID);
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



        public static void EnterAutoroom(Player player)
        {
            if (player.HasData("autoroom_ID"))
            {
                int autoroom_ID = player.GetData<int>("autoroom_ID");
                Autorooms autoroom;
                Autorooms.AllAutoroomsDictionary.TryGetValue(autoroom_ID, out autoroom);
                // можно будет добавить условие на проверку гос или куплен
                ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                NAPI.Entity.SetEntityPosition(player, autoroomInteriors[autoroom.GetInterior(autoroom.type)]);
                NAPI.Entity.SetEntityDimension(player, (uint)autoroom_ID);
                player.SetData("INSIDE_AUTOROOM_ID", autoroom_ID);
                player.SetSharedData("INSIDE_TYPE", "INSIDE_AUTOROOM");
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы должны быть на метке бизнеса.");
            }
        }
        public static void ExitAutoroom(Player player)
        {
            if (player.HasData("INSIDE_AUTOROOM_ID"))
            {
                int autoroom_ID = player.GetData<int>("INSIDE_AUTOROOM_ID");
                Autorooms autoroom;
                Autorooms.AllAutoroomsDictionary.TryGetValue(autoroom_ID, out autoroom);
                ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                player.Position = (autoroom.position + new Vector3(0, 0, 1));
                NAPI.Entity.SetEntityDimension(player, 0);
                player.ResetData("INSIDE_AUTOROOM_ID");
                player.ResetSharedData("INSIDE_TYPE");
                return;
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы не в бизнесе.");
            }
        }



        public static void BuyCar(Player player, string carModel)
        {
            if (player.HasData("INSIDE_AUTOROOM_ID"))
            {
                int autoroom_ID = player.GetData<int>("INSIDE_AUTOROOM_ID");
                Autorooms autoroom;
                Autorooms.AllAutoroomsDictionary.TryGetValue(autoroom_ID, out autoroom);
                CharacterData data;
                PlayerAPI.AllPlayersDictonary.TryGetValue(player.SocialClubName, out data);
                ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                NAPI.Entity.SetEntityDimension(player, 0);
                VehicleSystem.CreatePesonalVehicle(player, carModel, 1, 1, autoroom.position + new Vector3(0, 0, 1), new Vector3(0, 0, 0), data.SocialClubName);
                player.ResetData("INSIDE_AUTOROOM_ID");
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
