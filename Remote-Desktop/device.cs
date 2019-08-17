using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remote_Desktop
{
    class device
    {
        private int _num;
        private string _ip;
        private string _status;
        private string _username=string.Empty;
        private string _password=string.Empty;
        public int Num
        {
            get { return _num;}
            set { _num = value; }
        }
        public string Ip
        {
            set { _ip = value;}
            get { return _ip; }
        }
        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public device(int num,string ip,string status,string username,string password)
        {
            _num = num;
            _ip = ip;
            _status = status;
            _username = username;
            _password = password;
        }

        public device(device d)
        {
            _num = d.Num;
            _ip = d.Ip;
            _status = d.Status;
            _username = d.Username;
            _password = d.Password;
        }
    }
}
