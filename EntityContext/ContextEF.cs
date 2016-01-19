using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace EntityContext
{
    public class ContextEF : EntityContext.ContextModels.producerinterface_Entities
    {
        public override int SaveChanges()
        {
            foreach (ObjectStateEntry entry in (this as IObjectContextAdapter).ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted))
            {
                // Validate the objects in the Added and Modified state
                // if the validation fails throw an exeption.




            }

            return base.SaveChanges();
        }

    }
}
