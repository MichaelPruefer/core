﻿using System;
using System.Collections.Generic;
using System.Linq;
using Eshava.Core.Dynamic.Fields.Enums;
using Eshava.Core.Dynamic.Fields.Validation.Models;
using Eshava.Core.Validation.Enums;
using Eshava.Core.Validation.Models;

namespace Eshava.Core.Dynamic.Fields.Validation.ValidationMethods
{
	internal static class DecimalPlacesValidation<T, D>
	{
		public static ValidationCheckResult CheckDecimalPlaces(ValidationCheckParameters<T, D> parameters)
		{
			var decimalPlacesRule = parameters.GetConfigurations(FieldConfigurationType.DecimalPlaces).FirstOrDefault();
			if (decimalPlacesRule == null || parameters.Field.Value == null)
			{
				return new ValidationCheckResult { IsValid = true };
			}

			var decimalPlacesValue = decimalPlacesRule.ValueInteger ?? 0;
			if (decimalPlacesValue < 0)
			{
				decimalPlacesValue = 0;
			}

			if (decimalPlacesValue >= 0)
			{
				var faktor = Convert.ToInt32(Math.Pow(10, decimalPlacesValue));

				if (parameters.Field.Type == typeof(float) || parameters.Field.Type == typeof(double))
				{
					var valueDouble = Convert.ToDouble(parameters.Field.Value);
					valueDouble *= faktor;

					if (!Equals(Math.Truncate(valueDouble), valueDouble))
					{
						return GetErrorResult(ValidationErrorType.DataTypeFloatOrDouble, parameters.Field.Id);
					}
				}
				else if (parameters.Field.Type == typeof(decimal))
				{
					var valueDecimal = (decimal)parameters.Field.Value;
					valueDecimal *= faktor;

					if (!Equals(Math.Truncate(valueDecimal), valueDecimal))
					{
						return GetErrorResult(ValidationErrorType.DataTypeDecimal, parameters.Field.Id);
					}
				}
			}

			return new ValidationCheckResult { IsValid = true };
		}

		private static ValidationCheckResult GetErrorResult(ValidationErrorType errorType, string fieldId)
		{
			return new ValidationCheckResult
			{
				ValidationErrors = new List<ValidationCheckResultEntry>
				{
					new ValidationCheckResultEntry
					{
						MethodType = ValidationMethodType.DecimalPlaces,
						ErrorType = errorType,
						PropertyName = fieldId
					}
				}
			};
		}
	}
}