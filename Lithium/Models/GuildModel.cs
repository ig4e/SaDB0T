﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lithium.Models
{
    public class GuildModel
    {
        public List<Guild> Guilds { get; set; }= new List<Guild>();
        public class Guild
        {
            public ulong GuildID { get; set; }
            public Moderation ModerationSetup { get; set; } = new Moderation();
            public settings Settings { get; set; } = new settings();
            public autochannels AutoMessage { get; set; } = new autochannels();
            public antispams Antispam { get; set; } = new antispams();

            public class Moderation
            {
                public List<ulong> ModeratorRoles { get; set; } = new List<ulong>();
                public List<ulong> AdminRoles { get; set; } = new List<ulong>();
                public List<kick> Kicks { get; set; } = new List<kick>();
                public List<warn> Warns { get; set; } = new List<warn>();
                public List<ban> Bans { get; set; } = new List<ban>();
                public msettings Settings { get; set; } = new msettings();
                public class msettings
                {
                    //Warnings before doing a specific action.
                    public int warnlimit { get; set; } = int.MaxValue;
                    public warnLimitAction WarnLimitAction { get; set; } = warnLimitAction.NoAction;
                    public enum warnLimitAction
                    {
                        NoAction,
                        Kick,
                        Ban
                    }
                }

                public class muted
                {
                    public ulong mutedrole { get; set; } = 0;
                    public List<muteduser> MutedUsers { get; set; } = new List<muteduser>();
                    public class muteduser
                    {
                        public ulong userid { get; set; }
                        public bool expired { get; set; } = false;
                        public DateTime expiry { get; set; } = DateTime.UtcNow;
                    }
                }

                public class kick
                {
                    public ulong userID { get; set; }
                    public string username { get; set; }
                    public string reason { get; set; }

                    public string modname { get; set; }
                    public ulong modID { get; set; }
                }
                public class warn
                {
                    public ulong userID { get; set; }
                    public string username { get; set; }
                    public string reason { get; set; }

                    public string modname { get; set; }
                    public ulong modID { get; set; }
                }
                public class ban
                {
                    public ulong userID { get; set; }
                    public string username { get; set; }
                    public string reason { get; set; }

                    public string modname { get; set; }
                    public ulong modID { get; set; }

                    public bool Expires { get; set; } = false;
                    public DateTime ExpiryDate { get; set; } = DateTime.MaxValue;
                }
            }

            public class settings
            {
                public string Prefix { get; set; } = Config.Load().DefaultPrefix;
                public visibilityconfig DisabledParts { get; set; } = new visibilityconfig();
                public class visibilityconfig
                {
                    public List<string> BlacklistedModules { get; set; } = new List<string>();
                    public List<string> BlacklistedCommands { get; set; } = new List<string>();
                }
            }

            public class autochannels
            {
                public List<autochannel> AutoChannels { get; set; } = new List<autochannel>();
                public class autochannel
                {
                    public acsettings Settings { get; set; } = new acsettings();
                    public class acsettings
                    {
                        public bool enabled { get; set; } = false;
                        public int msgcount { get; set; } = 0;
                        public int sendlimit { get; set; } = 50;
                    }

                    public ulong channelID { get; set; }

                    public string title { get; set; } = "AutoMessage";
                    public string automessage { get; set; } = "PassiveBOT";
                    public string ImgURL { get; set; } = null;
                }
            }

            public tags Tags { get; set; } = new tags();
            public class tags
            {
                public tsettings Settings { get; set; } = new tsettings();
                public class tsettings
                {
                    public bool AllowAllUsersToCreate { get; set; } = false;
                }
                public List<tag> Tags = new List<tag>();
                public class tag
                {
                    public string name { get; set; }
                    public string content { get; set; }
                    public string imgURL { get; set; } = null;
                    public int uses { get; set; } = 0;
                    public ulong ownerID { get; set; }
                }
            }

            public class antispams
            {
                public blacklist Blacklist { get; set; } = new blacklist();
                public antispam Antispam { get; set; } = new antispam();
                public advertising Advertising { get; set; } = new advertising();
                public mention Mention { get; set; } = new mention();
                public privacy Privacy { get; set; } = new privacy();
                public toxicity Toxicity { get; set; } = new toxicity();

                public List<IgnoreRole> IgnoreRoles { get; set; } = new List<IgnoreRole>();

                public class toxicity
                {
                    public bool WarnOnDetection { get; set; } = false;
                    public bool UsePerspective { get; set; } = false;
                    public int ToxicityThreshHold { get; set; } = 90;
                }


                public class antispam
                {
                    //remove repetitive messages and messages posted in quick succession
                    public bool NoSpam { get; set; } = false;

                    //words to skip while using antispam
                    public List<string> AntiSpamSkip { get; set; } = new List<string>();

                    //Toggle wether or not to use antispam on bot commands
                    public bool IgnoreCommandMessages { get; set; } = true;

                    public bool antiraid { get; set; } = false;
                    public bool WarnOnDetection { get; set; } = false;
                }

                public class blacklist
                {
                    //the blacklist word groupings
                    public List<BlacklistWords> BlacklistWordSet { get; set; } = new List<BlacklistWords>();
                    public string DefaultBlacklistMessage { get; set; } = "";

                    public bool WarnOnDetection { get; set; } = false;

                    public class BlacklistWords
                    {
                        //Words for the specified blacklist message
                        public List<string> WordList { get; set; } = new List<string>();

                        //Custom response for certain words.
                        public string BlacklistResponse { get; set; } = null;
                    }
                }

                public class advertising
                {
                    public string NoInviteMessage { get; set; } = null;

                    //blacklist for discord invites
                    public bool Invite { get; set; } = false;
                    public bool WarnOnDetection { get; set; } = false;
                }

                public class mention
                {
                    public string MentionAllMessage { get; set; } = null;

                    //blacklist for @everyone and @here 
                    public bool MentionAll { get; set; } = false;

                    //Remove 5+ mentions of roles or users
                    public bool RemoveMassMention { get; set; } = false;
                    public bool WarnOnDetection { get; set; } = false;
                }

                public class privacy
                {
                    //remove all ip addresses posted in the format x.x.x.x
                    public bool RemoveIPs { get; set; } = false;
                    public bool WarnOnDetection { get; set; } = false;
                }

                public class IgnoreRole
                {
                    public ulong RoleID { get; set; }

                    //false = filter
                    //true = bypass filter
                    public bool AntiSpam { get; set; } = false;
                    public bool Blacklist { get; set; } = false;
                    public bool Advertising { get; set; } = false;
                    public bool Mention { get; set; } = false;
                    public bool Privacy { get; set; } = false;
                    public bool Toxicity { get; set; } = false;
                }
            }

        }
    }
}