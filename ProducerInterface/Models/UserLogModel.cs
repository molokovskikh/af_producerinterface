using System.ComponentModel;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using AnalitFramefork.Mvc;
using ProducerInterface.Controllers;

namespace ProducerInterface.Models
{
	/// <summary>
	///     Логирование действий связанных с моделями
	/// </summary>
	[Model(Database = "ProducerInterface", Table = "user_logs")]
	public class UserLogModel : LogModel
	{
		[BelongsTo("UserId"), Description("Инициатор события")]
		public virtual ProducerUser ProducerUser { get; set; }

		public override void SetAdditionParams(BaseController controller)
		{
			var ctrl = (BaseInterfaceController) controller;
			ProducerUser = ctrl.GetCurrentUser();
		}
	}
}