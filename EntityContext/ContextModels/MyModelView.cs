using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityContext.ContextModels
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

    public class SearchPromotion
    {
        [Required(ErrorMessage = "Не выбран производитель")]
        public long IdProducer { get; set; }
    }

    public partial class promotions
    {
        public List<OptionElement> GlobalDrugList { get; set; }
    }

    public partial class ProducerUser
    {
        public List<OptionElement> ListPermission { get; set; }

        [UIHint("LongListPermission")]
        public List<long> ListSelectedPermission { get; set; }
    }

   [MetadataType(typeof(promotionsMetaData))]
    public partial class promotions
    {

    }
    public class promotionsMetaData
    {
        public long Id { get; set; }
        public System.DateTime UpdateTime { get; set; }
        public bool Enabled { get; set; }
        public Nullable<long> AdminId { get; set; }
        public long ProducerId { get; set; }
        public long ProducerUserId { get; set; }
        public string Annotation { get; set; }
        public string PromoFile { get; set; }
        public bool AgencyDisabled { get; set; }
        public string Name { get; set; }
        public decimal RegionMask { get; set; }


        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> Begin { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> End { get; set; }

        public bool Status { get; set; }

        public virtual ProducerUser ProducerUser { get; set; }
        public virtual ProducerUser ProducerUser1 { get; set; }
        public virtual ICollection<promotionToDrug> promotionToDrug { get; set; }

    }



    [MetadataType(typeof(promotionsGroupMetaData))]
    partial class ControlPanelGroup
    {
        [UIHint("LongListUser")]
        public List<long> ListUser { get; set; }

        [UIHint("LongListPermission")]
        public List<long> ListPermission { get; set; }
    }

    public class promotionsGroupMetaData
    {


    }

    public class SearchProducerReportsModel
    {
        [UIHint("EditorProducer")]
        [Required(ErrorMessage = "Выберите компанию")]
        [Display(Name = "Название компании производителя: ")]
        public long Producers { get; set; }
    }
}
