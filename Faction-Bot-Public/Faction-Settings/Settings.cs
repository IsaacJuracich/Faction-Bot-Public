using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faction_Bot_Public.Faction_Settings {
    public class Settings {
        public string d_prefix;
        public List<ulong> d_adminusers;
        public int d_cmdcooldown;
        public ulong d_serverchatID;
        public bool d_serverchatBot;
        public string m_email;
        public string m_password;
        public string m_logintype;
        public string m_serverip;
        public string m_version;
        public string m_renderdistance;
        public int m_cmdcooldown;
        public string m_chatformat;
        public string m_hubcmd;
        public bool premium;
        public List<Server_Socket.SocketUser> socketUser;
        public ulong d_ownerId;
        public Settings(string prefix, List<ulong> adminusers, int cooldown, ulong ownerId) {
            d_prefix = prefix;
            d_adminusers = adminusers;
            d_cmdcooldown = cooldown;
            d_serverchatBot = false;
            d_serverchatID = 0;
            m_email = null;
            m_password = null;
            m_logintype = null;
            m_serverip = null;
            m_version = null;
            m_renderdistance = null;
            m_cmdcooldown = 0;
            m_chatformat = null;
            m_hubcmd = null;
            premium = false;
            socketUser = new List<Server_Socket.SocketUser>();
            socketUser.Add(new Server_Socket.SocketUser(true, ownerId, ""));
            d_ownerId = ownerId;
        }
    }
}
