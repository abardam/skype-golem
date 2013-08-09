using System;
using System.Windows.Forms;
using SKYPE4COMLib; // Our COM library
using System.Collections.Generic;

namespace SkypeBot
{
    public partial class Form1 : Form
    {
        private Skype skype;
        private const string trigger = "!"; // Say !help
        private const string nick = "BOT";
        //private Dictionary<string, Game> gameDic;
        private Game game;

        private IChat log;

        public Form1()
        {
            InitializeComponent();

            //gameDic = new Dictionary<string, Game>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            skype = new Skype();
            // Use skype protocol version 7 
            skype.Attach(7, false);
            // Listen 
            skype.MessageStatus +=
              new _ISkypeEvents_MessageStatusEventHandler(skype_MessageStatus);
        }
        private void skype_MessageStatus(ChatMessage msg,
                     TChatMessageStatus status)
        {
            Console.WriteLine(status);
            if (status.ToString() != "cmsReceived" && status.ToString() != "cmsSending")
            {
                return;
            }

            

            richTextBox1.AppendText(msg.FromDisplayName+": "+msg.Body + "\n");
            // Proceed only if the incoming message is a trigger
            if (msg.Body.IndexOf(trigger) == 0 )
            {
                // Remove trigger string and make lower case
                string command = msg.Body.Remove(0, trigger.Length).ToLower();
                
                // Send processed message back to skype chat window
                IChat ichat = skype.get_Chat(msg.Chat.Name);

                String msg2 = ProcessCommand(command, ichat, msg);

                if(msg2.Length > 0)
                    ichat.SendMessage(nick + ": " + msg2);


                msg.Seen = true;
            }

        }

        public void say(String msg, IChat ichat, String cat = "")
        {
            if (cat.Length > 0)
            {
                cat = "[" + cat + "]";
            }

            ichat.SendMessage(nick + cat + ": " + msg);
        }

        private string ProcessCommand(string str, IChat ichat, ChatMessage msg)
        {
            string result = "";
            switch (str.Split(' ')[0])
            {
                case "roll":
                    try
                    {
                        /*String[] a = str.Split(' ', '+');
                        String[] b = a[1].Split('d');
                        result = RollDice(Int32.Parse(b[0]),Int32.Parse(b[1]));*/
                        result = parseDice(str);
                    }
                    catch (Exception e)
                    {
                        result = "nice try";
                    }
                    break;
                case "attack":

                    parseAttack(str, ichat, msg);

                    break;
                case "potion":
                    parsePotion(str, ichat, msg);
                    break;
                case "log":
                    /*
                    foreach (KeyValuePair<string, Game> g in gameDic)
                    {
                        g.Value.ichat = ichat;
                    }*/
                    game.ichat = ichat;

                    break;
                case "cast":
                    parseCast(str, ichat, msg);
                    break;
                case "invite":
                    /*
                    if (gameDic.ContainsKey(ichat.Topic))
                    {
                        gameDic[ichat.Topic].ichat.AddMembers(ichat.Members);
                    }*/

                    game.ichat.AddMembers(ichat.Members);
                    break;

                default:
                    result = "";
                    break;
            }

            return result;
        }

        private void initGame(IChat ichat)
        {
            /*
            if (!gameDic.ContainsKey(ichat.Topic))
            {
                if(log == null)
                    gameDic[ichat.Topic] = new Game(this, ichat);
                else
                    gameDic[ichat.Topic] = new Game(this, log);


            }*/

            say("Golem starting up", ichat);

            if (game == null)
            {
                game = new Game(this, ichat);
            }

        }

        private void parsePotion(string str, IChat ichat, ChatMessage msg)
        {
            initGame(ichat);
            
            string[] strs = str.Split(' ');

            if (strs.Length > 1)
            {
                User target = findPlayer(strs[1], ichat);
                
                if (target == null)
                {

                    //gameDic[ichat.Topic].BadAttack(msg.Sender.Handle, strs[1]);
                    game.BadAttack(msg.Sender.Handle, strs[1]);
                    return;
                }

                //gameDic[ichat.Topic]
                game.Potion(msg.Sender.Handle, target.Handle);
            }
            else
            {
                //gameDic[ichat.Topic]
                game.Potion(msg.Sender.Handle);
            }
        }

        private void parseCast(string str, IChat ichat, ChatMessage msg)
        {
            initGame(ichat);

            string[] strs = str.Split(' ');

            if (strs.Length <= 1) // "cast"
            {
                return;
            }
            else
            {
                if (strs.Length == 2) // "cast light"
                {
                    //gameDic[ichat.Topic]
                    game.Cast(msg.Sender.Handle, "", strs[1]);

                }
                else // "cast noizde fireball large"
                {
                    User target = findPlayer(strs[1], ichat);

                    if (target == null)
                    {
                        //gameDic[ichat.Topic]
                        game.BadAttack(msg.Sender.Handle, strs[1]);
                        return;
                    }

                    //gameDic[ichat.Topic]
                    game.Cast(msg.Sender.Handle, target.Handle, strs[2]);
                }
            }


        }

