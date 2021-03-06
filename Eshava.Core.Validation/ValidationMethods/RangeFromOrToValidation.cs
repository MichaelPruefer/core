﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Eshava.Core.Extensions;
using Eshava.Core.Validation.Attributes;
using Eshava.Core.Validation.Models;

namespace Eshava.Core.Validation.ValidationMethods
{
	internal static class RangeFromOrToValidation
	{
		public static ValidationCheckResult CheckRangeFrom(ValidationCheckParameters parameters)
		{
			return CheckRangeFromOrTo<RangeFromAttribute>(parameters, false);
		}

		public static ValidationCheckResult CheckRangeTo(ValidationCheckParameters parameters)
		{
			return CheckRangeFromOrTo<RangeToAttribute>(parameters, true);
		}

		private static ValidationCheckResult CheckRangeFromOrTo<T>(ValidationCheckParameters parameters, bool invertProperties, [CallerMemberName] string memberName = null) where T : AbstractRangeFromOrToAttribute
		{
			var propertyInfoTarget = parameters.PropertyInfo;
			var rangeSource = Attribute.GetCustomAttribute(propertyInfoTarget, typeof(T)) as AbstractRangeFromOrToAttribute;
			var dataType = parameters.PropertyInfo.GetDataType();

			if (rangeSource == null)
			{
				return new ValidationCheckResult { IsValid = true };
			}

			//Determining the proterty for the start value of the value range
			var propertiesSource = rangeSource.PropertyName.Contains(",") ? rangeSource.PropertyName.Split(',') : new[] { rangeSource.PropertyName };
			var results = new List<ValidationCheckResult>();

			foreach (var propertySource in propertiesSource)
			{
				var propertyInfoSource = parameters.DataType.GetProperty(propertySource.Trim());

				if (propertyInfoSource == null)
				{
					results.Add(new ValidationCheckResult { ValidationError = $"{memberName}->{propertyInfoTarget.Name}->propertyInfoFromIsNull" });

					continue;
				}

				var dataTypeFrom = propertyInfoSource.PropertyType.GetDataType();

				//Check whether the data types match
				if (dataType != dataTypeFrom)
				{
					results.Add(new ValidationCheckResult { ValidationError = $"{memberName}->{propertyInfoTarget.Name}->{propertyInfoSource.Name}->DataTypesNotEqual" });

					continue;
				}

				parameters.AllowNull = rangeSource.AllowNull;

				if (invertProperties)
				{
					results.Add(BaseRangeValidation.CheckRangeValue(parameters, propertyInfoTarget, propertyInfoSource));
				}
				else
				{
					results.Add(BaseRangeValidation.CheckRangeValue(parameters, propertyInfoSource, propertyInfoTarget));
				}
			}

			if (results.All(result => result.IsValid))
			{
				return results.First();
			}

			return new ValidationCheckResult { ValidationError = String.Join(Environment.NewLine, results.Where(result => !result.IsValid).Select(result => result.ValidationError)) };
		}
	}
}