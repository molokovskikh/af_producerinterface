using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProducerInterfaceControlPanelDomain.Models
{
    public class ListGroupView
    {
        public long Id { get; set; }
        public string NameGroup { get; set; }
        public int CountUser { get; set; }
        public string[] Users { get; set; }
        public string[] Permissions { get; set; }
    }
    public class ListUserView
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int CountGroup { get; set; }
        public string[] Groups { get; set; }
    }
}