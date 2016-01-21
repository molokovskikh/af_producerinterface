﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.ContextModels
{

 

    [MetadataType(typeof(JobExtendMetaData))]
    public partial class jobextend
    {
        public Reports ReportTypeEnum
        {
            get { return (Reports)ReportType; }
            set { ReportType = (int)value; }
        }

        public DisplayStatus DisplayStatusEnum
        {
            get { return (DisplayStatus)DisplayStatus; }
            set { DisplayStatus = (int)value; }
        }

    }

    public class JobExtendMetaData
    {
        [ScaffoldColumn(false)]
        public string SchedName { get; set; }

        [ScaffoldColumn(false)]
        public string JobName { get; set; }

        [ScaffoldColumn(false)]
        public string JobGroup { get; set; }

        [Display(Name = "Название")]
        public string CustomName { get; set; }

        [Display(Name = "Расписание")]
        public string Scheduler { get; set; }

        [ScaffoldColumn(false)]
        public int ReportType { get; set; }

        [Display(Name = "Тип и Параметры")]
        public Reports ReportTypeEnum { get; }

        [ScaffoldColumn(false)]
        public long ProducerId { get; set; }

        [Display(Name = "Создатель")]
        public string Creator { get; set; }

        [ScaffoldColumn(false)]
        public System.DateTime CreationDate { get; set; }

        //[Display(Name = "Последние изменения")]
        [ScaffoldColumn(false)]
        public System.DateTime LastModified { get; set; }

        [ScaffoldColumn(false)]
        public int DisplayStatus { get; set; }

        [Display(Name = "Статус")]
        public DisplayStatus DisplayStatusEnum { get; }

        [Display(Name = "Запуск")]
        public System.DateTime LastRun { get; set; }

        [ScaffoldColumn(false)]
        public bool Enable { get; set; }
    }

   


 

    public partial class ProducerUser
    {
        [Display(Name = "Списко прав")]
        [UIHint("LongList")]
        public List<long> UserPermission { get; set; }

        public List<OptionElement> ListPermission { get; set; }

        [UIHint("LongListPermission")]
        public List<long> ListSelectedPermission { get; set; }
    }
    public partial class ControlPanelGroup
    {
        public List<int> ListPermission { get; set; }
        public List<long> ListUser { get; set; }
    }
       
    public partial class promotions
    {
        public List<OptionElement> GlobalDrugList { get; set; }
    }
 
    [MetadataType(typeof(promotionsMetaData))]
    public partial class promotions
    {

    }
    public class promotionsMetaData
    {
        public long Id { get; set; }
        public System.DateTime UpdateTime { get; set; }
  
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

        public virtual ICollection<promotionToDrug> promotionToDrug { get; set; }

    } 
}