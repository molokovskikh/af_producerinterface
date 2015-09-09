using System.ComponentModel;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using ProducerControlPanel.Controllers;

namespace ProducerControlPanel.Models
{
	/// <summary>
	///     Логирование действий связанных с моделями
	/// </summary>
	[Model(Database = "accessright", Table = "admin_logs")]
	public class AdminLogModel : LogModel
	{
		[BelongsTo, Description("Инициатор события")]
		public virtual Admin Admin { get; set; }

		public override void SetAdditionParams(BaseController controller)
		{
			var ctrl = (BaseAdminController) controller;
			Admin = ctrl.GetCurrentUser(false);
		}
	}
}