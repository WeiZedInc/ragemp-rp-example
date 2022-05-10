using GTANetworkAPI;
using WeiZed.Core;
using WeiZed.MySQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WeiZed.Systems
{
    class InventorySystem
    {
        public enum ItemType
        {
            Mask = -1, // Маска
            Gloves = -2, // Перчатки
            Leg = -3, // Штанишки
            Bag = -4, // Рюкзачок
            Feet = -5, // Обуточки 
            Jewelry = -6, // Аксессуарчики всякие там
            Undershit = -7, // Рубашечки
            Top = -8, // Верх
            Hat = -9, // Шляпы
            Glasses = -10, // Очочки
            Accessories = -11, // Часы/Браслеты
            BodyArmor = -12, // Бронька

            HealthKit = 1,    // Аптечка
            GasCan = 2,       // Канистра
            Crisps = 3,       // Чипсы
            Beer = 4,         // Пиво
            Pizza = 5,        // Пицца
            Burger = 6,       // Бургер
            HotDog = 7,       // Хот-Дог
            Sandwich = 8,     // Сэндвич
            eCola = 9,        // Кока-Кола
            Sprunk = 10,      // Спрайт
            Lockpick = 11,    // Отмычка для замка
            BagWithMoney = 12,// Сумка с деньгами
            Material = 13,    // Материалы
            Drugs = 14,       // Наркота
            BagWithDrill = 15,// Сумка с дрелью
            ArmyLockpick = 16,// Военная отмычка
            Pocket = 17,      // Мешок
            Cuffs = 18,       // Стяжки
            CarKey = 19,      // Ключи от личной машины
            KeyRing = 20,     // Связка ключей

            /* Weapons */
            /* Pistols */
            Pistol = 100,
            CombatPistol = 101,
            Pistol50 = 102,
            SNSPistol = 103,
            HeavyPistol = 104,
            VintagePistol = 105,
            MarksmanPistol = 106,
            Revolver = 107,
            APPistol = 108,
            StunGun = 109,
            FlareGun = 110,
            DoubleAction = 111,
            PistolMk2 = 112,
            SNSPistolMk2 = 113,
            RevolverMk2 = 114,
            /* SMG */
            MicroSMG = 115,
            MachinePistol = 116,
            SMG = 117,
            AssaultSMG = 118,
            CombatPDW = 119,
            MG = 120,
            CombatMG = 121,
            Gusenberg = 122,
            MiniSMG = 123,
            SMGMk2 = 124,
            CombatMGMk2 = 125,
            /* Rifles */
            AssaultRifle = 126,
            CarbineRifle = 127,
            AdvancedRifle = 128,
            SpecialCarbine = 129,
            BullpupRifle = 130,
            CompactRifle = 131,
            AssaultRifleMk2 = 132,
            CarbineRifleMk2 = 133,
            SpecialCarbineMk2 = 134,
            BullpupRifleMk2 = 135,
            /* Sniper */
            SniperRifle = 136,
            HeavySniper = 137,
            MarksmanRifle = 138,
            HeavySniperMk2 = 139,
            MarksmanRifleMk2 = 140,
            /* Shotguns */
            PumpShotgun = 141,
            SawnOffShotgun = 142,
            BullpupShotgun = 143,
            AssaultShotgun = 144,
            Musket = 145,
            HeavyShotgun = 146,
            DoubleBarrelShotgun = 147,
            SweeperShotgun = 148,
            PumpShotgunMk2 = 149,
            /* MELEE WEAPONS */
            Knife = 180,
            Nightstick = 181,
            Hammer = 182,
            Bat = 183,
            Crowbar = 184,
            GolfClub = 185,
            Bottle = 186,
            Dagger = 187,
            Hatchet = 188,
            KnuckleDuster = 189,
            Machete = 190,
            Flashlight = 191,
            SwitchBlade = 192,
            PoolCue = 193,
            Wrench = 194,
            BattleAxe = 195,
            /* Ammo */
            PistolAmmo = 200,
            SMGAmmo = 201,
            RiflesAmmo = 202,
            SniperAmmo = 203,
            ShotgunsAmmo = 204,

        }
        public static Dictionary<int, string> ItemsNamesDictionary = new Dictionary<int, string>
        {
            {-1, "Маска" },
            {-2, "Перчатки" },
            {-3, "Штаны"},
            {-4, "Рюкзак"},
            {-5, "Обувь"},
            {-6, "Аксессуар"},
            {-7, "Нижняя одежда"},
            {-8, "Верхняя одежда"},
            {-9, "Шляпы"},
            {-10, "Очки" },
            {-11, "Часы/браслеты" },
            {-12, "Броня" },
            {1, "Аптечка"},
            {2, "Канистра"},
            {3, "Чипсы"},
            {4, "Пиво"},
            {5, "Пицца"},
            {6, "Бургер"},
            {7, "Хот-Дог"},
            {8, "Сэндвич"},
            {9, "eCola"},
            {10, "Sprunk"},
            {11, "Отмычка для замков"},
            {12, "Сумка с деньгами"},
            {13, "Материалы"},
            {14, "Наркотики"},
            {15, "Сумка с дрелью"},
            {16, "Военная отмычка"},
            {17, "Мешок"},
            {18, "Стяжки"},
            {19, "Ключи от машины"},
            {20, "Связка ключей"},

            {100, "Pistol" },
            {101, "Combat Pistol" },
            {102, "Pistol 50" },
            {103, "SNS Pistol" },
            {104, "Heavy Pistol" },
            {105, "Vintage Pistol" },
            {106, "Marksman Pistol" },
            {107, "Revolver" },
            {108, "AP Pistol" },
            {109, "Stun Gun" },
            {110, "Flare Gun" },
            {111, "Double Action" },
            {112, "Pistol Mk2" },
            {113, "SNSPistol Mk2" },
            {114, "Revolver Mk2" },

            {115, "Micro SMG" },
            {116, "Machine Pistol" },
            {117, "SMG" },
            {118, "Assault SMG" },
            {119, "Combat PDW" },
            {120, "MG" },
            {121, "Combat MG" },
            {122, "Gusenberg" },
            {123, "Mini SMG" },
            {124, "SMG Mk2" },
            {125, "Combat MG Mk2" },

            {126, "Assault Rifle" },
            {127, "Carbine Rifle" },
            {128, "Advanced Rifle" },
            {129, "Special Carbine" },
            {130, "Bullpup Rifle" },
            {131, "Compact Rifle" },
            {132, "Assault Rifle Mk2" },
            {133, "Carbine Rifle Mk2" },
            {134, "Special Carbine Mk2" },
            {135, "Bullpup Rifle Mk2" },

            {136, "Sniper Rifle" },
            {137, "Heavy Sniper" },
            {138, "Marksman Rifle" },
            {139, "Heavy Sniper Mk2" },
            {140, "Marksman Rifle Mk2" },

            {141, "Pump Shotgun" },
            {142, "SawnOff Shotgun" },
            {143, "Bullpup Shotgun" },
            {144, "Assault Shotgun" },
            {145, "Musket" },
            {146, "Heavy Shotgun" },
            {147, "Double Barrel Shotgun" },
            {148, "Sweeper Shotgun" },
            {149, "Pump Shotgun Mk2" },

            {180, "Нож" },
            {181, "Дубинка" },
            {182, "Молоток" },
            {183, "Бита" },
            {184, "Лом" },
            {185, "Гольф клюшка" },
            {186, "Бутылка" },
            {187, "Кинжал" },
            {188, "Топор" },
            {189, "Кастет" },
            {190, "Мачете" },
            {191, "Фонарик" },
            {192, "Швейцарский нож" },
            {193, "Кий" },
            {194, "Ключ" },
            {195, "Боевой топор" },

            {200, "Пистолетный калибр" },
            {201, "Малый калибр" },
            {202, "Автоматный калибр" },
            {203, "Снайперский калибр" },
            {204, "Дробь" }
        };
        public static Dictionary<ItemType, uint> ItemModelsDictionary = new Dictionary<ItemType, uint>()
        {
            { ItemType.Hat, 1619813869 },
            { ItemType.Mask, 3887136870 },
            { ItemType.Gloves, 3125389411 },
            { ItemType.Leg, 2086911125 },
            { ItemType.Bag, 0000000 },
            { ItemType.Feet, 1682675077 },
            { ItemType.Jewelry, 2329969874 },
            { ItemType.Undershit, 578126062 },
            { ItemType.BodyArmor, 701173564 },
            { ItemType.Top, 3038378640 },
            { ItemType.Glasses, 2329969874 },
            { ItemType.Accessories, 2329969874 },

            { ItemType.Drugs, 4293279169 },
            { ItemType.Material, 3045218749 },
            { ItemType.HealthKit, 678958360 },
            { ItemType.GasCan, 786272259 },
            { ItemType.Crisps, 2564432314 },
            { ItemType.Beer, 1940235411 },
            { ItemType.Pizza, 604847691 },
            { ItemType.Burger, 2240524752 },
            { ItemType.HotDog, 2565741261 },
            { ItemType.Sandwich, 987331897 },
            { ItemType.eCola, 144995201 },
            { ItemType.Sprunk, 2973713592 },
            { ItemType.Lockpick, 977923025 },
            { ItemType.ArmyLockpick, 977923025 },
            { ItemType.Pocket, 3887136870 },
            { ItemType.Cuffs, 3887136870 },
            { ItemType.CarKey, 977923025 },
            { ItemType.KeyRing, 977923025 },

            { ItemType.Pistol, NAPI.Util.GetHashKey("w_pi_pistol") },
            { ItemType.CombatPistol, NAPI.Util.GetHashKey("w_pi_combatpistol") },
            { ItemType.Pistol50, NAPI.Util.GetHashKey("w_pi_pistol50") },
            { ItemType.SNSPistol, NAPI.Util.GetHashKey("w_pi_sns_pistol") },
            { ItemType.HeavyPistol, NAPI.Util.GetHashKey("w_pi_heavypistol") },
            { ItemType.VintagePistol, NAPI.Util.GetHashKey("w_pi_vintage_pistol") },
            { ItemType.MarksmanPistol, NAPI.Util.GetHashKey("w_pi_singleshot") },
            { ItemType.Revolver, NAPI.Util.GetHashKey("w_pi_revolver") },
            { ItemType.APPistol, NAPI.Util.GetHashKey("w_pi_appistol") },
            { ItemType.StunGun, NAPI.Util.GetHashKey("w_pi_stungun") },
            { ItemType.FlareGun, NAPI.Util.GetHashKey("w_pi_flaregun") },
            { ItemType.DoubleAction, NAPI.Util.GetHashKey("mk2") },
            { ItemType.PistolMk2, NAPI.Util.GetHashKey("w_pi_pistolmk2") },
            { ItemType.SNSPistolMk2, NAPI.Util.GetHashKey("w_pi_sns_pistolmk2") },
            { ItemType.RevolverMk2, NAPI.Util.GetHashKey("w_pi_revolvermk2") },

            { ItemType.MicroSMG, NAPI.Util.GetHashKey("w_sb_microsmg") },
            { ItemType.MachinePistol, NAPI.Util.GetHashKey("w_sb_compactsmg") },
            { ItemType.SMG, NAPI.Util.GetHashKey("w_sb_smg") },
            { ItemType.AssaultSMG, NAPI.Util.GetHashKey("w_sb_assaultsmg") },
            { ItemType.CombatPDW, NAPI.Util.GetHashKey("w_sb_pdw") },
            { ItemType.MG, NAPI.Util.GetHashKey("w_mg_mg") },
            { ItemType.CombatMG, NAPI.Util.GetHashKey("w_mg_combatmg") },
            { ItemType.Gusenberg, NAPI.Util.GetHashKey("w_sb_gusenberg") },
            { ItemType.MiniSMG, NAPI.Util.GetHashKey("w_sb_minismg") },
            { ItemType.SMGMk2, NAPI.Util.GetHashKey("w_sb_smgmk2") },
            { ItemType.CombatMGMk2, NAPI.Util.GetHashKey("w_mg_combatmgmk2") },

            { ItemType.AssaultRifle, NAPI.Util.GetHashKey("w_ar_assaultrifle") },
            { ItemType.CarbineRifle, NAPI.Util.GetHashKey("w_ar_carbinerifle") },
            { ItemType.AdvancedRifle, NAPI.Util.GetHashKey("w_ar_advancedrifle") },
            { ItemType.SpecialCarbine, NAPI.Util.GetHashKey("w_ar_specialcarbine") },
            { ItemType.BullpupRifle, NAPI.Util.GetHashKey("w_ar_bullpuprifle") },
            { ItemType.CompactRifle, NAPI.Util.GetHashKey("w_ar_assaultrifle_smg") },
            { ItemType.AssaultRifleMk2, NAPI.Util.GetHashKey("w_ar_assaultriflemk2") },
            { ItemType.CarbineRifleMk2, NAPI.Util.GetHashKey("w_ar_carbineriflemk2") },
            { ItemType.SpecialCarbineMk2, NAPI.Util.GetHashKey("w_ar_specialcarbinemk2") },
            { ItemType.BullpupRifleMk2, NAPI.Util.GetHashKey("w_ar_bullpupriflemk2") },

            { ItemType.SniperRifle, NAPI.Util.GetHashKey("w_sr_sniperrifle") },
            { ItemType.HeavySniper, NAPI.Util.GetHashKey("w_sr_heavysniper") },
            { ItemType.MarksmanRifle, NAPI.Util.GetHashKey("w_sr_marksmanrifle") },
            { ItemType.HeavySniperMk2, NAPI.Util.GetHashKey("w_sr_heavysnipermk2") },
            { ItemType.MarksmanRifleMk2, NAPI.Util.GetHashKey("w_sr_marksmanriflemk2") },

            { ItemType.PumpShotgun, NAPI.Util.GetHashKey("w_sg_pumpshotgun") },
            { ItemType.SawnOffShotgun, NAPI.Util.GetHashKey("w_sg_sawnoff") },
            { ItemType.BullpupShotgun, NAPI.Util.GetHashKey("w_sg_bullpupshotgun") },
            { ItemType.AssaultShotgun, NAPI.Util.GetHashKey("w_sg_assaultshotgun") },
            { ItemType.Musket, NAPI.Util.GetHashKey("w_ar_musket") },
            { ItemType.HeavyShotgun, NAPI.Util.GetHashKey("w_sg_heavyshotgun") },
            { ItemType.DoubleBarrelShotgun, NAPI.Util.GetHashKey("w_sg_doublebarrel") },
            { ItemType.SweeperShotgun, NAPI.Util.GetHashKey("mk2") },
            { ItemType.PumpShotgunMk2, NAPI.Util.GetHashKey("w_sg_pumpshotgunmk2") },

            { ItemType.Knife, NAPI.Util.GetHashKey("w_me_knife_01") },
            { ItemType.Nightstick, NAPI.Util.GetHashKey("w_me_nightstick") },
            { ItemType.Hammer, NAPI.Util.GetHashKey("w_me_hammer") },
            { ItemType.Bat, NAPI.Util.GetHashKey("w_me_bat") },
            { ItemType.Crowbar, NAPI.Util.GetHashKey("w_me_crowbar") },
            { ItemType.GolfClub, NAPI.Util.GetHashKey("w_me_gclub") },
            { ItemType.Bottle, NAPI.Util.GetHashKey("w_me_bottle") },
            { ItemType.Dagger, NAPI.Util.GetHashKey("w_me_dagger") },
            { ItemType.Hatchet, NAPI.Util.GetHashKey("w_me_hatchet") },
            { ItemType.KnuckleDuster, NAPI.Util.GetHashKey("w_me_knuckle") },
            { ItemType.Machete, NAPI.Util.GetHashKey("prop_ld_w_me_machette") },
            { ItemType.Flashlight, NAPI.Util.GetHashKey("w_me_flashlight") },
            { ItemType.SwitchBlade, NAPI.Util.GetHashKey("w_me_switchblade") },
            { ItemType.PoolCue, NAPI.Util.GetHashKey("prop_pool_cue") },
            { ItemType.Wrench, NAPI.Util.GetHashKey("prop_cs_wrench") },
            { ItemType.BattleAxe, NAPI.Util.GetHashKey("w_me_battleaxe") },

            { ItemType.PistolAmmo, NAPI.Util.GetHashKey("w_am_case") },
            { ItemType.RiflesAmmo, NAPI.Util.GetHashKey("w_am_case") },
            { ItemType.ShotgunsAmmo, NAPI.Util.GetHashKey("w_am_case") },
            { ItemType.SMGAmmo, NAPI.Util.GetHashKey("w_am_case") },
            { ItemType.SniperAmmo, NAPI.Util.GetHashKey("w_am_case") },
        };
        public static Dictionary<ItemType, Vector3> ItemsPosOffsetDictionary = new Dictionary<ItemType, Vector3>()
        {
            { ItemType.Hat, new Vector3(0, 0, -0.93) },
            { ItemType.Mask, new Vector3(0, 0, -1) },
            { ItemType.Gloves, new Vector3(0, 0, -1) },
            { ItemType.Leg, new Vector3(0, 0, -0.85) },
            { ItemType.Bag, new Vector3() },
            { ItemType.Feet, new Vector3(0, 0, -0.95) },
            { ItemType.Jewelry, new Vector3(0, 0, -0.98) },
            { ItemType.Undershit, new Vector3(0, 0, -0.98) },
            { ItemType.BodyArmor, new Vector3(0, 0, -0.88) },
            { ItemType.Top, new Vector3(0, 0, -0.96) },
            { ItemType.Glasses, new Vector3(0, 0, -0.98) },
            { ItemType.Accessories, new Vector3(0, 0, -0.98) },

            { ItemType.Drugs, new Vector3(0, 0, -0.95) },
            { ItemType.Material, new Vector3(0, 0, -0.6) },
            { ItemType.HealthKit, new Vector3(0, 0, -0.9) },
            { ItemType.GasCan, new Vector3(0, 0, -1) },
            { ItemType.Crisps, new Vector3(0, 0, -1) },
            { ItemType.Beer, new Vector3(0, 0, -1) },
            { ItemType.Pizza, new Vector3(0, 0, -1) },
            { ItemType.Burger, new Vector3(0, 0, -0.97) },
            { ItemType.HotDog, new Vector3(0, 0, -0.97) },
            { ItemType.Sandwich, new Vector3(0, 0, -0.99) },
            { ItemType.eCola, new Vector3(0, 0, -1) },
            { ItemType.Sprunk, new Vector3(0, 0, -1) },
            { ItemType.Lockpick, new Vector3(0, 0, -0.98) },
            { ItemType.ArmyLockpick, new Vector3(0, 0, -0.98) },
            { ItemType.Pocket, new Vector3(0, 0, -0.98) },
            { ItemType.Cuffs, new Vector3(0, 0, -0.98) },
            { ItemType.CarKey, new Vector3(0, 0, -0.98) },
            { ItemType.KeyRing, new Vector3(0, 0, -0.98) },

            { ItemType.Pistol, new Vector3(0, 0, -0.99) },
            { ItemType.CombatPistol, new Vector3(0, 0, -0.99) },
            { ItemType.Pistol50, new Vector3(0, 0, -0.99) },
            { ItemType.SNSPistol, new Vector3(0, 0, -0.99) },
            { ItemType.HeavyPistol, new Vector3(0, 0, -0.99) },
            { ItemType.VintagePistol, new Vector3(0, 0, -0.99) },
            { ItemType.MarksmanPistol, new Vector3(0, 0, -0.99) },
            { ItemType.Revolver, new Vector3(0, 0, -0.99) },
            { ItemType.APPistol, new Vector3(0, 0, -0.99) },
            { ItemType.StunGun, new Vector3(0, 0, -0.99) },
            { ItemType.FlareGun, new Vector3(0, 0, -0.99) },
            { ItemType.DoubleAction, new Vector3(0, 0, -0.99) },
            { ItemType.PistolMk2, new Vector3(0, 0, -0.99) },
            { ItemType.SNSPistolMk2, new Vector3(0, 0, -0.99) },
            { ItemType.RevolverMk2, new Vector3(0, 0, -0.99) },

            { ItemType.MicroSMG, new Vector3(0, 0, -0.99) },
            { ItemType.MachinePistol, new Vector3(0, 0, -0.99) },
            { ItemType.SMG, new Vector3(0, 0, -0.99) },
            { ItemType.AssaultSMG, new Vector3(0, 0, -0.99) },
            { ItemType.CombatPDW, new Vector3(0, 0, -0.99) },
            { ItemType.MG, new Vector3(0, 0, -0.99) },
            { ItemType.CombatMG, new Vector3(0, 0, -0.99) },
            { ItemType.Gusenberg, new Vector3(0, 0, -0.99) },
            { ItemType.MiniSMG, new Vector3(0, 0, -0.99) },
            { ItemType.SMGMk2, new Vector3(0, 0, -0.99) },
            { ItemType.CombatMGMk2, new Vector3(0, 0, -0.99) },

            { ItemType.AssaultRifle, new Vector3(0, 0, -0.99) },
            { ItemType.CarbineRifle, new Vector3(0, 0, -0.99) },
            { ItemType.AdvancedRifle, new Vector3(0, 0, -0.99) },
            { ItemType.SpecialCarbine, new Vector3(0, 0, -0.99) },
            { ItemType.BullpupRifle, new Vector3(0, 0, -0.99) },
            { ItemType.CompactRifle, new Vector3(0, 0, -0.99) },
            { ItemType.AssaultRifleMk2, new Vector3(0, 0, -0.99) },
            { ItemType.CarbineRifleMk2, new Vector3(0, 0, -0.99) },
            { ItemType.SpecialCarbineMk2, new Vector3(0, 0, -0.99) },
            { ItemType.BullpupRifleMk2, new Vector3(0, 0, -0.99) },

            { ItemType.SniperRifle, new Vector3(0, 0, -0.99) },
            { ItemType.HeavySniper, new Vector3(0, 0, -0.99) },
            { ItemType.MarksmanRifle, new Vector3(0, 0, -0.99) },
            { ItemType.HeavySniperMk2, new Vector3(0, 0, -0.99) },
            { ItemType.MarksmanRifleMk2, new Vector3(0, 0, -0.99) },

            { ItemType.PumpShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.SawnOffShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.BullpupShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.AssaultShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.Musket, new Vector3(0, 0, -0.99) },
            { ItemType.HeavyShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.DoubleBarrelShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.SweeperShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.PumpShotgunMk2, new Vector3(0, 0, -0.99) },

            { ItemType.Knife, new Vector3(0, 0, -0.99) },
            { ItemType.Nightstick, new Vector3(0, 0, -0.99) },
            { ItemType.Hammer, new Vector3(0, 0, -0.99) },
            { ItemType.Bat, new Vector3(0, 0, -0.99) },
            { ItemType.Crowbar, new Vector3(0, 0, -0.99) },
            { ItemType.GolfClub, new Vector3(0, 0, -0.99) },
            { ItemType.Bottle, new Vector3(0, 0, -0.99) },
            { ItemType.Dagger, new Vector3(0, 0, -0.99) },
            { ItemType.Hatchet, new Vector3(0, 0, -0.99) },
            { ItemType.KnuckleDuster, new Vector3(0, 0, -0.99) },
            { ItemType.Machete, new Vector3(0, 0, -0.99) },
            { ItemType.Flashlight, new Vector3(0, 0, -0.99) },
            { ItemType.SwitchBlade, new Vector3(0, 0, -0.99) },
            { ItemType.PoolCue, new Vector3(0, 0, -0.99) },
            { ItemType.Wrench, new Vector3(0, 0, -0.985) },
            { ItemType.BattleAxe, new Vector3(0, 0, -0.99) },

            { ItemType.PistolAmmo, new Vector3(0, 0, -1) },
            { ItemType.RiflesAmmo, new Vector3(0, 0, -1) },
            { ItemType.ShotgunsAmmo, new Vector3(0, 0, -1) },
            { ItemType.SMGAmmo, new Vector3(0, 0, -1) },
            { ItemType.SniperAmmo, new Vector3(0, 0, -1) },
        };
        public static Dictionary<ItemType, Vector3> ItemsRotOffsetDictionary = new Dictionary<ItemType, Vector3>()
        {
            { ItemType.Hat, new Vector3() },
            { ItemType.Mask, new Vector3() },
            { ItemType.Gloves, new Vector3(90, 0, 0) },
            { ItemType.Leg, new Vector3() },
            { ItemType.Bag, new Vector3() },
            { ItemType.Feet, new Vector3() },
            { ItemType.Jewelry, new Vector3() },
            { ItemType.Undershit, new Vector3() },
            { ItemType.BodyArmor, new Vector3(90, 90, 0) },
            { ItemType.Top, new Vector3() },
            { ItemType.Glasses, new Vector3() },
            { ItemType.Accessories, new Vector3() },

            { ItemType.Drugs, new Vector3() },
            { ItemType.Material, new Vector3() },
            { ItemType.HealthKit, new Vector3() },
            { ItemType.GasCan, new Vector3() },
            { ItemType.Crisps, new Vector3(90, 90, 0) },
            { ItemType.Beer, new Vector3() },
            { ItemType.Pizza, new Vector3() },
            { ItemType.Burger, new Vector3() },
            { ItemType.HotDog, new Vector3() },
            { ItemType.Sandwich, new Vector3() },
            { ItemType.eCola, new Vector3() },
            { ItemType.Sprunk, new Vector3() },
            { ItemType.Lockpick, new Vector3() },
            { ItemType.ArmyLockpick, new Vector3() },
            { ItemType.Pocket, new Vector3() },
            { ItemType.Cuffs, new Vector3() },
            { ItemType.CarKey, new Vector3() },
            { ItemType.KeyRing, new Vector3() },

            { ItemType.Pistol, new Vector3(90, 0, 0) },
            { ItemType.CombatPistol, new Vector3(90, 0, 0) },
            { ItemType.Pistol50, new Vector3(90, 0, 0) },
            { ItemType.SNSPistol, new Vector3(90, 0, 0) },
            { ItemType.HeavyPistol, new Vector3(90, 0, 0) },
            { ItemType.VintagePistol, new Vector3(90, 0, 0) },
            { ItemType.MarksmanPistol, new Vector3(90, 0, 0) },
            { ItemType.Revolver, new Vector3(90, 0, 0) },
            { ItemType.APPistol, new Vector3(90, 0, 0) },
            { ItemType.StunGun, new Vector3(90, 0, 0) },
            { ItemType.FlareGun, new Vector3(90, 0, 0) },
            { ItemType.DoubleAction, new Vector3(90, 0, 0) },
            { ItemType.PistolMk2, new Vector3(90, 0, 0) },
            { ItemType.SNSPistolMk2, new Vector3(90, 0, 0) },
            { ItemType.RevolverMk2, new Vector3(90, 0, 0) },

            { ItemType.MicroSMG, new Vector3(90, 0, 0) },
            { ItemType.MachinePistol, new Vector3(90, 0, 0) },
            { ItemType.SMG, new Vector3(90, 0, 0) },
            { ItemType.AssaultSMG, new Vector3(90, 0, 0) },
            { ItemType.CombatPDW, new Vector3(90, 0, 0) },
            { ItemType.MG, new Vector3(90, 0, 0) },
            { ItemType.CombatMG, new Vector3(90, 0, 0) },
            { ItemType.Gusenberg, new Vector3(90, 0, 0) },
            { ItemType.MiniSMG, new Vector3(90, 0, 0) },
            { ItemType.SMGMk2, new Vector3(90, 0, 0) },
            { ItemType.CombatMGMk2, new Vector3(90, 0, 0) },

            { ItemType.AssaultRifle, new Vector3(90, 0, 0) },
            { ItemType.CarbineRifle, new Vector3(90, 0, 0) },
            { ItemType.AdvancedRifle, new Vector3(90, 0, 0) },
            { ItemType.SpecialCarbine, new Vector3(90, 0, 0) },
            { ItemType.BullpupRifle, new Vector3(90, 0, 0) },
            { ItemType.CompactRifle, new Vector3(90, 0, 0) },
            { ItemType.AssaultRifleMk2, new Vector3(90, 0, 0) },
            { ItemType.CarbineRifleMk2, new Vector3(90, 0, 0) },
            { ItemType.SpecialCarbineMk2, new Vector3(90, 0, 0) },
            { ItemType.BullpupRifleMk2, new Vector3(90, 0, 0) },

            { ItemType.SniperRifle, new Vector3(90, 0, 0) },
            { ItemType.HeavySniper, new Vector3(90, 0, 0) },
            { ItemType.MarksmanRifle, new Vector3(90, 0, 0) },
            { ItemType.HeavySniperMk2, new Vector3(90, 0, 0) },
            { ItemType.MarksmanRifleMk2, new Vector3(90, 0, 0) },

            { ItemType.PumpShotgun, new Vector3(90, 0, 0) },
            { ItemType.SawnOffShotgun, new Vector3(90, 0, 0) },
            { ItemType.BullpupShotgun, new Vector3(90, 0, 0) },
            { ItemType.AssaultShotgun, new Vector3(90, 0, 0) },
            { ItemType.Musket, new Vector3(90, 0, 0) },
            { ItemType.HeavyShotgun, new Vector3(90, 0, 0) },
            { ItemType.DoubleBarrelShotgun, new Vector3(90, 0, 0) },
            { ItemType.SweeperShotgun, new Vector3(90, 0, 0) },
            { ItemType.PumpShotgunMk2, new Vector3(90, 0, 0) },

            { ItemType.Knife, new Vector3(90, 0, 0) },
            { ItemType.Nightstick, new Vector3(90, 0, 0) },
            { ItemType.Hammer, new Vector3(90, 0, 0) },
            { ItemType.Bat, new Vector3(90, 0, 0) },
            { ItemType.Crowbar, new Vector3(90, 0, 0) },
            { ItemType.GolfClub, new Vector3(90, 0, 0) },
            { ItemType.Bottle, new Vector3(90, 0, 0) },
            { ItemType.Dagger, new Vector3(90, 0, 0) },
            { ItemType.Hatchet, new Vector3(90, 0, 0) },
            { ItemType.KnuckleDuster, new Vector3(90, 0, 0) },
            { ItemType.Machete, new Vector3(90, 0, 0) },
            { ItemType.Flashlight, new Vector3(90, 0, 0) },
            { ItemType.SwitchBlade, new Vector3(90, 0, 0) },
            { ItemType.PoolCue, new Vector3(90, 0, 0) },
            { ItemType.Wrench, new Vector3(-12, 0, 0) },
            { ItemType.BattleAxe, new Vector3(90, 0, 0) },

            { ItemType.PistolAmmo, new Vector3(90, 0, 0) },
            { ItemType.RiflesAmmo, new Vector3(90, 0, 0) },
            { ItemType.ShotgunsAmmo, new Vector3(90, 0, 0) },
            { ItemType.SMGAmmo, new Vector3(90, 0, 0) },
            { ItemType.SniperAmmo, new Vector3(90, 0, 0) },

        };
        public static Dictionary<ItemType, int> ItemsStacksDictionary = new Dictionary<ItemType, int>()
        {
            { ItemType.BagWithMoney, 1 },
            { ItemType.Material, 300 },
            { ItemType.Drugs, 50 },
            { ItemType.BagWithDrill, 1 },
            { ItemType.HealthKit, 5 },
            { ItemType.GasCan, 2 },
            { ItemType.Crisps, 4 },
            { ItemType.Beer, 5 },
            { ItemType.Pizza, 3 },
            { ItemType.Burger, 4 },
            { ItemType.HotDog, 5 },
            { ItemType.Sandwich, 7 },
            { ItemType.eCola, 5 },
            { ItemType.Sprunk, 5 },
            { ItemType.Lockpick, 10 },
            { ItemType.ArmyLockpick, 10 },
            { ItemType.Pocket, 5 },
            { ItemType.Cuffs, 5 },
            { ItemType.CarKey, 1 },
            { ItemType.KeyRing, 1 },

            { ItemType.Mask, 1 },
            { ItemType.Gloves, 1 },
            { ItemType.Leg, 1 },
            { ItemType.Bag, 1 },
            { ItemType.Feet, 1 },
            { ItemType.Jewelry, 1 },
            { ItemType.Undershit, 1 },
            { ItemType.BodyArmor, 1 },
            { ItemType.Top, 1 },
            { ItemType.Hat, 1 },
            { ItemType.Glasses, 1 },
            { ItemType.Accessories, 1 },

            { ItemType.Pistol, 1 },
            { ItemType.CombatPistol, 1 },
            { ItemType.Pistol50, 1 },
            { ItemType.SNSPistol, 1 },
            { ItemType.HeavyPistol, 1 },
            { ItemType.VintagePistol, 1 },
            { ItemType.MarksmanPistol, 1 },
            { ItemType.Revolver, 1 },
            { ItemType.APPistol, 1 },
            { ItemType.StunGun, 1 },
            { ItemType.FlareGun, 1 },
            { ItemType.DoubleAction, 1 },
            { ItemType.PistolMk2, 1 },
            { ItemType.SNSPistolMk2, 1 },
            { ItemType.RevolverMk2, 1 },

            { ItemType.MicroSMG, 1 },
            { ItemType.MachinePistol, 1 },
            { ItemType.SMG, 1 },
            { ItemType.AssaultSMG, 1 },
            { ItemType.CombatPDW, 1 },
            { ItemType.MG, 1 },
            { ItemType.CombatMG, 1 },
            { ItemType.Gusenberg, 1 },
            { ItemType.MiniSMG, 1 },
            { ItemType.SMGMk2, 1 },
            { ItemType.CombatMGMk2, 1 },

            { ItemType.AssaultRifle, 1 },
            { ItemType.CarbineRifle, 1 },
            { ItemType.AdvancedRifle, 1 },
            { ItemType.SpecialCarbine, 1 },
            { ItemType.BullpupRifle, 1 },
            { ItemType.CompactRifle, 1 },
            { ItemType.AssaultRifleMk2, 1 },
            { ItemType.CarbineRifleMk2, 1 },
            { ItemType.SpecialCarbineMk2, 1 },
            { ItemType.BullpupRifleMk2, 1 },

            { ItemType.SniperRifle, 1 },
            { ItemType.HeavySniper, 1 },
            { ItemType.MarksmanRifle, 1 },
            { ItemType.HeavySniperMk2, 1 },
            { ItemType.MarksmanRifleMk2, 1 },

            { ItemType.PumpShotgun, 1 },
            { ItemType.SawnOffShotgun, 1 },
            { ItemType.BullpupShotgun, 1 },
            { ItemType.AssaultShotgun, 1 },
            { ItemType.Musket, 1 },
            { ItemType.HeavyShotgun, 1 },
            { ItemType.DoubleBarrelShotgun, 1 },
            { ItemType.SweeperShotgun, 1 },
            { ItemType.PumpShotgunMk2, 1 },

            { ItemType.Knife, 1 },
            { ItemType.Nightstick, 1 },
            { ItemType.Hammer, 1 },
            { ItemType.Bat, 1 },
            { ItemType.Crowbar, 1 },
            { ItemType.GolfClub, 1 },
            { ItemType.Bottle, 1 },
            { ItemType.Dagger, 1 },
            { ItemType.Hatchet, 1 },
            { ItemType.KnuckleDuster, 1 },
            { ItemType.Machete, 1 },
            { ItemType.Flashlight, 1 },
            { ItemType.SwitchBlade, 1 },
            { ItemType.PoolCue, 1 },
            { ItemType.Wrench, 1 },
            { ItemType.BattleAxe, 1 },

            { ItemType.PistolAmmo, 120 },
            { ItemType.RiflesAmmo, 200 },
            { ItemType.ShotgunsAmmo, 100 },
            { ItemType.SMGAmmo, 200 },
            { ItemType.SniperAmmo, 20 },
        };
        public static Dictionary<string, List<Item>> AllPlayersListsOfItemsDictionary = new Dictionary<string, List<Item>>(); // SocialClubName, (list of his items)

        public static List<Item> WholeDroppedItemsList = new List<Item>();
        public static List<ItemType> WholeItemsList = new List<ItemType>()
        {
            //clothes
            ItemType.Mask,
            ItemType.Gloves,
            ItemType.Leg,
            ItemType.Bag,
            ItemType.Feet,
            ItemType.Jewelry,
            ItemType.Undershit,
            ItemType.BodyArmor,
            ItemType.Top,
            ItemType.Hat,
            ItemType.Glasses,
            ItemType.Accessories,


            //items
            ItemType.HealthKit,
            ItemType.GasCan,
            ItemType.Crisps,
            ItemType.Beer,
            ItemType.Pizza,
            ItemType.Burger,
            ItemType.eCola,
            ItemType.Sandwich,
            ItemType.Sprunk,
            ItemType.Lockpick,
            ItemType.BagWithMoney,
            ItemType.Material,
            ItemType.Drugs,
            ItemType.BagWithDrill,
            ItemType.ArmyLockpick,
            ItemType.Pocket, // meshok
            ItemType.Cuffs,
            ItemType.CarKey,
            ItemType.KeyRing,

            //guns 
            ItemType.Pistol,
            ItemType.CombatPistol,
            ItemType.Pistol50,
            ItemType.SNSPistol,
            ItemType.HeavyPistol,
            ItemType.VintagePistol,
            ItemType.MarksmanPistol,
            ItemType.Revolver,
            ItemType.APPistol,
            ItemType.FlareGun,
            ItemType.DoubleAction,
            ItemType.PistolMk2,
            ItemType.SNSPistolMk2,
            ItemType.RevolverMk2,

            ItemType.MicroSMG,
            ItemType.MachinePistol,
            ItemType.SMG,
            ItemType.AssaultSMG,
            ItemType.CombatPDW,
            ItemType.MG,
            ItemType.CombatMG,
            ItemType.Gusenberg,
            ItemType.MiniSMG,
            ItemType.SMGMk2,
            ItemType.CombatMGMk2,

            ItemType.AssaultRifle,
            ItemType.CarbineRifle,
            ItemType.AdvancedRifle,
            ItemType.SpecialCarbine,
            ItemType.BullpupRifle,
            ItemType.CompactRifle,
            ItemType.AssaultRifleMk2,
            ItemType.CarbineRifleMk2,
            ItemType.SpecialCarbineMk2,
            ItemType.BullpupRifleMk2,

            ItemType.SniperRifle,
            ItemType.HeavySniper,
            ItemType.MarksmanRifle,
            ItemType.HeavySniperMk2,
            ItemType.MarksmanRifleMk2,

            ItemType.PumpShotgun,
            ItemType.SawnOffShotgun,
            ItemType.BullpupShotgun,
            ItemType.AssaultShotgun,
            ItemType.Musket,
            ItemType.HeavyShotgun,
            ItemType.DoubleBarrelShotgun,
            ItemType.SweeperShotgun,
            ItemType.PumpShotgunMk2,


            //melee weapons
            ItemType.Knife,
            ItemType.Nightstick,
            ItemType.Hammer,
            ItemType.Bat,
            ItemType.Crowbar,
            ItemType.GolfClub,
            ItemType.Bottle,
            ItemType.Dagger,
            ItemType.Hatchet,
            ItemType.KnuckleDuster,
            ItemType.Machete,
            ItemType.Flashlight,
            ItemType.SwitchBlade,
            ItemType.PoolCue,
            ItemType.Wrench,
            ItemType.BattleAxe,
            ItemType.StunGun,


            //ammo
            ItemType.PistolAmmo,
            ItemType.RiflesAmmo,
            ItemType.ShotgunsAmmo,
            ItemType.SMGAmmo,
            ItemType.SniperAmmo
        };

        public static void AddToInventory(Player player, Item itemToAdd, int quantity = 1)
        {
            int itemKey = FindItemKey(player.SocialClubName, itemToAdd.Type);
            List<Item> itemsList = AllPlayersListsOfItemsDictionary[player.SocialClubName];
            if (!IsInventoryFull(player.SocialClubName))
            {
                if (itemKey != -1) // если предмет такой предмет был найден в списке предметов добавить кол-во, если не был то добавить предмет
                {
                    itemsList[itemKey].Quantity += quantity; // добавляет в стак любой предмет,надо пофиксить
                    NAPI.Chat.SendChatMessageToAll("Added to inv quantity - " + quantity);
                    NAPI.Chat.SendChatMessageToAll("Now in inv " + itemsList[itemKey].Quantity.ToString());
                }
                else
                {
                    int i = 0;
                    NAPI.Task.Run(() =>
                    {
                        do
                        {
                            itemsList.Add(itemToAdd);
                            i++;
                        } while (i < quantity);
                        NAPI.Chat.SendChatMessageToAll($"Added {i} " + itemToAdd.Type.ToString());
                    });
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToAll("Inventory is full!");
            }
        }
        public static void RemoveFromInventory(string socialClubName, Item item, int quantity = 1, bool removeAll = false)
        {
            int index = FindItemKey(socialClubName, item.Type);
            if (index != -1)
            {
                List<Item> itemsList = AllPlayersListsOfItemsDictionary[socialClubName];
                if (!removeAll) // если удалить все
                {
                    itemsList.RemoveAt(index);
                }
                else
                {
                    int count = itemsList[index].Quantity - quantity;
                    if (count > 0)
                    {
                        itemsList[index].Quantity = count;
                    }
                    else
                    {
                        itemsList.RemoveAt(index);
                    }
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToAll("Not found item in inventory");
                return;
            }
        }


        public static void SaveAllItems()
        {
            if (AllPlayersListsOfItemsDictionary.Count == 0)
                return;

            foreach (var keyValuePair in AllPlayersListsOfItemsDictionary)
            {
                string socialClubName = keyValuePair.Key;
                List<Item> itemList = keyValuePair.Value;
                string serialized = JsonConvert.SerializeObject(itemList);
                DBFunctions.SetStringToBD(socialClubName, "inventory", "items", serialized, "ownerSocialName");
                NAPI.Chat.SendChatMessageToAll("All Saved to inv");
            }
        }
        public static void SavePlayerItems(string socialClubName)
        {
            if (!AllPlayersListsOfItemsDictionary.ContainsKey(socialClubName))
                return;

            List<Item> itemList = AllPlayersListsOfItemsDictionary[socialClubName];
            string serialized = JsonConvert.SerializeObject(itemList);
            NAPI.Chat.SendChatMessageToAll("Saved to inv");
        }


        public static void DropItem(Player player, Item droppedItem)
        {
            try
            {
                var random = new Random();
                droppedItem.IsActive = false;

                var xrnd = random.NextDouble();
                var yrnd = random.NextDouble();
                GTANetworkAPI.Object droppedItemObject = NAPI.Object.CreateObject(InventorySystem.ItemModelsDictionary[droppedItem.Type], player.Position +
                    InventorySystem.ItemsPosOffsetDictionary[droppedItem.Type] + new Vector3(xrnd, yrnd, 0), player.Rotation +
                    InventorySystem.ItemsRotOffsetDictionary[droppedItem.Type], 255, player.Dimension);
                random = null;

                // убераю из инветаря
                List<Item> playerItemsList = AllPlayersListsOfItemsDictionary[player.SocialClubName];
                playerItemsList.RemoveAt(FindItemKey(player.SocialClubName, droppedItem.Type));

                // запускаю таймер очистки и добавляю в список выкинутых предметов
                WholeDroppedItemsList.Add(droppedItem);
                DeleteDroppedItemTask(droppedItemObject, droppedItem);
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput("Ex: void DropItem, InventorySystem " + e.Message);
            }
        }
        public static void DeleteDroppedItemTask(GTANetworkAPI.Object obj, Item item)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    if (WholeDroppedItemsList.Contains(item)) // если он все ёще будет в листе выброшеных, удалится нахой
                    {
                        WholeDroppedItemsList.Remove(item);
                        obj.Delete();
                        item = null;
                    }
                    else return;
                }, delayTime: 10000);
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput("Ex: void DeleteDroppedItem, InventorySystem " + e.Message);
            }
        }


        public static bool IsInventoryFull(string SocialClubName)
        {
            if (AllPlayersListsOfItemsDictionary[SocialClubName].Count >= 20)
                return true;
            else
                return false;
        }
        public static void CheckForExist(string socialClubName)
        {
            string result = DBFunctions.GetStringFromBD(socialClubName, "inventory", "ownerSocialName", WHERE: "ownerSocialName");
            bool isPlayerTableExist = (result.Length == 0) ? false : true; // Проверка на существование таблы с сошлом,если есть то тру,нету фолс
            if (!isPlayerTableExist)
            {
                NAPI.Util.ConsoleOutput("Инвентаря нет,создаю");
                AllPlayersListsOfItemsDictionary.Add(socialClubName, new List<Item>());
                MySQLStatic.Query($"INSERT INTO `inventory`(`id`,`ownerSocialName`,`items`) " +
                    $"VALUES (NULL,'{socialClubName}','{JsonConvert.SerializeObject(new List<Item>())}')");
            }
            else
            {
                string serialized = DBFunctions.GetStringFromBD(socialClubName, "inventory", "items", WHERE: "ownerSocialName");
                List<Item> deserializedList = JsonConvert.DeserializeObject<List<Item>>(serialized);
                NAPI.Util.ConsoleOutput("Инвентарь есть");
                if (!AllPlayersListsOfItemsDictionary.ContainsKey(socialClubName))
                {
                    AllPlayersListsOfItemsDictionary.Add(socialClubName, deserializedList);
                }
            }
        }
        public static int FindItemKey(string SocialClubName, ItemType type)
        {
            List<Item> itemsList = AllPlayersListsOfItemsDictionary[SocialClubName];
            return itemsList.FindIndex(item => item.Type == type);
            // если не найдет item равый type в листе, то выдаст -1, иначе отдаст его номер в листе
        }


    }

    class Item
    {
        public string ItemSerial { get; set; } // serial of item
        public InventorySystem.ItemType Type { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
        public Item(InventorySystem.ItemType type, int quantity = 1, bool isActive = false)
        {
            Type = type;
            Quantity = quantity;
            IsActive = isActive;
            ItemSerial = CreateSerialForItem(type);
        }

        private string CreateSerialForItem(InventorySystem.ItemType type)
        {
            Random random = new Random();
            string serial = type.ToString() + "-";
            serial += (char)random.Next(0x0041, 0x005A); // 1 
            for (int i = 1; i <= 7; i++) // 8
            {
                serial += (char)random.Next(0x0030, 0x0039);
            }
            serial += (char)random.Next(0x0041, 0x005A); // 9
            return serial;
        }
    }
}
