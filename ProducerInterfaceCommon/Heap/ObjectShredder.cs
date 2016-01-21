using ProducerInterfaceCommon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace ProducerInterfaceCommon.Heap
{
	// https://msdn.microsoft.com/ru-ru/library/bb669096(v=vs.110).aspx
	public class ObjectShredder<T>
	{
		private PropertyInfo[] _pi;
		private Dictionary<string, int> _ordinalMap;
		private Type _type;

		// ObjectShredder constructor.
		public ObjectShredder()
		{
			_type = typeof(T);
			_pi = _type.GetProperties();
			_ordinalMap = new Dictionary<string, int>();
		}

		/// <summary>
		/// Loads a DataTable from a sequence of objects.
		/// </summary>
		/// <param name="source">The sequence of objects to load into the DataTable.</param>
		/// <returns>A DataTable created from the source sequence.</returns>
		public DataTable Shred(IEnumerable<T> source)
		{
			// Create a new table if the input table is null.
			var	table = new DataTable(typeof(T).Name);

			// Initialize the ordinal map and extend the table schema based on type T.
			table = ExtendTable(table, typeof(T));

			// Enumerate the source sequence and load the object values into rows.
			table.BeginLoadData();
			using (IEnumerator<T> e = source.GetEnumerator()) {
				while (e.MoveNext()) {
						table.LoadDataRow(ShredObject(table, e.Current), true);
				}
			}
			table.EndLoadData();

			// Return the table.
			return table;
		}

		public object[] ShredObject(DataTable table, T instance)
		{
			PropertyInfo[] pi = _pi;

			if (instance.GetType() != typeof(T)) {
				// If the instance is derived from T, extend the table schema
				// and get the properties and fields.
				ExtendTable(table, instance.GetType());
				pi = instance.GetType().GetProperties();
			}

			// Add the property and field values of the instance to an array.
			Object[] values = new object[table.Columns.Count];

			foreach (PropertyInfo p in pi) {
				if (Attribute.IsDefined(p, typeof(HiddenAttribute)))
					continue;

				var val = p.GetValue(instance, null);
				var rd = p.GetCustomAttribute<RoundAttribute>();
				if (rd != null && val != null) {
					val = Decimal.Round((Decimal)val, rd.Precision);
				}
				values[_ordinalMap[p.Name]] = val ?? DBNull.Value;
			}

			// Return the property and field values of the instance.
			return values;
		}

		public DataTable ExtendTable(DataTable table, Type type)
		{
			// Extend the table schema if the input table was null or if the value 
			foreach (PropertyInfo p in type.GetProperties()) {
				if (!_ordinalMap.ContainsKey(p.Name) && !Attribute.IsDefined(p, typeof(HiddenAttribute))) {
					// Add the property as a column in the table if it doesn't exist
					// already.
					DataColumn dc = table.Columns.Contains(p.Name) ? table.Columns[p.Name]
						: table.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);

					var da = p.GetCustomAttribute<DisplayAttribute>();
					dc.Caption = (da != null ? da.Name : "");

					// Add the property to the ordinal map.
					_ordinalMap.Add(p.Name, dc.Ordinal);
				}
			}

			// Return the table.
			return table;
		}
	}
}