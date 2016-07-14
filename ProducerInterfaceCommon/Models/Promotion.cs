using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceCommon.Models
{
	public enum ActualPromotionStatus
	{
		[Display(Name = "������� �������������")]
		NotConfirmed = 0,
		[Display(Name = "�������� ���� ������ ����������")]
		ConfirmedNotBegin = 1,
		[Display(Name = "���������")]
		ConfirmedEnded = 2,
		[Display(Name = "������������")]
		Active = 3,
		[Display(Name = "��������� �������������")]
		Disabled = 4,
		[Display(Name = "�����")]
		All = 5,
		[Display(Name = "��������� ���������������")]
		Rejected = 6
	}

	public enum PromotionStatus
	{
		[Display(Name = "������� �������������")]
		New = 0,
		[Display(Name = "������������")]
		Confirmed = 1,
		[Display(Name = "���������")]
		Rejected = 2
	}


	public class promotionsMetaData
	{
		public long Id { get; set; }
		public DateTime UpdateTime { get; set; }

		public long? AdminId { get; set; }
		public long ProducerId { get; set; }
		public long ProducerUserId { get; set; }
		public string Annotation { get; set; }
		public string PromoFile { get; set; }
		public bool AgencyDisabled { get; set; }
		public string Name { get; set; }
		public decimal RegionMask { get; set; }

		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime? Begin { get; set; }

		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime? End { get; set; }

		public bool Status { get; set; }

		public virtual ICollection<PromotionToDrug> promotionToDrug { get; set; }
	}

	public class MediaFile
	{
		public MediaFile()
		{
		}

		public MediaFile(string imageName)
		{
			ImageName = ApplyLimit(Path.GetFileName(imageName));
		}

		public int Id { get; set; }
		public string ImageName { get; set; }
		public byte[] ImageFile { get; set; }
		public string ImageType { get; set; }
		public string ImageSize { get; set; }
		public int EntityType { get; set; }

		public static string ApplyLimit(string path)
		{
			var fileName = Path.GetFileName(path);
			if (fileName.Length > 50) {
				var name = Path.GetFileNameWithoutExtension(path);
				var ext = Path.GetExtension(path);
				name = name.Substring(0, 50 - ext.Length);
				fileName = name + ext;
			}
			return fileName;
		}
	}

	public class PromotionToDrug
	{
			public long DrugId { get; set; }
			public long PromotionId { get; set; }

			public virtual Promotion promotions { get; set; }
	}

	public class PromotionsToSupplier
	{
			public long PromotionId { get; set; }
			public long SupplierId { get; set; }

			public virtual Promotion promotions { get; set; }
	}

	public class User
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Login { get; set; }
	}


	//[MetadataType(typeof(promotionsMetaData))]
	[DisplayName("�����")]
	public class Promotion
	{
		public Promotion()
		{
			this.promotionToDrug = new HashSet<PromotionToDrug>();
			this.PromotionsToSupplier = new HashSet<PromotionsToSupplier>();
		}

		public long Id { get; set; }
		public DateTime UpdateTime { get; set; }
		public bool Enabled { get; set; }
		public long? AdminId { get; set; }
		public long ProducerId { get; set; }
		public string Annotation { get; set; }
		public string PromoFile { get; set; }
		public bool AgencyDisabled { get; set; }
		public string Name { get; set; }
		public decimal RegionMask { get; set; }
		public DateTime Begin { get; set; }
		public DateTime End { get; set; }
		public PromotionStatus Status { get; set; }
		public long? ProducerAdminId { get; set; }
		public bool AllSuppliers { get; set; }

		public virtual User Author { get; set; }
		public virtual MediaFile MediaFile { get; set; }
		public virtual ICollection<PromotionToDrug> promotionToDrug { get; set; }
		public virtual ICollection<PromotionsToSupplier> PromotionsToSupplier { get; set; }

		public List<OptionElement> GlobalDrugList { get; set; }
		public List<OptionElement> RegionnamesList { get; set; }

		public List<long> RegionList { get; set; }
		public List<OptionElement> DrugList { get; set; }

		public void GetRegionnamesList()
		{
			var cntx = new producerinterface_Entities();
			//var h = new NamesHelper(cntx, cntx.Account.First(xxx => xxx.Id == this.Account.Id).Id);
			//RegionnamesList = h.GetPromotionRegionNames(Convert.ToUInt64(this.RegionMask));
		}

		public ActualPromotionStatus GetStatus()
		{
			// ��������� �������
			if (Status == PromotionStatus.Rejected)
				return ActualPromotionStatus.Rejected;

			// ��������� �������������
			if (!Enabled)
				return ActualPromotionStatus.Disabled;

			// ������� ������������
			if (Enabled && Status == PromotionStatus.New)
				return ActualPromotionStatus.NotConfirmed;

			// �� ���������, �����������, �� ��������
			if (Enabled && Status == PromotionStatus.Confirmed && Begin > DateTime.Now)
				return ActualPromotionStatus.ConfirmedNotBegin;

			// �� ���������, �����������, ����������
			if (Enabled && Status == PromotionStatus.Confirmed && End < DateTime.Now)
				return ActualPromotionStatus.ConfirmedEnded;

			// ��������
			if (Enabled && Status == PromotionStatus.Confirmed && Begin < DateTime.Now && End > DateTime.Now)
				return ActualPromotionStatus.Active;

			throw new NotSupportedException("����������� ������");
		}
	}
}