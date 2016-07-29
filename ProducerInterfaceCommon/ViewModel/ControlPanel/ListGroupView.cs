using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.Permission
{
	public class ListGroupView
	{
		public int Id { get; set; }

		public TypeUsers TypeUser { get; set; }

		public string Description { get; set; }

		public string NameGroup { get; set; }

		public string[] Permissions { get; set; }

		public List<UsersViewInChange> ListUsersInGroup { get; set; }
	}

	public class UsersViewInChange
	{
		public string Name { get; set; }

		public string eMail { get; set; }

		public string ProducerName { get; set; }
	}
}
