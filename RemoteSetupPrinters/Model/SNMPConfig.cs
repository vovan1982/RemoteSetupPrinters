namespace RemoteSetupPrinters.Model
{
    class SNMPConfig
    {
        private string _SNMPVER;
        private string _READCOMMUNITY;
        private int _TIMEOUT;
        private int _RETRIES;
        private int _PORT;
        private string _USER;
        private string _AUTHALGORITHM;
        private string _PASSAUTH;
        private string _PRIVACYALGORITHM;
        private string _PASSPRIVACY;
        private string _CONTEXTNAME;

        public SNMPConfig()
        {
            _SNMPVER = "2";
            _READCOMMUNITY = "public";
            _TIMEOUT = 2000;
            _RETRIES = 1;
            _PORT = 161;
        }

        public string CONTEXTNAME
        {
            get { return _CONTEXTNAME; }
            set { _CONTEXTNAME = value; }
        }

        public string PASSPRIVACY
        {
            get { return _PASSPRIVACY; }
            set { _PASSPRIVACY = value; }
        }

        public string PRIVACYALGORITHM
        {
            get { return _PRIVACYALGORITHM; }
            set { _PRIVACYALGORITHM = value; }
        }

        public string PASSAUTH
        {
            get { return _PASSAUTH; }
            set { _PASSAUTH = value; }
        }

        public string AUTHALGORITHM
        {
            get { return _AUTHALGORITHM; }
            set { _AUTHALGORITHM = value; }
        }

        public string USER
        {
            get { return _USER; }
            set { _USER = value; }
        }

        public int PORT
        {
            get { return _PORT; }
            set { _PORT = value; }
        }

        public int RETRIES
        {
            get { return _RETRIES; }
            set { _RETRIES = value; }
        }

        public int TIMEOUT
        {
            get { return _TIMEOUT; }
            set { _TIMEOUT = value; }
        }

        public string READCOMMUNITY
        {
            get { return _READCOMMUNITY; }
            set { _READCOMMUNITY = value; }
        }

        public string SNMPVER
        {
            get { return _SNMPVER; }
            set { _SNMPVER = value; }
        }
    }
}
