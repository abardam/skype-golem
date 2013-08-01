using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKYPE4COMLib;

namespace SkypeBot
{
    class Player
    {
        public int hp;
        public int dmg;
        public DateTime stun;

        public Player()
        {
            hp = 20;
            dmg = 5;
            stun = DateTime.Now;
        }
    }

    class Game
    {
        private Dictionary<string, Player> playerDic;
        private Form1 form;
        private IChat ichat;

        public Game(Form1 f, IChat i)
        {
            playerDic = new Dictionary<string, Player>();
            form = f;
            ichat = i;
        }

        public void Attack(string p1, string p2, string special = "")
        {
            if (!playerDic.ContainsKey(p1))
                playerDic[p1] = new Player();

            if (!playerDic.ContainsKey(p2))
                playerDic[p2] = new Player();

            if (playerDic[p1].hp <= 0)
            {
                form.say(p1 + " tries to attack " + p2 + ", but is too dead to do that.", ichat, "FIGHT");
            }
            else if (playerDic[p1].stun.CompareTo(DateTime.Now) > 0)
            {
                form.say(p1 + " tries to attack " + p2 + ", but is stunned!", ichat, "FIGHT");
            }
            else
            {

                if (special == "boot")
                {
                    form.say(p1 + " boots " + p2 + "'s head! " + p2 + " is stunned for 5 seconds!", ichat, "FIGHT");
                    playerDic[p2].stun = DateTime.Now.AddSeconds(5);
                }
                else
                {

                    playerDic[p2].hp -= playerDic[p1].dmg;

                    form.say(p1 + " attacks " + p2 + " for " + playerDic[p1].dmg + "!", ichat, "FIGHT");
                    form.say(p2 + "[" + playerDic[p2].hp + "/20]", ichat, "FIGHT");
                }
            }
        }

        public void BadAttack(string p)
        {
            form.say(p + " swings at air..?", ichat, "FIGHT");
        }
    }
}
