﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Enums;
using Eshava.Core.Validation.Models;

namespace Eshava.Core.Validation.ValidationMethods
{
	internal static class RequiredValidation
	{
		public static ValidationCheckResult CheckRequired(ValidationCheckParameters parameters)
		{
			if (Attribute.GetCustomAttribute(parameters.PropertyInfo, typeof(RequiredAttribute)) != null)
			{
				if (parameters.PropertyValue == null)
				{
					return GetErrorResult(ValidationErrorType.IsNull, parameters.PropertyInfo.Name);
				}

				if (parameters.PropertyInfo.PropertyType == typeof(string))
				{
					var valueString = parameters.PropertyValue as string;

					if (valueString.IsNullOrEmpty())
					{
						return GetErrorResult(ValidationErrorType.IsEmpty, parameters.PropertyInfo.Name);
					}
				}

				if (parameters.PropertyInfo.PropertyType == typeof(Guid))
				{
					var valueGuid = parameters.PropertyValue as Guid?;

					if (!valueGuid.HasValue || valueGuid.Value == Guid.Empty)
					{
						return GetErrorResult(ValidationErrorType.IsEmpty, parameters.PropertyInfo.Name);
					}
				}

				if (parameters.PropertyInfo.PropertyType == typeof(DateTime))
				{
					var valueGuid = parameters.PropertyValue as DateTime?;

					if (!valueGuid.HasValue || valueGuid.Value == DateTime.MinValue)
					{
						return GetErrorResult(ValidationErrorType.IsEmpty, parameters.PropertyInfo.Name);
					}
				}

				if (parameters.PropertyInfo.PropertyType.ImplementsIEnumerable())
				{
					var elements = parameters.PropertyValue as IEnumerable;
					if (elements.Cast<object>().Any())
					{
						return new ValidationCheckResult();
					}

					return GetErrorResult(ValidationErrorType.IsEmptyIEnumerable, parameters.PropertyInfo.Name);
				}
			}

			return new ValidationCheckResult();
		}

		private static ValidationCheckResult GetErrorResult(ValidationErrorType errorType, string propertyName)
		{
			return new ValidationCheckResult
			{
				ValidationErrors = new List<ValidationError>
				{
					new ValidationError
					{
						MethodType = ValidationMethodType.Required.ToString(),
						ErrorType = errorType.ToString(),
						PropertyName = propertyName
					}
				}
			};
		}
	}
}