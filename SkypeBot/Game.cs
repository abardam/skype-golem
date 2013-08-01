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
        public int pots;

        public Player()
        {
            hp = 20;
            dmg = 5;
            stun = DateTime.Now;
            pots = 0;
        }
    }

    class Game
    {
        private Dictionary<string, Player> playerDic;
        private Form1 form;
        public IChat ichat;
        private Random r;


        public Game(Form1 f, IChat i)
        {
            playerDic = new Dictionary<string, Player>();
            form = f;
            ichat = i;
            r = new Random();
        }

        private void initPlayer(String p1)
        {
            if (!playerDic.ContainsKey(p1))
                playerDic[p1] = new Player();
        }


        public void Attack(string p1, string p2, string special = "")
        {
            initPlayer(p1);
            initPlayer(p2);

            if (playerDic[p1].hp <= 0)
            {
                form.say(p1 + " tries to attack " + p2 + ", but is too dead to do that.", ichat, "FIGHT");
            }
            else if (playerDic[p2].hp <= 0)
            {
                form.say(p1 + " tries to attack " + p2 + ", but " + p2 + " is already dead.", ichat, "FIGHT");
            }

            else if (playerDic[p1].stun.CompareTo(DateTime.Now) > 0)
            {
                form.say(p1 + " tries to attack " + p2 + ", but is stunned!", ichat, "FIGHT");
            }
            else
            {
                switch (special)
                {
                    case "boot":
                        form.say(p1 + " boots " + p2 + "'s head! " + p2 + " is stunned for 5 seconds!", ichat, "FIGHT");
                        playerDic[p2].stun = DateTime.Now.AddSeconds(5);
                        break;
                    default:
                        playerDic[p2].hp -= playerDic[p1].dmg;

                        form.say(p1 + " attacks " + p2 + " for " + playerDic[p1].dmg + "!", ichat, "FIGHT");
                        form.say(p2 + "[" + playerDic[p2].hp + "/20]", ichat, "FIGHT");

                        if (playerDic[p2].hp <= 0)
                        {
                            form.say(p1 + " loots the corpse!", ichat, "FIGHT");

                            int ran = r.Next(2);

                            if (ran == 0)
                            {
                                form.say(p1 + " finds a sword!", ichat, "FIGHT");
                                playerDic[p1].dmg += playerDic[p2].dmg;
                            }
                            else
                            {
                                form.say(p1 + " finds a health potion!", ichat, "FIGHT");
                                playerDic[p1].pots++;
                            }
                        }
                        break;
                }
            }
        }

        public void BadAttack(string p, string p2)
        {
            form.say(p + " tries to look for " + p2 + "..?", ichat, "FIGHT");
        }

        public void Potion(string p, string p_2 = "")
        {
            initPlayer(p);
            if (playerDic[p].pots <= 0)
            {
                form.say(p + " has no potions...", ichat, "FIGHT");
                return;
            }

            if (p_2 == "")
            {
                form.say(p + " uses a potion!", ichat, "FIGHT");
                playerDic[p].hp = 20;
            }
            else
            {
                initPlayer(p_2);
                form.say(p + " uses a potion on " + p_2 + "!", ichat, "FIGHT");
                playerDic[p_2].hp = 20;
            }

            --playerDic[p].pots;

        }
    }
}
