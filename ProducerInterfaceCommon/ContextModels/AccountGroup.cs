//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProducerInterfaceCommon.ContextModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class AccountGroup
    {
        public AccountGroup()
        {
            this.AccountPermission = new HashSet<AccountPermission>();
            this.Account = new HashSet<Account>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Description { get; set; }
        public sbyte TypeGroup { get; set; }
    
        public virtual ICollection<AccountPermission> AccountPermission { get; set; }
        public virtual ICollection<Account> Account { get; set; }
    }
}