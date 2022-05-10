using GTANetworkAPI;
using System;

namespace WeiZed.MySQL
{
    class CharacterData
    {
        public string SocialClubName { get; set; } = null;
        public string firstName { get; set; } = null;
        public string lastName { get; set; } = null;
        public int adminLVL { get; set; } = 0;
        public int cashMoney { get; set; } = 500;
        public int bankMoney { get; set; } = 0;
        public int bankAcc_ID { get; set; }
        public int lvl { get; set; } = 0;
        public int exp { get; set; } = 0;
        public int fraction_ID { get; set; } = -1;
        public int fractionLVL { get; set; } = -1;
        public Vector3 spawnPosition { get; set; } = new Vector3(-1517.5931, -922.0931, 10.147163);
        public Vector3 spawnRotation { get; set; } = new Vector3(0, 0, 123.20353);
        public int spawnDimension { get; set; }
        public int warns { get; set; } = 0;
        public string unwarnDate { get; set; } //= DateTime.Now;  // returns string (example = "2021-10-03 20:24:38")
        public bool isVoiceMuted = false;
        public int hp { get; set; } = 100;
        public int biz_ID { get; set; } = -1;
        public int ownedHouse_ID { get; set; } = -1;
        public int sim { get; set; } = -1;
        public int arrestTime { get; set; } = 0;
        public int wantedLVL { get; set; } = 0;
        public string lastVeh { get; set; } = null;
        public int work_ID { get; set; } = -1;
        public string gender { get; set; } = "nogender";
        public string promo { get; set; } = null;
        public string birthDate { get; set; } = null;
        public string createDate { get; set; } = Convert.ToString(DateTime.Now); // returns string (example = "2021-10-03 20:24:38")



        // temperory data
        public bool isAlive = false;
    }
}
