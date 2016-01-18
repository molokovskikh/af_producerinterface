using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProducerInterfaceControlPanelDomain.Models
{
    public class ListGroupView
    {
        public long Id { get; set; }
        public string NameGroup { get; set; }
        public string Description { get; set; }
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
        public int CountPermissions { get; set; }
        public string[] ListPermission { get; set; }
    }

    public partial class ProducerUser
    {
        public List<OptionElement> ListPermission { get; set; }

        [UIHint("LongListPermission")]
        public List<long> ListSelectedPermission { get; set; }
    }


    [MetadataType(typeof(ControlPanelGroupMetaData))]
    partial class ControlPanelGroup
    {
        [UIHint("LongListUser")]
        public List<long> ListUser { get; set; }

        [UIHint("LongListPermission")]
        public List<long> ListPermission{ get; set; }     
    }

    public class ControlPanelGroupMetaData
    {
        
          
    }
}