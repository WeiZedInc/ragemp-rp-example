using GTANetworkAPI;
using WeiZed.MySQL;
using WeiZed.Systems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WeiZed.Core
{
    class HouseSystem
    {
        public static Dictionary<int, HouseSystem> AllHousesDictionary = new Dictionary<int, HouseSystem>();  // Exmpl: <house_ID, houseInstance>

        public int house_ID { get; set; }
        public string ownerRPName { get; set; }
        public string ownerSocialName { get; set; }
        public string type { get; set; } = null;
        public Vector3 position { get; set; } = new Vector3();
        public int price { get; set; } = 0;
        public bool isLocked { get; set; } = false;
        public int houseGarage_ID { get; set; }
        public List<string> roommates = new List<string>();
        public TextLabel textLabel;
        public Marker marker;
        public ColShape shape;
        public static Vector3[] houseInteriors =
        {
            new Vector3(261.4586, -998.8196, -99.00863), // low end apart
            new Vector3(347.2686, -999.2955, -99.19622), // medium end
            new Vector3(-614.86, 40.6783, 97.60007), // rich end
            new Vector3(-774.0349, 342.0296, 196.6862), // Mansion (Aqua 3 Apartment	apa_v_mp_h_08_b	new mp.Vector3(-774.0349, 342.0296, 196.6862);)
            new Vector3(152.2605, -1004.471, -98.99999), // Motel
            new Vector3(-1908.024, -573.4244, 19.09722) // House on the beach
        };
        public static bool isHousesExists = false;




        public static void CreateNewHouse(Player sender, string type, int price, bool withGarage)
        {
            Vector3 housePos = sender.Position;
            HouseSystem house = new HouseSystem();

            //Проверка на существование щаписи в базе для присвоения ID дому
            string DBReturn = MySQLStatic.QueryRead($"SELECT `house_ID` FROM `houses` WHERE `house_ID`=0");
            bool isTableExist = (DBReturn.Length != 0) ? false : true;
            if (isTableExist)
                house.house_ID = 1;
            else
                house.house_ID = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `house_ID` FROM `houses` ORDER BY `house_ID` DESC LIMIT 1")) + 1;
            //Проверка на существование щаписи в базе для присвоения ID дому

            house.type = type;
            house.position = (housePos - new Vector3(0, 0, 1.33));
            house.price = price;
            house.ownerRPName = null;
            house.ownerSocialName = null;
            if (!withGarage) // если нет гаража то ставим сайз -1,который будет как Нет мест
            {
                house.houseGarage_ID = -1;
            }
            else
            {
                house.houseGarage_ID = house.house_ID;
            }


            #region MySQL Operations
            MySQLStatic.Query($"INSERT INTO `houses` (`id`, `house_ID`, `type`, `position`, `price`, `isLocked`, `houseGarage_ID`)" +
                    $" VALUES(NULL, " +
                    $"'{ house.house_ID}', " +
                    $"'{house.type}', " +
                    $"'{JsonConvert.SerializeObject(house.position)}', " +
                    $"'{house.price}'," +
                    $"'{house.isLocked}'," +
                    $"'{house.houseGarage_ID}')");
            NAPI.Util.ConsoleOutput("Created house record in db");
            #endregion MySQL Operations

            house.CreateInfoForNewHouse();
            /////////////////////////////////////////////
            int DictKey = house.house_ID;
            HouseSystem.AllHousesDictionary.Add(DictKey, house);
            /////////////////////////////////////////////
        }//Для создания через команду
        public void CreateInfoForNewHouse()
        {
            GarageSystem garage;
            GarageSystem.AllGaragesDictionary.TryGetValue(house_ID, out garage);
            //blip = NAPI.Blip.CreateBlip(position);
            textLabel = NAPI.TextLabel.CreateTextLabel($"Дом #{house_ID}", position + new Vector3(0, 0, 1.5), 5f, 0.4f, 0, new Color(255, 255, 255), false, 0);
            #region Creating Blip // метка дома
            //if (string.IsNullOrEmpty(ownerSocialName))
            //{
            //    blip.Sprite = 40;
            //    blip.Color = 2;
            //}
            //else
            //{
            //    blip.Sprite = 40;
            //    blip.Color = 49;
            //}

            //blip.Scale = 1f;
            //blip.ShortRange = true;
            #endregion

            #region Creating Marker & Colshape
            ColShape shape = NAPI.ColShape.CreateCylinderColShape(position, 1, 2, 0);
            shape.OnEntityEnterColShape += (colshape, player) =>
            {
                player.SetData("house_ID", house_ID);
                shape.SetSharedData("colshapeType", "HOUSE_COLSHAPE");
                shape.SetSharedData("object_ID", house_ID);
            };
            shape.OnEntityExitColShape += (colshape, player) =>
            {
                NAPI.Data.ResetEntityData(player, "house_ID");
                shape.ResetSharedData("colshapeType");
                shape.ResetSharedData("object_ID");
            };
            Color color = new Color(245, 245, 220);
            marker = NAPI.Marker.CreateMarker(29, position + new Vector3(0, 0, 1.7), new Vector3(), new Vector3(), 0.6f, color);
            #endregion

            #region TextLabel
            string text = $"~w~Дом ~b~#{house_ID}\n\n~w~Тип: ~b~{type}\n";
            if (string.IsNullOrEmpty(ownerSocialName))
            {
                text += $"~w~Цена: ~b~{price}$\n";
            }
            else
            {
                text += $"~w~Владелец: ~b~{ownerRPName}\n";
            }

            if (houseGarage_ID != -1)
            {
                text += $"~w~Гаражных мест: ~b~{garage.garageSize}\n";
            }
            else
            {
                text += $"~w~Гаражных мест: ~b~Нет\n";
            }

            if (isLocked == true)
            {
                text += $"~r~Закрыт\n";
            }
            else
            {
                text += $"~g~Открыт\n";
            }
            textLabel.Text = text;
            #endregion TextLabel
        }//Для создания через команду


        public static void UpdateVisualInfo(int house_ID)
        {
            HouseSystem targetHouse;
            HouseSystem.AllHousesDictionary.TryGetValue(house_ID, out targetHouse);
            GarageSystem garage;
            GarageSystem.AllGaragesDictionary.TryGetValue(house_ID, out garage);
            #region textLabel
            string text = $"~w~Дом ~b~#{house_ID}\n\n~w~Тип: ~b~{targetHouse.type}\n";
            if (string.IsNullOrEmpty(targetHouse.ownerSocialName))
            {
                text += $"~w~Цена: ~b~{targetHouse.price}$\n";
            }
            else
            {
                text += $"~w~Владелец: ~b~{targetHouse.ownerRPName}\n";
            }

            if (targetHouse.houseGarage_ID != -1)
            {
                text += $"~w~Гаражных мест: ~b~{garage.garageSize}\n";
            }
            else
            {
                text += $"~w~Гаражных мест: ~b~Нет\n";
            }

            if (targetHouse.isLocked == true)
            {
                text += $"~r~Закрыт\n";
            }
            else
            {
                text += $"~g~Открыт\n";
            }
            targetHouse.textLabel.Text = text;
            #endregion textLabel

            #region Blip + marker
            if ((MySQLStatic.QueryRead($"SELECT `ownerSocialName` FROM `houses` WHERE `house_ID` = {house_ID}")).Length == 0)
            {
                //targetHouse.blip.Sprite = 40;
                //targetHouse.blip.Color = 2;
                Color color = new Color(245, 245, 220);
                targetHouse.marker = NAPI.Marker.CreateMarker(29, targetHouse.position + new Vector3(0, 0, 1.7), new Vector3(), new Vector3(), 0.6f, color);
            }
            else
            {
                //targetHouse.blip.Sprite = 40;
                //targetHouse.blip.Color = 49;
                targetHouse.marker.Delete();
            }

            //targetHouse.blip.Scale = 1f;
            //targetHouse.blip.ShortRange = true;
            #endregion Blip + marker
        }//Для отрисовки после покупки/продажи


        public static void LoadHousesFromDB()
        {
            int allHousesID = 0;
            string result = Convert.ToString(MySQLStatic.QueryRead($"SELECT `house_ID` FROM `houses` ORDER BY `house_ID` DESC LIMIT 1"));

            if (result != "")
            {
                isHousesExists = true;
                allHousesID = Convert.ToInt32(result);
                NAPI.Util.ConsoleOutput("Last house id in base - " + allHousesID);
                for (int i = 1; i <= allHousesID; i++)
                {
                    HouseSystem.AllHousesDictionary.Add(i, CreateHouseFromDB(i));
                }
                NAPI.Util.ConsoleOutput("Houses added to housesDictionary - " + HouseSystem.AllHousesDictionary.Count.ToString());
            }
            else
            {
                NAPI.Util.ConsoleOutput("Not found houses in base!");
            }
        }//Для создания из бд
        public static HouseSystem CreateHouseFromDB(int house_ID)//Для создания из бд
        {
            HouseSystem house = new HouseSystem();
            house.house_ID = house_ID;
            house.ownerRPName = MySQLStatic.QueryRead($"SELECT `ownerRPName` FROM `houses` WHERE `house_ID` = {house_ID}");
            house.ownerSocialName = MySQLStatic.QueryRead($"SELECT `ownerSocialName` FROM `houses` WHERE `house_ID` = {house_ID}");
            house.type = MySQLStatic.QueryRead($"SELECT `type` FROM `houses` WHERE `house_ID` = {house_ID}");
            house.isLocked = Convert.ToBoolean(MySQLStatic.QueryRead($"SELECT `isLocked` FROM `houses` WHERE `house_ID` = {house_ID}"));
            house.position = JsonConvert.DeserializeObject<Vector3>(MySQLStatic.QueryRead($"SELECT `position` FROM `houses` WHERE `house_ID` = {house_ID}"));
            house.price = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `price` FROM `houses` WHERE `house_ID` = {house_ID}"));
            house.houseGarage_ID = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `houseGarage_ID` FROM `houses` WHERE `house_ID` = {house_ID}"));

            #region Руммейты из бдшки
            string roommatesFromDB = MySQLStatic.QueryRead($"SELECT `roommates` FROM `houses` WHERE `house_ID` = {house_ID}");
            if (roommatesFromDB != "")
            {
                if (roommatesFromDB.Contains(", ")) // если несколько 
                {
                    string[] roommatesArr = roommatesFromDB.Split(", ");
                    foreach (var roommate in roommatesArr)
                    {
                        house.roommates.Add(roommate);
                    }
                }
                else // если один
                {
                    house.roommates.Add(roommatesFromDB);
                }
            }
            #endregion

            house.CreateInfoForDBHouse();
            return house;
        }
        public void CreateInfoForDBHouse()
        {
            GarageSystem garage;
            GarageSystem.AllGaragesDictionary.TryGetValue(house_ID, out garage);
            //blip = NAPI.Blip.CreateBlip(position);
            textLabel = NAPI.TextLabel.CreateTextLabel($"Дом #{house_ID}", position + new Vector3(0, 0, 1.5), 5f, 0.4f, 0, new Color(255, 255, 255), false, 0);
            #region Creating Blip + marker // метка дома
            if (ownerSocialName.Length == 0)
            {
                //blip.Sprite = 40;
                //blip.Color = 2;
                Color color = new Color(245, 245, 220);
                marker = NAPI.Marker.CreateMarker(29, position + new Vector3(0, 0, 1.7), new Vector3(), new Vector3(), 0.6f, color);
            }
            else
            {
                //blip.Sprite = 40;
                //blip.Color = 49;
            }

            //blip.Scale = 1f;
            //blip.ShortRange = true;
            #endregion

            #region Creating Colshape
            shape = NAPI.ColShape.CreateCylinderColShape(position, 1, 2, 0);
            shape.OnEntityEnterColShape += (colshape, player) =>
            {
                player.SetData("house_ID", house_ID);
                shape.SetSharedData("colshapeType", "HOUSE_COLSHAPE");
                shape.SetSharedData("object_ID", house_ID);
            };
            shape.OnEntityExitColShape += (colshape, player) =>
            {
                NAPI.Data.ResetEntityData(player, "house_ID");
                shape.ResetSharedData("colshapeType");
                shape.ResetSharedData("object_ID");
            };
            #endregion

            #region TextLabel
            string text = $"~w~Дом ~b~#{house_ID}\n\n~w~Тип: ~b~{type}\n";
            if (string.IsNullOrEmpty(ownerSocialName))
            {
                text += $"~w~Цена: ~b~{price}$\n";
            }
            else
            {
                text += $"~w~Владелец: ~b~{ownerRPName}\n";
            }

            if (houseGarage_ID == -1)
            {
                text += $"~w~Гаражных мест: ~b~Нет\n";
            }
            else
            {
                text += $"~w~Гаражных мест: ~b~{garage.garageSize}\n";
            }

            if (isLocked == true)
            {
                text += $"~r~Закрыт\n";
            }
            else
            {
                text += $"~g~Открыт\n";
            }
            textLabel.Text = text;
            #endregion TextLabel
        }//Для создания из бд (разница в услоовиях,ебаные isnull и length = 0)
        public static void CheckIfRoommate(Player player) // если чел есть в чъем-то руммейтлисте,то присвою данные
        {
            if (isHousesExists)
            {
                int allHousesID = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `house_ID` FROM `houses` ORDER BY `house_ID` DESC LIMIT 1"));
                for (int i = 1; i <= allHousesID; i++)
                {
                    HouseSystem house;
                    HouseSystem.AllHousesDictionary.TryGetValue(i, out house);
                    if (house.roommates.Contains(Functions.GetRPName(player)))
                    {
                        player.SetData("ROOMMATE_houseID", house.house_ID);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }



        public static void LoadIPL()
        {
            NAPI.World.RequestIpl("apa_v_mp_h_08_b");
            NAPI.World.RequestIpl("imp_dt1_11_cargarage_a");
        }
        public int GetInterior(string type)
        {
            switch (type)
            {
                case "Low-End":
                    return 0;
                case "Medium-End":
                    return 1;
                case "Rich-End":
                    return 2;
                case "Mansion":
                    return 3;
                case "Hotel":
                    return 4;
                case "Seaview Apartments":
                    return 5;
                default:
                    return -1;
            }
        }



        public static void BuyHouse(Player player)
        {
            if (player.HasData("house_ID"))
            {
                int house_ID = player.GetData<int>("house_ID");
                HouseSystem house;
                HouseSystem.AllHousesDictionary.TryGetValue(house_ID, out house);
                if (DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney >= house.price)
                {
                    if (string.IsNullOrEmpty(house.ownerSocialName))
                    {
                        MySQLStatic.Query($"UPDATE `houses` SET `ownerSocialName`='{player.SocialClubName}' WHERE `house_ID`='{house_ID}'");
                        MySQLStatic.Query($"UPDATE `houses` SET `ownerRPName`='{Functions.GetRPName(player)}' WHERE `house_ID`='{house_ID}'");
                        MySQLStatic.Query($"UPDATE `character_data` SET `ownedHouse_ID`='{house_ID}' WHERE `socialClubName`='{player.SocialClubName}'");
                        house.ownerSocialName = player.SocialClubName;
                        house.ownerRPName = Functions.GetRPName(player);
                        DBFunctions.GetPlayerInstanceFromDictionary(player).ownedHouse_ID = house_ID;
                        DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney -= house.price;
                        DBFunctions.GetPlayerInstanceFromDictionary(player).ownedHouse_ID = house_ID;
                        player.SendChatMessage($"{Colors.ORANGE}Поздравляем с покупкой нового жилья!");
                        house.isLocked = true;
                        UpdateVisualInfo(house_ID);
                    }
                    else
                    {
                        player.SendChatMessage($"{Colors.ORANGE}Этот дом уже куплен!");
                    }
                }
                else
                {
                    player.SendChatMessage($"{Colors.ORANGE}Недостаточно наличных средств.");
                }
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы должны находиться на метке дома.");
            }
        }
        public static void SellHouse(Player player)
        {
            if (player.HasData("house_ID"))
            {
                int house_ID = player.GetData<int>("house_ID");
                HouseSystem house;
                HouseSystem.AllHousesDictionary.TryGetValue(house_ID, out house);
                if (house.ownerSocialName == player.SocialClubName)
                {
                    MySQLStatic.Query($"UPDATE `houses` SET `ownerSocialName`= NULL WHERE `house_ID`='{house_ID}'");
                    MySQLStatic.Query($"UPDATE `houses` SET `ownerRPName`= NULL WHERE `house_ID`='{house_ID}'");
                    MySQLStatic.Query($"UPDATE `character_data` SET `ownedHouse_ID`= -1 WHERE `socialClubName`='{player.SocialClubName}'");
                    house.ownerSocialName = null;
                    house.ownerRPName = null;
                    DBFunctions.GetPlayerInstanceFromDictionary(player).ownedHouse_ID = -1;
                    DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney += (int)(house.price * 0.8);
                    player.SendChatMessage($"{Colors.ORANGE}Вы продали дом.");
                    house.isLocked = false;
                    UpdateVisualInfo(house_ID);
                }
                else
                {
                    player.SendChatMessage($"{Colors.ORANGE}Вы не являетесь владельцем этого дома.");
                }
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы должны находиться на метке дома.");
            }
        }


        public static void AddRoommate(Player owner, ushort id)
        {
            int house_ID = DBFunctions.GetPlayerInstanceFromDictionary(owner).ownedHouse_ID;
            HouseSystem house;
            HouseSystem.AllHousesDictionary.TryGetValue(house_ID, out house);
            if (house_ID != -1)
            {
                if (house.roommates.Count <= 2)
                {
                    if (Functions.GetPlayerByID(id) != null)
                    {
                        Player roommate = Functions.GetPlayerByID(id);
                        roommate.SetData("ROOMMATE_houseID", house_ID);
                        house.roommates.Add(Functions.GetRPName(roommate));
                        owner.SendChatMessage($"{Colors.LIME}Вы добавили жильца по имени {Functions.GetRPName(roommate)}.");
                        roommate.SendChatMessage($"{Colors.LIME}{Functions.GetRPName(owner)} подселил вас к себе.");

                        #region Запись руммейтов в базу
                        string roommatesFromDB = MySQLStatic.QueryRead($"SELECT `roommates` FROM `houses` WHERE `house_ID`='{house_ID}'");
                        if (roommatesFromDB != "") // если roommatesFromDB не пустой то прибавляем к имеющемуся
                        {
                            string newNumberPlatesToBD = roommatesFromDB + ", " + Functions.GetRPName(roommate);
                            MySQLStatic.Query($"UPDATE `houses` SET `roommates`= '{newNumberPlatesToBD}' WHERE `house_ID`='{house_ID}'");
                        }
                        else // если пустой
                        {
                            MySQLStatic.Query($"UPDATE `houses` SET `roommates`= '{Functions.GetRPName(roommate)}' WHERE `house_ID`='{house_ID}'");
                        }
                        #endregion
                    }
                    else
                    {
                        owner.SendChatMessage($"{Colors.ORANGE}Игрок с таким ID не найден.");
                    }
                }
                else
                {
                    owner.SendChatMessage($"{Colors.ORANGE}У вас максимально количество жильцов.");
                }
            }
            else
            {
                owner.SendChatMessage($"{Colors.ORANGE}У вас нет жилья,на улицу не подселишь...");
            }
        }
        public static void RemoveRoommate(Player owner, ushort id)
        {
            int house_ID = DBFunctions.GetPlayerInstanceFromDictionary(owner).ownedHouse_ID;
            HouseSystem house;
            HouseSystem.AllHousesDictionary.TryGetValue(house_ID, out house);
            if (house_ID != -1)
            {
                if (house.roommates.Count == 1)
                {

                    owner.SendChatMessage($"{Colors.LIME}Вы выгнали жильца по имени {house.roommates[0]}.");
                    #region Убираю челикуса с базы илиста
                    Player target = Functions.GetPlayerByID(id);
                    target.ResetData("ROOMMATE_houseID");
                    string roommatesFromDB = MySQLStatic.QueryRead($"SELECT `roommates` FROM `houses` WHERE `house_ID`='{house_ID}'");
                    if (roommatesFromDB.Contains(", " + Functions.GetRPName(target)))
                    {
                        int indexOfRoommate = house.roommates.IndexOf(Functions.GetRPName(target));
                        house.roommates.RemoveAt(indexOfRoommate);

                        string newRoommates = roommatesFromDB.Replace(", " + Functions.GetRPName(target), "");
                        MySQLStatic.Query($"UPDATE `houses` SET `roommates`= '{newRoommates}' WHERE `house_ID`='{house_ID}'");
                        return;
                    }
                    else if (roommatesFromDB.Contains(Functions.GetRPName(target) + ", "))
                    {
                        int indexOfRoommate = house.roommates.IndexOf(Functions.GetRPName(target));
                        house.roommates.RemoveAt(indexOfRoommate);

                        string newRoommates = roommatesFromDB.Replace(Functions.GetRPName(target) + ", ", "");
                        MySQLStatic.Query($"UPDATE `houses` SET `roommates`= '{newRoommates}' WHERE `house_ID`='{house_ID}'");
                        return;
                    }
                    else
                    {
                        MySQLStatic.Query($"UPDATE `houses` SET `roommates`=NULL WHERE `house_ID`='{house_ID}'");
                        house.roommates.RemoveAt(0);
                        return;
                    }
                    #endregion
                }
                else
                {
                    owner.SendChatMessage($"{Colors.ORANGE}У вас вас нет жильцов... *хнык*");
                }
            }
            else
            {
                owner.SendChatMessage($"{Colors.ORANGE}У вас нет жилья,с улицы не прогонишь...");
            }
        }



        public static void OpenHouse(Player player)
        {
            if (player.HasData("house_ID"))
            {
                int house_ID = player.GetData<int>("house_ID");
                CharacterData playerData = DBFunctions.GetPlayerInstanceFromDictionary(player);
                HouseSystem house;
                HouseSystem.AllHousesDictionary.TryGetValue(house_ID, out house);
                if ((playerData.ownedHouse_ID == house_ID) || (player.GetData<int>("ROOMMATE_houseID") == house_ID))
                {
                    if (house.isLocked)
                    {
                        house.isLocked = false;
                        player.SendChatMessage($"{Colors.LIMEGREEN}Вы открыли дом.");
                        MySQLStatic.Query($"UPDATE `houses` SET `isLocked`= 0 WHERE `house_ID`='{house_ID}'");
                        UpdateVisualInfo(house_ID);
                    }
                    else
                    {
                        house.isLocked = true;
                        player.SendChatMessage($"{Colors.CRIMSON_RED}Вы закрыли дом.");
                        MySQLStatic.Query($"UPDATE `houses` SET `isLocked`= 1 WHERE `house_ID`='{house_ID}'");
                        UpdateVisualInfo(house_ID);
                    }
                }
                else
                {
                    player.SendChatMessage($"{Colors.ORANGE}У вас нет доступа к этому жилью.");
                }
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы должны быть на метке дома.");
            }
        }
        public static void EnterHouse(Player player)
        {
            if (player.HasData("house_ID"))
            {
                HouseSystem house;
                HouseSystem.AllHousesDictionary.TryGetValue(player.GetData<int>("house_ID"), out house);
                CharacterData playerData = DBFunctions.GetPlayerInstanceFromDictionary(player);
                if (player.IsInVehicle)
                {
                    player.SendChatMessage($"{Colors.ORANGE}Покиньте транспорт.");
                    return;
                }
                else
                {
                    if ((playerData.ownedHouse_ID == house.house_ID) || (player.GetData<int>("ROOMMATE_houseID") == house.house_ID))
                    {
                        ClientSideFunctions.ScreenFadeEffect(player, 0, 550);
                        NAPI.Entity.SetEntityPosition(player, houseInteriors[house.GetInterior(house.type)]);
                        NAPI.Entity.SetEntityDimension(player, (uint)house.house_ID);
                        player.SetSharedData("INSIDE_HOUSE_ID", house.house_ID);
                        player.SetSharedData("INSIDE_TYPE", "INSIDE_HOUSE");
                        return;
                    }
                    if (playerData.ownedHouse_ID != house.house_ID && house.isLocked == true)
                    {
                        player.SendChatMessage($"{Colors.RED}Дом закрыт.");
                        return;
                    }
                    else
                    {
                        ClientSideFunctions.ScreenFadeEffect(player, 0, 550);
                        NAPI.Entity.SetEntityPosition(player, houseInteriors[house.GetInterior(house.type)]);
                        NAPI.Entity.SetEntityDimension(player, (uint)house.house_ID);
                        player.SetSharedData("INSIDE_HOUSE_ID", house.house_ID);
                        player.SetSharedData("INSIDE_TYPE", "INSIDE_HOUSE");
                        return;
                    }
                }
            }
        }
        public static void ExitHouse(Player player)
        {
            if (player.HasSharedData("INSIDE_HOUSE_ID"))
            {
                int house_ID = player.GetSharedData<int>("INSIDE_HOUSE_ID");
                CharacterData playerData = DBFunctions.GetPlayerInstanceFromDictionary(player);
                HouseSystem house;
                HouseSystem.AllHousesDictionary.TryGetValue(house_ID, out house);
                ClientSideFunctions.ScreenFadeEffect(player, 0, 550);
                NAPI.Entity.SetEntityPosition(player, (house.position + new Vector3(0, 0, 1)));
                NAPI.Entity.SetEntityDimension(player, 0);
                player.ResetSharedData("INSIDE_HOUSE_ID");
                player.ResetSharedData("INSIDE_TYPE");
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы не в доме.");
            }
        }
    }
}