        private String parseAttack(string str, IChat ichat, ChatMessage msg)
        {
            initGame(ichat);

            string[] strs = str.Split(' ');

            if (strs.Length > 1)
            {

                User target = findPlayer(strs[1], ichat);

                if (target == null)
                {

                    //gameDic[ichat.Topic]
                    game.BadAttack(msg.Sender.Handle, strs[1]);
                    return "";
                }

                if (strs.Length > 2)
                {
                    //gameDic[ichat.Topic]
                    game.Attack(msg.Sender.Handle, target.Handle, strs[2]);
                }
                else
                {
                    //gameDic[ichat.Topic]
                    game.Attack(msg.Sender.Handle, target.Handle);
                }
            }

            return "";
        }

        private User findPlayer(string s, IChat ichat)
        {
            foreach(User user in ichat.Members){
                if (user.Handle.Contains(s))
                {
                    return user;
                }                
            }

            foreach (User user in ichat.Members)
            {
                if (user.DisplayName.Contains(s))
                {
                    return user;
                }
            }

            foreach (User user in ichat.Members)
            {
                if (user.FullName.Contains(s))
                {
                    return user;
                }
            }

            return null;
        }

        private String parseDice(String s)
        {
            s = s + " ";
            List<int> results = new List<int>();
            int state = 0;
            int curr = 4; //"roll" length
            char next;
            String currnum = "";
            int numdie = 0;
            int negat = 1;

            Random r = new Random();
            bool sex = false;

            String label = "";

            while (curr < s.Length)
            {
                next = s[curr];
                //richTextBox1.AppendText("staate " + state+ " next " + next);
                switch (state)
                {
                    case 0:
                        if (next >= '0' && next <= '9')
                        {
                            currnum = "" + next;
                            numdie = 0;
                            state = 1; //number
                        }
                        else if (next == ' ')
                        {
                        }
                        else
                        {
                            throw new Exception();
                        }
                        break;
                    case 1:
                        if (next >= '0' && next <= '9')
                        {
                            currnum += next;
                            state = 1; //number
                        }
                        else if (next == 'd')
                        {
                            numdie = Int32.Parse(currnum);

                            if (numdie == 69)
                            {
                                sex = true;
                            }

                            currnum = "";
                            state = 2; //die size
                        }
                        else if (next == '+' || next == '-' || next == ' ')
                        {
                            int adds = Int32.Parse(currnum);
                            if (adds == 69)
                            {
                                sex = true;
                            }
                            results.Add(negat * adds);

                            currnum = "0";
                            if (next == '+')
                            {
                                state = 0;
                                negat = 1;
                            }
                            else if (next == '-')
                            {
                                state = 0;
                                negat = -1;
                            }
                            else if (next == ' ')
                            {
                                state = 3; //op
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                        break;
                    case 2:
                        if (next >= '0' && next <= '9')
                        {
                            currnum += next;
                            state = 2; 
                        }
                        else if (next == '+' || next == '-' || next == ' ')
                        {
                            int size = Int32.Parse(currnum);

                            if (size == 69)
                            {
                                sex = true;
                            }

                            currnum = "0";
                            for (int i = 0; i < numdie; ++i)
                            {
                                results.Add(negat * r.Next(1, size+1));
                            }

                            if (next == '+')
                            {
                                numdie = 0;
                                state = 0;
                                negat = 1;
                            }
                            else if (next == '-')
                            {
                                numdie = 0;
                                state = 0;
                                negat = -1;
                            }
                            else if (next == ' ')
                            {
                                numdie = 0;
                                state = 3; //op
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                        break;

                    case 3:
                        if (next == '+')
                        {
                            state = 0;
                            negat = 1;
                        }
                        else if (next == '-')
                        {
                            state = 0;
                            negat = -1;
                        }
                        else if (next == ' ')
                        { }
                        else
                        {
                            label += next;
                            state = 4;
                        }
                        break;
                    case 4:
                        label += next;
                        break;
                }

                ++curr;
            }


            if (sex == true)
            {
                return "sex [sex]";
            }

            label = label.Trim();

            int res = 0;
            String outs = " [";

            for (int i = 0; i < results.Count; ++i)
            {
                res += results[i];

                if (results.Count < 100)
                {
                    outs += results[i];

                    if (i < results.Count - 1)
                    {
                        outs += ", ";
                    }
                }
            }

            if (results.Count >= 100)
            {
                outs += "alot";
            }

            outs = res + outs + "]";

            if(label.Length > 0)
                outs += "(" + label + ")";

            return outs;
        }

        private String RollDice(int num, int size)
        {
            String ret = "[";
            int rum = 0;
            Random r = new Random();
            int temp;
            for (int i = 0; i < num; i++)
            {
                temp = r.Next(1, size);
                rum += temp;
                ret += temp;

                if (i < num - 1)
                {
                    ret += ", ";
                }
            }

            ret += "]";

            return rum + " "+ret;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*foreach(KeyValuePair<string, Game> g in gameDic){
                foreach (User u in g.Value.ichat.Members)
                {
                    g.Value.ichat.Kick(u.Handle);
                    g.Value.ichat.Leave();
                }
            }*/
        }
    }
}