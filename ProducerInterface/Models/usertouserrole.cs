//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProducerInterface.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class usertouserrole
    {
        public Nullable<long> ProducerUserId { get; set; }
        public Nullable<long> UserRoleId { get; set; }
        public Nullable<long> UserPermissionId { get; set; }
        public long Id { get; set; }
    
        public virtual produceruser produceruser { get; set; }
        public virtual produceruser produceruser1 { get; set; }
        public virtual userpermission userpermission { get; set; }
        public virtual userpermission userpermission1 { get; set; }
        public virtual userrole userrole { get; set; }
    }
}