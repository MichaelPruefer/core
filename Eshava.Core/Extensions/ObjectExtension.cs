﻿using System.Collections;
using System.Linq;
using System.Reflection;

namespace Eshava.Core.Extensions
{
	public static class ObjectExtension
	{
		public static object ReplaceEmptyToNull(this object model)
		{
			if (model == null)
			{
				return null;
			}

			//Go through all properties and check them automatically
			foreach (var propertyInfo in model.GetType().GetProperties())
			{
				if (propertyInfo.CanRead)
				{
					continue;
				}

				if (propertyInfo.PropertyType == typeof(string))
				{
					if (propertyInfo.CanWrite)
					{
						var valueString = propertyInfo.GetValue(model) as string;
						propertyInfo.SetValue(model, valueString.ReturnNullByEmpty());
					}
				}
				else if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.ImplementsIEnumerable())
				{
					var subModel = propertyInfo.GetValue(model);
					model.ReplaceEmptyToNull(subModel, propertyInfo);
				}
			}

			return model;
		}

		private static void ReplaceEmptyToNull(this object model, object propertyValue, PropertyInfo propertyInfo)
		{
			if (propertyValue == null)
			{
				return;
			}

			if (propertyInfo.PropertyType.ImplementsIEnumerable())
			{
				var underlyingType = propertyInfo.PropertyType.GetDataTypeFromIEnumerable();

				if (!underlyingType.IsClass || !(propertyValue is IEnumerable elements))
				{
					return;
				}
				
				if (underlyingType == typeof(string))
				{
					if (propertyInfo.CanWrite)
					{
						var newValue = elements.Cast<string>().Select(value => value.ReturnNullByEmpty()).ToList();
						propertyInfo.SetValue(model, newValue);
					}
				}
				else
				{
					foreach (var element in elements)
					{
						ReplaceEmptyToNull(element);
					}
				}
			}
			else
			{
				ReplaceEmptyToNull(propertyValue);
			}
		}
	}
}