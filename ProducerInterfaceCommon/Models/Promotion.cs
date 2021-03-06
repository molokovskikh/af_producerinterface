using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Common.Tools;
using Newtonsoft.Json;
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
		public long ImageSize { get; set; }
		public EntityType EntityType { get; set; }

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
		public PromotionToDrug()
		{
		}

		public PromotionToDrug(long drugId, long promotionId)
		{
			DrugId = drugId;
			PromotionId = promotionId;
		}

		public long DrugId { get; set; }
		public long PromotionId { get; set; }

		public virtual Promotion promotions { get; set; }
	}

	public class PromotionsToSupplier
	{
		public PromotionsToSupplier()
		{
		}

		public PromotionsToSupplier(long promotionId, long supplierId)
		{
			PromotionId = promotionId;
			SupplierId = supplierId;
		}

		public long PromotionId { get; set; }
		public long SupplierId { get; set; }

		public virtual Promotion promotions { get; set; }
	}

	public class User
	{
		public virtual long Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Login { get; set; }
		public virtual string DisplayName => Name ?? Login;
	}

	public class PromotionSnapshot
	{
		private PromotionSnapshot old;

		public PromotionSnapshot()
		{
		}

		public PromotionSnapshot(ProducerInterfaceCommon.ContextModels.Account author,
			Promotion promotion,
			producerinterface_Entities db,
			Context db2,
			string comment = null)
		{
			Author = db2.Users.Find(author.Id);
			AuthorName = Author.DisplayName;
			CreatedOn = DateTime.Now;
			Promotion = promotion;
			Name = promotion.Name;
			SnapshotName = "�������� ����������";
			SnapshotComment = comment;
			Annotation = promotion.Annotation;
			Status = promotion.GetStatus().DisplayName();
			Begin = promotion.Begin;
			End = promotion.End;
			File = promotion.MediaFile;
			var ids = promotion.PromotionToDrug.Select(x => x.DrugId).ToArray();
			var products = db.assortment.Where(x => ids.Contains(x.CatalogId)).Select(x => x.CatalogName).ToArray();
			ProductsJson = JsonConvert.SerializeObject(products);
			var regions = db.Regions((ulong)promotion.RegionMask).Select(x => x.Name).ToArray();
			RegionsJson = JsonConvert.SerializeObject(regions);
			ids = promotion.PromotionsToSupplier.Select(x => x.SupplierId).ToArray();
			var suppliers = db.suppliernames.Where(x => ids.Contains(x.SupplierId)).Select(x => x.SupplierName).ToArray();
			SuppliersJson = JsonConvert.SerializeObject(suppliers);
		}

		public virtual int Id { get; set; }
		public virtual DateTime CreatedOn { get; set; }
		public virtual string SnapshotName { get; set; }
		public virtual string SnapshotComment { get; set; }
		public virtual string AuthorName { get; set; }
		public virtual string AuthorDisplayName => Author.DisplayName ?? AuthorName;
		public virtual User Author { get; set; }

		public virtual Promotion Promotion { get; set; }
		public virtual string Name { get; set; }
		public virtual string Annotation { get; set; }
		public virtual string Status { get; set; }
		public virtual DateTime Begin { get; set; }
		public virtual DateTime End { get; set; }
		public virtual MediaFile File { get; set; }
		public virtual string ProductsJson { get; set; }
		public virtual string RegionsJson { get; set; }
		public virtual string SuppliersJson { get; set; }

		public virtual string[] Products => JsonConvert.DeserializeObject<string[]>(ProductsJson);
		public virtual string[] Regions => JsonConvert.DeserializeObject<string[]>(RegionsJson);
		public virtual string[] Suppliers => JsonConvert.DeserializeObject<string[]>(SuppliersJson);

		public virtual bool IsNameChanged => Name != old?.Name;
		public virtual bool IsAnnotationChanged => Annotation != old?.Annotation;
		public virtual bool IsStatusChanged => Status != old?.Status;
		public virtual bool IsBeginChanged => Begin != old?.Begin;
		public virtual bool IsEndChanged => End != old?.End;
		public virtual bool IsProductsChanged => ProductsJson != old?.ProductsJson;
		public virtual bool IsRegionsChanged => RegionsJson != old?.RegionsJson;
		public virtual bool IsSuppliersChanged => SuppliersJson != old?.SuppliersJson;
		public virtual bool IsFileChanged => File?.Id != old?.File?.Id;

		public void CalculateChanges(PromotionSnapshot old)
		{
			this.old = old;
		}
	}

	[DisplayName("�����")]
	public class Promotion
	{
		public Promotion()
		{
			PromotionToDrug = new HashSet<PromotionToDrug>();
			PromotionsToSupplier = new HashSet<PromotionsToSupplier>();
		}

		public Promotion(Promotion promotion, ContextModels.Account user)
			: this(user)
		{
			Name = promotion.Name + " �����";
			Annotation = promotion.Annotation;
			MediaFile = promotion.MediaFile;
			AllSuppliers = promotion.AllSuppliers;
			RegionMask = promotion.RegionMask;
			PromotionToDrug.AddEach(promotion.PromotionToDrug.Select(x => new PromotionToDrug(x.DrugId, x.PromotionId)));
			PromotionsToSupplier.AddEach(promotion.PromotionsToSupplier.Select(x => new PromotionsToSupplier(x.PromotionId, x.SupplierId)));
		}

		public Promotion(ContextModels.Account user)
			: this()
		{
			Name = "";
			Annotation = "";
			UpdateTime = DateTime.Now;
			Enabled = true;
			Status = PromotionStatus.New;
			ProducerId = user.AccountCompany.ProducerId.Value;
			Begin = DateTime.Today;
			End = DateTime.Today;
			AllSuppliers = true;
			unchecked {
				RegionMask = (long)ulong.MaxValue;
			}
		}

		public long Id { get; set; }
		public DateTime UpdateTime { get; set; }
		public bool Enabled { get; set; }
		public long ProducerId { get; set; }
		public string Annotation { get; set; }
		public string Name { get; set; }
		public long RegionMask { get; set; }
		public DateTime Begin { get; set; }
		public DateTime End { get; set; }
		public PromotionStatus Status { get; set; }
		public bool AllSuppliers { get; set; }

		public virtual User Author { get; set; }
		public virtual MediaFile MediaFile { get; set; }
		public virtual ICollection<PromotionToDrug> PromotionToDrug { get; set; }
		public virtual ICollection<PromotionsToSupplier> PromotionsToSupplier { get; set; }

		public List<OptionElement> GlobalDrugList { get; set; }
		public List<OptionElement> RegionnamesList { get; set; }

		public List<long> RegionList { get; set; }
		public List<OptionElement> DrugList { get; set; }

		public ActualPromotionStatus GetStatus()
		{
			// �� ���������, �����������, ����������
			if (End < DateTime.Today)
				return ActualPromotionStatus.ConfirmedEnded;

			// ��������� �������
			if (Status == PromotionStatus.Rejected)
				return ActualPromotionStatus.Rejected;

			// ��������� �������������
			if (!Enabled)
				return ActualPromotionStatus.Disabled;

			// �� ���������, �����������, �� ��������
			if (Enabled && Status == PromotionStatus.Confirmed && Begin > DateTime.Now)
				return ActualPromotionStatus.ConfirmedNotBegin;

			// ��������
			if (Enabled && Status == PromotionStatus.Confirmed && Begin < DateTime.Now && End >= DateTime.Today)
				return ActualPromotionStatus.Active;

			return ActualPromotionStatus.NotConfirmed;
		}

		public string RowStyle
		{
			get
			{
				var style = "";
				var status = GetStatus();
				if (status == ActualPromotionStatus.ConfirmedEnded) {
					style = "promotion-ended";
				} else if (status == ActualPromotionStatus.Active) {
					style = "promotion-active";
				} else if (status == ActualPromotionStatus.ConfirmedNotBegin) {
					style = "promotion-await";
				} else if (status == ActualPromotionStatus.Rejected) {
					style = "promotion-rejected";
				}
				return style;
			}
		}

		public virtual bool CheckSecurity(ContextModels.Account user)
		{
			return user.AccountCompany?.ProducerId == ProducerId;
		}
	}
}