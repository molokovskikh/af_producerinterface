﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace ProducerInterfaceCommon.Heap
{
	public class MyAutoMapper<T>
	{
		private Type _type;
		private PropertyInfo[] _pi;


		public MyAutoMapper()
		{
			_type = typeof(T);
			_pi = _type.GetProperties();
		}

		public List<T> Map(MySqlDataReader reader)
		{
			var result = new List<T>();
			if (reader.HasRows)
			{
				var typeReaderNames = new List<string>();
				for (int i = 0; i < reader.FieldCount; i++)
					typeReaderNames.Add(reader.GetName(i));

				var typeIntersectProps = _pi.Where(x => typeReaderNames.Contains(x.Name)).ToArray();

				while (reader.Read())
				{
					var en = (T)Activator.CreateInstance(typeof(T));
					foreach (var p in typeIntersectProps)
						p.SetValue(en, reader[p.Name] == DBNull.Value ? null : reader[p.Name]);
					result.Add(en);
				}
			}
			return result;
		}

		public T Map(object o)
		{
			var en = (T)Activator.CreateInstance(typeof(T));
			var inProps = o.GetType().GetProperties();
			var propNames = inProps.Select(x => x.Name);
			var typeIntersectProps = _pi.Where(x => propNames.Contains(x.Name)).ToArray();
			foreach (var p in typeIntersectProps)
			{
				var val = inProps.Single(x => x.Name == p.Name).GetValue(o);
				p.SetValue(en, val == DBNull.Value ? null : val);
			}
			return en;
		}

	}
}