using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.Permission
{
    public class ListGroupView
    {
        public int Id { get; set; }
        public int CountUser { get; set; }
        public string Description { get; set; }
        public string NameGroup { get; set; }

        public string[] Users { get; set; }

        public string[] Permissions { get; set; }
        public List<UsersViewInChange> ListUsersInGroup { get; set; }
    }

    public class UsersViewInChange
    {
        public string Name { get; set; }
        public string eMail { get; set; }
        public string ProducerName { get; set; }
    }
}
