using GTANetworkAPI;
using WeiZed.MySQL;
using WeiZed.Systems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace WeiZed.Core
{
    class PlayerAPI : Script
    {
        public static Dictionary<string, CharacterData> AllPlayersDictonary = new Dictionary<string, CharacterData>();  //  <player.SocialClubName, SocialClubName>
        public void ServerSystemsInit(Player player)
        {
            HouseSystem.LoadIPL();
            HouseSystem.CheckIfRoommate(player); 
            InventorySystem.CheckForExist(player.SocialClubName);
        }

        [ServerEvent(Event.PlayerConnected)] // Иполняеться при входе игрока
        public void OnPlayerConnected(Player player)
        {
            #region Создание/проверка базы зашедшего игрока
            CharacterData newCharacter = new CharacterData(); // Создаем объект игрока при его входе на сервер

            NAPI.Util.ConsoleOutput("Проверка на наличие записи в таблице...");
            newCharacter.SocialClubName = NAPI.Player.GetPlayerSocialClubName(player); // Присваеваем имя в объкт игрока

            #region Работа с базой
            string playerDBReturn = MySQLStatic.QueryRead($"SELECT `socialClubName` FROM `character_data` WHERE `socialClubName`='{player.SocialClubName}'");
            bool isPlayerTableExist = (playerDBReturn.Length == 0) ? false : true; // Проверка на существование таблы с сошлом,если есть то тру,нету фолс
            if (isPlayerTableExist)
            {
                NAPI.Util.ConsoleOutput("Запись найдена,присваивание данных с таблицы объекту игрока");
                // Обновляю класс игрока с базы
                try
                {
                    DBFunctions.GetCharacterDataFromDB(newCharacter, player);
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput($"Ошибка при присваивании данных с базы в класс игрока: {e}");
                }
            }
            else
            {
                NAPI.Util.ConsoleOutput("Записи нет,создаем!");

                DBFunctions.CreateNewCharacterRecordInDB(newCharacter);

                NAPI.Util.ConsoleOutput("Проверка cозданной таблицы...");
                string playerDBReturnNew = MySQLStatic.QueryRead($"SELECT `socialClubName` FROM `character_data` WHERE `socialClubName`='{player.SocialClubName}'");
                bool isPlayerTableCreated = (playerDBReturnNew.Length == 0) ? false : true; // Проверка на существование новой таблы с сошлом,если есть то тру,нету фолс
                if (isPlayerTableCreated)
                {
                    NAPI.Util.ConsoleOutput("Запись создана,инициализация данных...");
                }
                else
                {
                    NAPI.Util.ConsoleOutput("Ошибка,не удалось создать запись");
                }
            }
            #endregion
            #endregion Создание/проверка базы зашедшего игрока

            #region Запись игрока в лист
            if (!PlayerAPI.AllPlayersDictonary.ContainsKey(player.SocialClubName))
            {
                PlayerAPI.AllPlayersDictonary.Add(player.SocialClubName, newCharacter);
            }
            #endregion

            #region Спавн игрока
            NAPI.Player.SpawnPlayer(player, newCharacter.spawnPosition); // Спавним перса по его коордам с базы
            player.Rotation = newCharacter.spawnRotation;
            player.SetSharedData("ADMIN_LVL", newCharacter.adminLVL);
            #endregion Спавн игрока

            #region Инициализация персональных машин

            DataTable dataTable = MySQLStatic.QueryReadTable($"SELECT * FROM `personal_vehicles` WHERE `ownerSocialName`='{player.SocialClubName}'");
            if (dataTable != null && dataTable.Rows.Count != 0)
            {
                NAPI.Util.ConsoleOutput("Запись машины игрока найдена,присваивание данных с таблицы объекту машины");
                // Добавляю машину в лист с базы
                try
                {
                    NAPI.Util.ConsoleOutput("Найдено машин " + dataTable.Rows.Count.ToString());
                    foreach (DataRow Row in dataTable.Rows)
                    {
                        int garage_ID = Convert.ToInt32(Row["garage_ID"]);
                        string numberPlate, carModelName, ownerSocialName, tune, itemsInside;
                        int color1, color2, fuel, price;
                        Vector3 position, rotation;
                        if (garage_ID == 0) // если машина не в гараже спавним по кордам
                        {
                            numberPlate = Convert.ToString(Row["numberPlate"]);
                            carModelName = Convert.ToString(Row["carModelName"]);
                            color1 = Convert.ToInt32(Row["color1"]);
                            color2 = Convert.ToInt32(Row["color2"]);
                            ownerSocialName = Convert.ToString(Row["ownerSocialName"]);
                            fuel = Convert.ToInt32(Row["fuel"]);
                            price = Convert.ToInt32(Row["price"]);
                            tune = JsonConvert.DeserializeObject<string>(Row["tune"].ToString());
                            itemsInside = JsonConvert.DeserializeObject<string>(Row["itemsInside"].ToString());
                            position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                            rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                            VehicleSystem.PersonalVehiclesDictionary.Add(numberPlate, VehicleSystem.CreatePesonalVehicleFromBD(numberPlate, carModelName, color1, color2, ownerSocialName, fuel, price, tune, itemsInside, position, rotation));
                        }
                        else // если машина в гараже не спавним,гараж будет спавнить при входе
                        {
                            continue;
                        }
                    }
                    NAPI.Util.ConsoleOutput("Запись машин в лист гаража завершено");
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput($"Ошибка при присваивании данных с базы в класс машины: {e}");
                }
            }
            else
            {
                NAPI.Util.ConsoleOutput("Записи персональной машины нет!");
            }

            #endregion

            ServerSystemsInit(player);
        }


        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType disconnectionType, string reason)
        {
            switch (disconnectionType)
            {
                case DisconnectionType.Left:
                    DBFunctions.SaveCharacterDataOnExit(player);
                    VehicleSystem.SaveVehicleToBD(player);
                    NAPI.Util.ConsoleOutput($"Игрок {player.SocialClubName} покинул сервер,база обновлена");
                    break;

                //   /q this case
                case DisconnectionType.Timeout:
                    DBFunctions.SaveCharacterDataOnExit(player);
                    VehicleSystem.SaveVehicleToBD(player);
                    NAPI.Util.ConsoleOutput($"Игрок {player.SocialClubName} вылетел с сервера,база обновлена");
                    break;


                case DisconnectionType.Kicked:
                    DBFunctions.SaveCharacterDataOnExit(player);
                    VehicleSystem.SaveVehicleToBD(player);
                    PlayerAPI.AllPlayersDictonary.Remove(player.SocialClubName);
                    NAPI.Util.ConsoleOutput($"Игрок {player.SocialClubName} кикнут с сервера,база обновлена");
                    break;
            }
        }


        [ServerEvent(Event.PlayerDamage)] // вызываеться при уроне/отхиле
        public void OnPlayerDamage(Player player, float health, float armor)
        {
            DBFunctions.GetPlayerInstanceFromDictionary(player).hp = (int)health;
            NAPI.Chat.SendChatMessageToPlayer(player, "ServerEvent PlayerDamage");
        }


        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatID)
        {
            player.SetData("LAST_VEHICLE", vehicle);
        }

    }

}
