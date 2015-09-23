using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AnalitFramefork.Components;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using AnalitFramefork.Hibernate.Models;
using MySql.Data.MySqlClient;
using NHibernate;
using ProducerInterface.Models;

namespace ProducerControlPanelTest.Infrastructure.Helpers
{
	public class ModelCleaner: ProducerInterfaceTest.Infrastructure.Helpers.ModelCleaner
	{
		public new void RemoveModels(ISession dbSession)
		{
			base.RemoveModels(dbSession);
		}
	}
}