using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceCommon.Models
{
	public class News
	{
		public News()
		{
				NewsChange = new HashSet<NewsSnapshot>();
		}

		public long Id { get; set; }

		[Display(Name = "Оглавление")]
		[MaxLength(150)]
		[Required(ErrorMessage = "Заполните поле Оглавление")]
		public string Subject { get; set; }

		[Required(ErrorMessage = "Заполните поле Новость")]
		[MaxLength(10000)]
		[Display(Name = "Новость")]
		public string Body { get; set; }

		[Display(Name = "Дата публикации")]
		public DateTime? DatePublication { get; set; }
		public bool Enabled { get; set; }

		public virtual ICollection<NewsSnapshot> NewsChange { get; set; }
	}

	public class NewsSnapshot
	{
		public NewsSnapshot()
		{
		}

		public NewsSnapshot(News news, User user, string name)
		{
			News = news;
			Author = user;
			AuthorName = Author.DisplayName;
			CreatedOn = DateTime.Now;
			SnapshotName = name;
			Body = news.Body;
			Subject = news.Subject;
		}

		public virtual int Id { get; set; }
		public virtual DateTime CreatedOn { get; set; }
		public virtual string SnapshotName { get; set; }
		public virtual string AuthorName { get; set; }
		public virtual string AuthorDisplayName => Author.DisplayName ?? AuthorName;
		public virtual User Author { get; set; }

		public virtual string Subject { get; set; }
		public virtual string Body { get; set; }
		public virtual News News { get; set; }
	}
}