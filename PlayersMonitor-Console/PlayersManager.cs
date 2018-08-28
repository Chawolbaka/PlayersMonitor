﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace PlayersMonitor
{
    public class PlayersManager
    {
        public delegate void PlayerJoinedEvntHandler(Player player);
        public delegate void PlayerDisconnectedEvntHandler(Player player);
        public event PlayerDisconnectedEvntHandler PlayerDisconnectedEvent;
        public event PlayerJoinedEvntHandler PlayerJoinedEvnt;

        public bool? IsOnlineMode;

        private Configuration Config;
        private List<Player> PlayersList = new List<Player>();

        public PlayersManager(Configuration config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }
        public void Add(string name, Guid uuid)
        {
            int DefPlayerBlood = PlayersList.Count < 12 ? 2 : Config.Blood; 
            if (Config == null)
                throw new Exception("Not Initializtion");
            Player FoundPlayer = PlayersList.Find(x => x.Uuid.ToString().Replace("-","") == uuid.ToString().Replace("-", ""));
            if (FoundPlayer != null) //如果找到了这个玩家就把它的血恢复到默认值(回血)
            {
                FoundPlayer.Blood = DefPlayerBlood;
                //Screen.ReviseLineField($"{GetBloodColor(FoundPlayer.Blood,Config.Blood)}{FoundPlayer.Blood.ToString("D2")}",3,FoundPlayer.ScreenTag);
            }
            else if (FoundPlayer == null)
            {
                Player NewPlayer = new Player(name, Guid.Parse(uuid.ToString()), DefPlayerBlood);
                if (PlayersList.Count == 0) {
                    Thread t = new Thread(iom => IsOnlineMode = NewPlayer.IsOnlineMode());
                    t.Start();
                }

                //格式:[玩家索引/玩家剩余生命]Name:玩家名(UUID)
                NewPlayer.ScreenTag = Screen.CreateLine(
                    "[", (PlayersList.Count + 1).ToString("D2"), "/", $"{GetBloodColor(NewPlayer.Blood-1, Config.Blood)}{(NewPlayer.Blood-1).ToString("D2")}", "]",
                    "Name:", NewPlayer.Name, "(", NewPlayer.Uuid.ToString(), ")");
                PlayersList.Add(NewPlayer);
                PlayerJoinedEvnt?.Invoke(NewPlayer);
            }
            //LifeTimer();
        }
        public void LifeTimer()
        {
            for (int i = 0; i < PlayersList.Count; i++)
            {
                PlayersList[i].Blood--;

                if (PlayersList[i].Blood == 0)
                {
                    Player PlayerTmp = PlayersList[i];
                    PlayersList.Remove(PlayersList[i--]);
                    //从屏幕上移除这个玩家&修改其它玩家的序号(屏幕上的)
                    Screen.RemoveLine(PlayerTmp.ScreenTag, true);
                    if (PlayersList.Count > 0)
                    {
                        for (int j = 0; j < PlayersList.Count; j++)
                        {
                            Screen.ReviseField((j + 1).ToString("D2"), 1, PlayersList[j].ScreenTag);
                        }
                    }
                    PlayerDisconnectedEvent?.Invoke(PlayerTmp);
                }
                else
                {
                    Screen.ReviseField(
                        $"{GetBloodColor(PlayersList[i].Blood, Config.Blood)}{PlayersList[i].Blood.ToString("D2")}", 3, PlayersList[i].ScreenTag);
                }
            }
        }
        private void RestoreHealthForAllPlayer(int? blood=null)
        {
            foreach (var Player in PlayersList)
            {
                Player.Blood = blood ?? Config.Blood;
                Screen.ReviseField($"&a{Player.Blood.ToString("D2")}", 3, Player.ScreenTag);
            }
        }
        private string GetBloodColor(int nowBlood,int maxBlood)
        {
            if (PlayersList.Count <= 12)
                return "&a";
            if (nowBlood <= 1 || nowBlood <= maxBlood / 100.0f * 30)
                return "&c";
            else if (nowBlood <= maxBlood / 100.0f * 48)
                return "&e";
            else
                return "&a";
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var player in PlayersList)
            {
                sb.Append($"{player.ToString()}{Environment.NewLine}");
                sb.Append("---------------------------------");
            }
           return sb.ToString();
        }
        public class Player
        {
            private bool? HasBuyGame=null;
            private bool OnlineMode;
            public Guid Uuid { get; set; }
            public string Name { get; set; }
            public int Blood { get; set; }
            public string ScreenTag { get; set; }

            public Player()
            {

            }
            public Player(string name, Guid uuid,int blood)
            {
                this.Name = name;
                this.Uuid = uuid;
                this.Blood = blood;
            }
            
            
            /// <summary>
            /// Need Network
            /// </summary>
            public bool? IsOnlineMode()
            {
                // GET https://api.mojang.com/users/profiles/minecraft/<username>
                // 通过这个API获取玩家的UUID,然后和Ping返回的UUID匹配如果不一样的话就是离线模式了
                if (Uuid != null && !string.IsNullOrWhiteSpace(Name))
                {
                    //缓存
                    if (HasBuyGame != null && HasBuyGame==true)
                        return OnlineMode;
                    else if (HasBuyGame != null && HasBuyGame != true)
                        return false;
                    //没有缓存的话就去通过API获取
                    try
                    {
                        WebClient wc = new WebClient();
                        string html = Encoding.UTF8.GetString(wc.DownloadData(
                            @"https://api.mojang.com/users/profiles/minecraft/" + Name));
                        HasBuyGame = !string.IsNullOrWhiteSpace(html);
                        if (HasBuyGame !=true)
                            return null;
                        OnlineMode = html.Contains(Uuid.ToString().Replace("-", string.Empty));
                        return OnlineMode;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                    throw new ArgumentNullException("Uuid or Name");
            }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"PlayerName:{Name}{Environment.NewLine}");
                sb.Append($"uuid:{Uuid.ToString()}{Environment.NewLine}");
                sb.Append($"Blood:{Blood}{Environment.NewLine}");
                sb.Append($"ScreenTag:{ScreenTag}{Environment.NewLine}");
                return sb.ToString();
            }
        }
    }

}
