using System;
using System.Linq;
using Common.Tools;
using NHibernate;
using NHibernate.Linq;

namespace ProducerInterfaceCommon.Models
{
     
    public class Slide
    { 
        public virtual uint Id { get; set; }
        
        public virtual string Url { get; set; }
        
        public virtual int? ImagePath { get; set; }
        
        public virtual DateTime? LastEdit { get; set; }
        
        public virtual bool Enabled { get; set; }
        
        public virtual uint? PositionIndex { get; set; }

        public virtual Slide UpdateAndGetIfExists(ISession dbSession)
        {
            if (Id!= 0) {
                var dbElement = dbSession.Query<Slide>().FirstOrDefault(s => s.Id == Id);
                if (dbElement!=null) {
                    dbElement.Url = Url;
                    dbElement.ImagePath = ImagePath.HasValue ? ImagePath : dbElement.ImagePath;
                    dbElement.LastEdit = SystemTime.Now();
                    dbElement.Enabled = Enabled; 
                    return dbElement;
                }
            }
            return this;
        }
    }
}