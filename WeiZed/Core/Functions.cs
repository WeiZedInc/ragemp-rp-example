using GTANetworkAPI;
using WeiZed.MySQL;
using WeiZed.Systems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WeiZed.Core
{

    class Functions
    {
        public static void RPChat(Player player, string message, int radius, string color)
        {
            List<Player> nearbyPlayers = NAPI.Player.GetPlayersInRadiusOfPlayer(radius, player); // Ищем игроков в радиусе и записываем в лист
            foreach (Player client in nearbyPlayers) // Перебираем игроков и отправляем им сообщение
            {
                client.SendChatMessage($"{color}{message}");
            }
        }
        public static void SendChatMessageToPlayersInRadius(Player player, int radius, string message)
        {
            message = message.Trim(); // Обрезает лишние пробелы
            List<Player> nearbyPlayers = NAPI.Player.GetPlayersInRadiusOfPlayer(radius, player); // Ищем игроков в радиусе и записываем в лист
            foreach (Player client in nearbyPlayers) // Перебираем игроков и отправляем им сообщение
            {
                client.SendChatMessage(message);
            }
        }
        public static string GetRPName(Player player) // возврат имени игрока в формате [Имя Фамилия]
        {
            string RPName = player.Name.Replace('_', ' ');
            return RPName;
        }
        public static Player GetPlayerBySocialClubName(string SocialClubName) // возврат игрока 
        {
            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (player.SocialClubName == SocialClubName)
                {
                    return player;
                }
            }
            return null;
        }
        public static Player GetPlayerByID(ushort id)
        {
            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (player.Id == id)
                    return player;
            }
            return null;
        }
        public static Vector3 GetPosition(Player sender) // возврат position
        {
            Vector3 pos = NAPI.Entity.GetEntityPosition(sender);
            if (NAPI.Player.IsPlayerInAnyVehicle(sender))
            {
                Vehicle vehicle = sender.Vehicle;
                pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
            }
            return pos;
        }
        public static Vector3 GetRotation(Player sender) // возврат rotation
        {
            Vector3 rot = NAPI.Entity.GetEntityRotation(sender);
            if (NAPI.Player.IsPlayerInAnyVehicle(sender))
            {
                Vehicle vehicle = sender.Vehicle;
                rot = NAPI.Entity.GetEntityRotation(vehicle);
            }
            return rot;
        }
        public static void SavePlayerPos(Player sender, string coordsNameInFile) // сейвит коорды в файл
        {
            Vector3 pos = NAPI.Entity.GetEntityPosition(sender);
            Vector3 rot = NAPI.Entity.GetEntityRotation(sender);

            if (NAPI.Player.IsPlayerInAnyVehicle(sender))
            {
                Vehicle vehicle = sender.Vehicle;
                pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                rot = NAPI.Entity.GetEntityRotation(vehicle);
            }
            try
            {

                StreamWriter savePos = new StreamWriter("savedcoords.txt", true, Encoding.UTF8);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                savePos.Write($"{coordsNameInFile}   Position: new Vector3({ pos.X }, { pos.Y}, { pos.Z}),  JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(pos)}    \r\n");
                savePos.Write($"{coordsNameInFile}   Rotation: new Vector3({ rot.X }, { rot.Y}, { rot.Z}),  JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(rot)}    \r\n");
                savePos.Close();
            }
            catch (Exception err)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "Exception: " + err);
            }

            finally
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, $"Position: new Vector3(X: { pos.X }, Y: { pos.Y}, Z: { pos.Z})");
                NAPI.Chat.SendChatMessageToPlayer(sender, $"Rotation: new Vector3(X: { rot.X }, Y: { rot.Y}, Z: { rot.Z})");
            }
        }
        public static void SaveVehiclePos(Player sender, string coordsNameInFile) // сейвит коорды в файл
        {
            Vector3 pos = sender.Vehicle.Position + new Vector3(0, 0, 0.5);
            Vector3 rot = sender.Vehicle.Rotation;
            try
            {

                StreamWriter savePos = new StreamWriter("savedvehcoords.txt", true, Encoding.UTF8);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                savePos.Write($"{coordsNameInFile}-pos        new Vector3( { pos.X }, { pos.Y}, { pos.Z} ),  JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(pos)}    \r\n");
                savePos.Write($"{coordsNameInFile}-rot        new Vector3( { rot.X }, { rot.Y}, { rot.Z} ),  JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(rot)}    \r\n");
                savePos.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine("Exception: " + err);
            }

            finally
            {
                sender.SendChatMessage("Saved vehicle: " + sender.Vehicle.NumberPlate + " coords to file");
                NAPI.Chat.SendChatMessageToPlayer(sender, $"Position: (X: { pos.X }, Y: { pos.Y}, Z: { pos.Z})");
                NAPI.Chat.SendChatMessageToPlayer(sender, $"Rotation: (X: { rot.X }, Y: { rot.Y}, Z: { rot.Z})");
            }
        }
        public static int ColshapeInteractionCheck(Player player)
        {
            if (player.HasData("IN_COLSHAPE"))
                return player.GetData<int>("IN_COLSHAPE");
            else
                return -1;
        }
    }


    class DBFunctions
    {
        public static CharacterData GetPlayerInstanceFromDictionary(Player player)
        {
            CharacterData data;
            if (PlayerAPI.AllPlayersDictonary.TryGetValue(player.SocialClubName, out data))
            {
                return data;
            }
            else
            {
                return null;
            }
        }

        //Getters

        public static string GetStringFromBD(string socialClubName, string table, string stringColumn, string WHERE = "socialClubName")
        {
            return MySQLStatic.QueryRead($"SELECT `{stringColumn}` FROM `{table}` WHERE `{WHERE}`='{socialClubName}'");
        }
        public static int GetIntFromBD(string socialClubName, string table, string intColumn, string WHERE = "socialClubName")
        {
            return Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `{intColumn}` FROM `{table}` WHERE `{WHERE}`='{socialClubName}'"));
        }
        public static bool GetBoolFromBD(string socialClubName, string table, string boolColumn, string WHERE = "socialClubName")
        {
            return Convert.ToBoolean(MySQLStatic.QueryRead($"SELECT `{boolColumn}` FROM `{table}` WHERE `{WHERE}`='{socialClubName}'"));
        }
        public static Vector3 GetVectorFromBD(string socialClubName, string table, string vectorColumn, string WHERE = "socialClubName")
        {
            Vector3 pos = new Vector3(0, 0, 0);
            pos = JsonConvert.DeserializeObject<Vector3>(MySQLStatic.QueryRead($"SELECT `{vectorColumn}` FROM `{table}` WHERE `{WHERE}`='{socialClubName}'"));
            return pos;
        }


        //Setters
        public static void SetStringToBD(string socialClubName, string table, string stringColumn, string newValue, string WHERE = "socialClubName")
        {
            MySQLStatic.Query($"UPDATE `{table}` SET `{stringColumn}`='{newValue}' WHERE `{WHERE}`='{socialClubName}'");
        }
        public static void SetIntToBD(string socialClubName, string table, string intColumn, int newValue, string WHERE = "socialClubName")
        {
            MySQLStatic.Query($"UPDATE `{table}` SET `{intColumn}`={newValue} WHERE `{WHERE}`='{socialClubName}'");
        }
        public static void SetBoolToBD(string socialClubName, string table, string boolColumn, bool newValue, string WHERE = "socialClubName")
        {
            MySQLStatic.Query($"UPDATE `{table}` SET `{boolColumn}`={newValue} WHERE `{WHERE}`='{socialClubName}'");
        }
        public static void SetVectorToBD(string socialClubName, string table, string vectorColumn, string newVectorValue, string WHERE = "socialClubName")
        {
            MySQLStatic.Query($"UPDATE `{table}` SET `{vectorColumn}`='{newVectorValue}' WHERE `{WHERE}`='{socialClubName}'");
        }


        // Whole table works
        public static void CreateNewCharacterRecordInDB(CharacterData newCharacter)
        {
            MySQLStatic.Query($"INSERT INTO `character_data` (`id`, `socialClubName`, `firstName`, `lastName`, `adminLVL`, `cashMoney`, `bankMoney`, " +
                    $"`bankAcc_ID`, `lvl`, `exp`, `fraction_ID`, `fractionLVL`, `spawnPosition`, `spawnRotation`, `warns`, `unwarnDate`, `isVoiceMuted`, `hp`, `biz_ID`, " +
                    $" `ownedHouse_ID`,`licenses`, `sim`, `arrestTime`, `wantedLVL`, `lastVeh`, `work_ID`, `gender`, `promo`, `birthDate`, `createDate`, " +
                    $"`isAlive`)" +
                    $" VALUES(NULL, " +
                    $"'{newCharacter.SocialClubName}', " +
                    $"'{newCharacter.firstName}', " +
                    $"'{newCharacter.lastName}', " +
                    $"'{newCharacter.adminLVL}'," +
                    $" '{newCharacter.cashMoney}', " +
                    $"'{newCharacter.bankMoney}', " +
                    $"'{newCharacter.bankAcc_ID}', " +
                    $"'{newCharacter.lvl}', " +
                    $"'{newCharacter.exp}'," +
                    $" '{newCharacter.fraction_ID}', " +
                    $"'{newCharacter.fractionLVL}', " +
                    $"'{JsonConvert.SerializeObject(newCharacter.spawnPosition)}', " +
                    $" '{JsonConvert.SerializeObject(newCharacter.spawnRotation)}', " +
                    $"'{newCharacter.warns}'," +
                    $" '{JsonConvert.SerializeObject(newCharacter.unwarnDate)}', " +
                    $"'{newCharacter.isVoiceMuted}', '{newCharacter.hp}', " +
                    $"'{newCharacter.biz_ID}'," +
                    $" '{newCharacter.ownedHouse_ID}', " +
                    $" 'licenses', " +
                    $"'{newCharacter.sim}', " +
                    $"'{newCharacter.arrestTime}', " +
                    $"'{newCharacter.wantedLVL}'," +
                    $" '{JsonConvert.SerializeObject(newCharacter.lastVeh)}', " +
                    $"'{newCharacter.work_ID}', " +
                    $" '{newCharacter.gender}', " +
                    $" '{newCharacter.promo}', " +
                    $"'{newCharacter.birthDate}', " +
                    $"'{JsonConvert.SerializeObject(newCharacter.createDate)}'," +
                    $"'{newCharacter.isAlive}')");
        }
        public static void SaveCharacterDataOnExit(Player player)
        {
            // Закоментил то что будет использоваться в других методах,можно копировать отсюда
            DBFunctions.SetStringToBD(player.SocialClubName, "character_data", "firstName", DBFunctions.GetPlayerInstanceFromDictionary(player).firstName);
            DBFunctions.SetStringToBD(player.SocialClubName, "character_data", "lastName", DBFunctions.GetPlayerInstanceFromDictionary(player).lastName);
            //DataBaseMethods.SetIntToBD(player.SocialClubName, "character_data", "adminLVL", DataBaseMethods.GetPlayerInstanceFromDictionary(player).adminLVL);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "cashMoney", DBFunctions.GetPlayerInstanceFromDictionary(player).cashMoney);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "bankMoney", DBFunctions.GetPlayerInstanceFromDictionary(player).bankMoney);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "bankAcc_ID", DBFunctions.GetPlayerInstanceFromDictionary(player).bankAcc_ID);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "lvl", DBFunctions.GetPlayerInstanceFromDictionary(player).lvl);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "exp", DBFunctions.GetPlayerInstanceFromDictionary(player).exp);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "fraction_ID", DBFunctions.GetPlayerInstanceFromDictionary(player).fraction_ID);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "fractionLVL", DBFunctions.GetPlayerInstanceFromDictionary(player).fractionLVL);
            DBFunctions.SetVectorToBD(player.SocialClubName, "character_data", "spawnPosition", $"{JsonConvert.SerializeObject(player.Position)}");
            DBFunctions.SetVectorToBD(player.SocialClubName, "character_data", "spawnRotation", $"{JsonConvert.SerializeObject(player.Rotation)}");
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "warns", DBFunctions.GetPlayerInstanceFromDictionary(player).warns);
            DBFunctions.SetStringToBD(player.SocialClubName, "character_data", "unwarnDate", DBFunctions.GetPlayerInstanceFromDictionary(player).unwarnDate);
            DBFunctions.SetBoolToBD(player.SocialClubName, "character_data", "isVoiceMuted", DBFunctions.GetPlayerInstanceFromDictionary(player).isVoiceMuted);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "hp", DBFunctions.GetPlayerInstanceFromDictionary(player).hp);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "ownedHouse_ID", DBFunctions.GetPlayerInstanceFromDictionary(player).ownedHouse_ID);
            //DataBaseMethods.SetIntToBD(player.SocialClubName, "character_data", "biz_ID", DataBaseMethods.GetPlayerInstanceFromDictionary(player).biz_ID);
            //licenses tut dolzho bit
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "sim", DBFunctions.GetPlayerInstanceFromDictionary(player).sim);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "arrestTime", DBFunctions.GetPlayerInstanceFromDictionary(player).arrestTime);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "wantedLVL", DBFunctions.GetPlayerInstanceFromDictionary(player).wantedLVL);
            DBFunctions.SetStringToBD(player.SocialClubName, "character_data", "lastVeh", DBFunctions.GetPlayerInstanceFromDictionary(player).lastVeh);
            DBFunctions.SetIntToBD(player.SocialClubName, "character_data", "work_ID", DBFunctions.GetPlayerInstanceFromDictionary(player).work_ID);
            //DataBaseMethods.SetStringToBD(player.SocialClubName, "character_data", "gender", DataBaseMethods.GetPlayerInstanceFromDictionary(player).gender);
            DBFunctions.SetStringToBD(player.SocialClubName, "character_data", "promo", DBFunctions.GetPlayerInstanceFromDictionary(player).promo);
            //DataBaseMethods.SetStringToBD(player.SocialClubName, "character_data", "birthDate", DataBaseMethods.GetPlayerInstanceFromDictionary(player).birthDate);
            //DataBaseMethods.SetStringToBD(player.SocialClubName, "character_data", "createDate", DataBaseMethods.GetPlayerInstanceFromDictionary(player).createDate);
            DBFunctions.SetBoolToBD(player.SocialClubName, "character_data", "isAlive", DBFunctions.GetPlayerInstanceFromDictionary(player).isAlive);
            PlayerAPI.AllPlayersDictonary.Remove($"{player.SocialClubName}");
            InventorySystem.AllPlayersListsOfItemsDictionary.Remove(player.SocialClubName);
        }
        public static void GetCharacterDataFromDB(CharacterData newCharacter, Player player)
        {
            newCharacter.firstName = DBFunctions.GetStringFromBD(player.SocialClubName, "character_data", "firstName");
            newCharacter.lastName = DBFunctions.GetStringFromBD(player.SocialClubName, "character_data", "lastName");
            newCharacter.adminLVL = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "adminLVL");
            newCharacter.cashMoney = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "cashMoney");
            newCharacter.bankMoney = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "bankMoney");
            newCharacter.bankAcc_ID = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "bankAcc_ID");
            newCharacter.lvl = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "lvl");
            newCharacter.exp = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "exp");
            newCharacter.fraction_ID = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "fraction_ID");
            newCharacter.fractionLVL = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "fractionLVL");
            newCharacter.spawnPosition = DBFunctions.GetVectorFromBD(player.SocialClubName, "character_data", "spawnPosition");
            newCharacter.spawnRotation = DBFunctions.GetVectorFromBD(player.SocialClubName, "character_data", "spawnRotation");
            newCharacter.warns = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "warns");
            newCharacter.unwarnDate = DBFunctions.GetStringFromBD(player.SocialClubName, "character_data", "unwarnDate");
            newCharacter.isVoiceMuted = DBFunctions.GetBoolFromBD(player.SocialClubName, "character_data", "isVoiceMuted");
            newCharacter.hp = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "hp");
            newCharacter.biz_ID = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "biz_ID");
            newCharacter.ownedHouse_ID = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "ownedHouse_ID");
            //licenses tut dolzho bit
            newCharacter.sim = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "sim");
            newCharacter.arrestTime = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "arrestTime");
            newCharacter.wantedLVL = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "wantedLVL");
            newCharacter.lastVeh = DBFunctions.GetStringFromBD(player.SocialClubName, "character_data", "lastVeh");
            newCharacter.work_ID = DBFunctions.GetIntFromBD(player.SocialClubName, "character_data", "work_ID");
            newCharacter.gender = DBFunctions.GetStringFromBD(player.SocialClubName, "character_data", "gender");
            newCharacter.promo = DBFunctions.GetStringFromBD(player.SocialClubName, "character_data", "promo");
            newCharacter.birthDate = DBFunctions.GetStringFromBD(player.SocialClubName, "character_data", "birthDate");
            newCharacter.createDate = DBFunctions.GetStringFromBD(player.SocialClubName, "character_data", "createDate");
            newCharacter.isAlive = DBFunctions.GetBoolFromBD(player.SocialClubName, "character_data", "isAlive");
        }
    }

    class AdmFunctions
    {
        public static void KillTarget(Player target, int hpValue)
        {
            NAPI.Player.SetPlayerHealth(target, hpValue);
        }
        public static void ReviveTarget(Player target)
        {
            NAPI.Player.SpawnPlayer(target, target.Position);
            NAPI.Player.SetPlayerHealth(target, 100);
        }
        public static int GetAdminLVL(Player player)
        {
            CharacterData data;
            if (PlayerAPI.AllPlayersDictonary.TryGetValue(player.SocialClubName, out data))
            {
                int admlvl = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `adminLVL` FROM `character_data` WHERE `socialClubName`='{data.SocialClubName}'"));
                if (admlvl == 0) // если админ лвл 0 отправить нотификейшн
                {
                    player.SendNotification("Нет админ прав.");
                }
                return admlvl;
            }
            return 0;
        }
        public static void KickPlayer(Player sender, ushort targetID, string reason)
        {
            if (reason != null)
            {
                reason = " Причина: " + reason;
            }
            Functions.GetPlayerByID(targetID).Kick();
            NAPI.Chat.SendChatMessageToAll($"{Colors.RED}{Functions.GetRPName(sender)} кикнул " +
                $"игрока {Functions.GetRPName(Functions.GetPlayerByID(targetID))}." + reason);
        }
        public static void TeleportTargetWithVehicleToPlayer(Player sender, Player target)
        {
            if (target.IsInVehicle)
            {
                int seatID = target.VehicleSeat;
                Vehicle targetVehicle = target.Vehicle;
                NAPI.Entity.SetEntityDimension(target, sender.Dimension);
                NAPI.Entity.SetEntityPosition(targetVehicle, sender.Position + new Vector3(2, 2, 2));
                NAPI.Entity.SetEntityDimension(targetVehicle, sender.Dimension);
                target.SetIntoVehicle(targetVehicle, seatID);
            }
            else
            {
                NAPI.Entity.SetEntityPosition(target, sender.Position);
                NAPI.Entity.SetEntityDimension(target, sender.Dimension);
            }
        }
        public static void TeleportTargetToPlayer(Player sender, Player target)
        {
            NAPI.Entity.SetEntityPosition(target, sender.Position + new Vector3(0, 0, 1));
            NAPI.Entity.SetEntityDimension(target, sender.Dimension);
        }
        public static void TeleportPlayerWithVehicleToTarget(Player sender, Player target)
        {
            if (sender.IsInVehicle)
            {
                int seatID = sender.VehicleSeat;
                Vehicle senderVehicle = sender.Vehicle;
                NAPI.Entity.SetEntityDimension(sender, target.Dimension);
                NAPI.Entity.SetEntityPosition(senderVehicle, target.Position + new Vector3(2, 2, 2));
                NAPI.Entity.SetEntityDimension(senderVehicle, target.Dimension);
                target.SetIntoVehicle(senderVehicle, seatID);
            }
            else
            {
                NAPI.Entity.SetEntityPosition(sender, target.Position);
                NAPI.Entity.SetEntityDimension(sender, target.Dimension);
            }
        }
        public static void TeleportPlayerToTarget(Player sender, Player target)
        {
            NAPI.Entity.SetEntityPosition(sender, target.Position);
            NAPI.Entity.SetEntityDimension(sender, target.Dimension);
        }

    }

    class ClientSideFunctions
    {
        public static void SetWaypoint(Player player, float x, float y)
        {
            player.TriggerEvent("client_SetNewWaypoint", x, y);
        }
        public static void ScreenFadeEffect(Player player, int durationDark, int durationLight)
        {
            player.TriggerEvent("client_ScreenFade", durationDark, durationLight);
        }
        public static void SetVehicleInvincibleAndTransparency(Player player, Vehicle vehicle, int forHowLong)
        {
            player.TriggerEvent("client_SetVehicleInvincibleAndTransparency", vehicle, forHowLong);
        }
        public static void SetVehicleInvincible(Player player, Vehicle vehicle, bool mode)
        {
            player.TriggerEvent("client_SetVehicleInvincible", vehicle, mode);
        }
        public static void AskForTrailerAttachedData(Player player)
        {
            player.TriggerEvent("client_AskForTrailer");
        }
        public static void BoostVehicleParameters(Player player, float power, float torque, float maxSpeed)
        {
            player.TriggerEvent("client_BoostVehicleParameters", power, torque, maxSpeed);
        }
    }
}
