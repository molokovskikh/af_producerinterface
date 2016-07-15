using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceCommon.Models
{
	public enum ActualPromotionStatus
	{
		[Display(Name = "Ожидает подтверждения")]
		NotConfirmed = 0,
		[Display(Name = "Ожидание даты начала публикации")]
		ConfirmedNotBegin = 1,
		[Display(Name = "Завершена")]
		ConfirmedEnded = 2,
		[Display(Name = "Опубликована")]
		Active = 3,
		[Display(Name = "Отключена пользователем")]
		Disabled = 4,
		[Display(Name = "Любой")]
		All = 5,
		[Display(Name = "Отклонена администратором")]
		Rejected = 6
	}

	public enum PromotionStatus
	{
		[Display(Name = "Ожидает подтверждения")]
		New = 0,
		[Display(Name = "Подтверждена")]
		Confirmed = 1,
		[Display(Name = "Отклонена")]
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
		public string DisplayName => Name ?? Login;
	}

	public class PromotionSnapshot
	{
		public PromotionSnapshot()
		{
		}

		public PromotionSnapshot(User author,
			Promotion promotion,
			producerinterface_Entities db)
		{
			Author = author;
			AuthorName = author.DisplayName;
			CreatedOn = DateTime.Now;
			Promotion = promotion;
			Name = promotion.Name;
			SnapshotName = "Изменена промоакция";
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
	}

	[DisplayName("Акция")]
	public class Promotion
	{
		public Promotion()
		{
			PromotionToDrug = new HashSet<PromotionToDrug>();
			PromotionsToSupplier = new HashSet<PromotionsToSupplier>();
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
			// отклонена админом
			if (Status == PromotionStatus.Rejected)
				return ActualPromotionStatus.Rejected;

			// отключена пользователем
			if (!Enabled)
				return ActualPromotionStatus.Disabled;

			// ожидает согласования
			if (Enabled && Status == PromotionStatus.New)
				return ActualPromotionStatus.NotConfirmed;

			// не отклонена, согласована, не началась
			if (Enabled && Status == PromotionStatus.Confirmed && Begin > DateTime.Now)
				return ActualPromotionStatus.ConfirmedNotBegin;

			// не отклонена, согласована, просрочена
			if (Enabled && Status == PromotionStatus.Confirmed && End < DateTime.Now)
				return ActualPromotionStatus.ConfirmedEnded;

			// активная
			if (Enabled && Status == PromotionStatus.Confirmed && Begin < DateTime.Now && End > DateTime.Now)
				return ActualPromotionStatus.Active;

			throw new NotSupportedException("Неизвестный статус");
		}
	}
}