using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProducerInterface.Models
{
    public class Producer
    {
        public virtual string Name { get; set; }
        public virtual IList<produceruser> Users { get; set; }
        public virtual List<Drug> Drugs { get; set; }
    }

    public class Drug
    {
        // drug NAME table assortment 
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual MNN MNN { get; set; }
        public virtual DateTime UpdateTime { get; set; }
    }
    public class MNN
    {
        public virtual string Value { get; set; }
        public virtual string RussianValue { get; set; }
        public virtual DateTime UpdateTime { get; set; }
    }

}