using System;
using System.Linq;
using Common.Tools;
using NHibernate;
using NHibernate.Linq;

namespace ProducerInterfaceCommon.Models
{

    public class DrugFormPicture
    {
	    public virtual uint Id { get; set; }
	    public virtual uint ProducerId { get; set; }
	    public virtual uint CatalogId { get; set; }
	    public virtual int? PictureKey { get; set; }
    }
}