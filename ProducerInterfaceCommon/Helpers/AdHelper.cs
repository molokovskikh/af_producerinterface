using System;
using System.DirectoryServices;

namespace ProducerInterfaceCommon.Helpers
{
    public class AdHelper
    {
        private DirectoryEntry entryAu;
        private string _path;
        private string _filterAttribute;
        public string ErrorMessageString;

        public bool IsAuthenticated(string username, string pwd)
        {
            if (Authenticated(@"LDAP://OU=ќфис,DC=adc,DC=analit,DC=net", username, pwd))
                return true;
            if (Authenticated(@"LDAP://OU= лиенты,DC=adc,DC=analit,DC=net", username, pwd))
                return true;
            return false;
        }
        public bool Authenticated(string LDAP, string username, string pwd)
        {
            var domainAndUsername = @"analit\" + username;
            entryAu = new DirectoryEntry(LDAP, domainAndUsername, pwd, AuthenticationTypes.None);
            try
            {
                // Bind to the native AdsObject to force authentication.
                var obj = entryAu.NativeObject;
                var search = new DirectorySearcher(entryAu);
                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");

                SearchResult result = search.FindOne();
                // Update the new path to the user in the directory
                _path = result.Path;
                _filterAttribute = (String)result.Properties["cn"][0];
            }
            catch (Exception ex)
            {
                //_log.Info("ѕароль или логин был введен неправильно");
                //_log.Info(ErrorMessage);
                ErrorMessageString = ex.Message;
                return false;
            }
            entryAu.RefreshCache();
            return true;
        }
}
}
