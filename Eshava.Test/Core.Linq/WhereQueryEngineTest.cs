﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Eshava.Core.Linq;
using Eshava.Core.Linq.Enums;
using Eshava.Core.Linq.Models;
using Eshava.Test.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.Core.Linq
{
	[TestClass, TestCategory("Core.Linq")]
	public class WhereQueryEngineTest
	{
		private WhereQueryEngine _classUnderTest;

		[TestInitialize]
		public void Setup()
		{
			_classUnderTest = new WhereQueryEngine();
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void BuildQueryExpressionsWithNullInputTest()
		{
			// Act
			_classUnderTest.BuildQueryExpressions<Alpha>(null);
		}

		[TestMethod]
		public void BuildQueryExpressionsNoConditionsTest()
		{
			// Arrange
			var queryParameter = new QueryParameters();

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(0);
		}

		[TestMethod]
		public void BuildQueryExpressionsGlobalSearchTermTest()
		{
			// Arrange
			var queryParameter = new QueryParameters
			{
				SearchTerm = "Darkwing Duck"
			};

			Expression<Func<Alpha, bool>> expectedResult = p =>
				p.Gamma.Contains("Darkwing Duck") ||
				p.Delta.Contains("Darkwing Duck") ||
				p.DeltaTwo.Contains("Darkwing Duck") ||
				p.DeltaMail.Contains("Darkwing Duck") ||
				p.DeltaUrl.Contains("Darkwing Duck") ||
				p.Epsilon.Contains("Darkwing Duck") ||
				p.EpsilonTwo.Contains("Darkwing Duck") ||
				p.Phi.Contains("Darkwing Duck");

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(1);
			result.First().Should().BeEquivalentTo(expectedResult);
		}

		[TestMethod]
		public void BuildQueryExpressionsStringPropertyTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Gamma = "DD",
					Delta = "QuackFu better than KungFu"
				},
				new Alpha
				{
					Beta = 2,
					Gamma = "Darkwing Duck",
					Delta = "QuackFu better than KungFu"
				},
				new Alpha
				{
					Beta = 3,
					Gamma = "Darkwing Duck",
					Delta = "KungFu"
				}
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.Gamma),
						SearchTerm = "Darkwing Duck"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Contains,
						PropertyName = nameof(Alpha.Delta),
						SearchTerm = "QuackFu"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultGamma = p => p.Gamma == "Darkwing Duck";
			Expression<Func<Alpha, bool>> expectedResultDelta = p => p.Delta != null && p.Delta.Contains("QuackFu");

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(2);
			result.First().Should().BeEquivalentTo(expectedResultGamma);
			result.Last().Should().BeEquivalentTo(expectedResultDelta);

			var resultWhere = exampleList.Where(result.First().Compile()).Where(result.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsIntegerPropertyTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Lambda = 4,
					LambdaNullable = 10
				},
				new Alpha
				{
					Beta = 2,
					Lambda = 6,
					LambdaNullable = 11
				},
				new Alpha
				{
					Beta = 3,
					Lambda = 6,
					LambdaNullable = 8
				},
				new Alpha
				{
					Beta = 4,
					Lambda = 6,
					LambdaNullable = 13
				}
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.GreaterThan,
						PropertyName = nameof(Alpha.Lambda),
						SearchTerm = "5"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.LessThan,
						PropertyName = nameof(Alpha.LambdaNullable),
						SearchTerm = "12"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultLambda = p => p.Lambda > 5;
			Expression<Func<Alpha, bool>> expectedResultLambdaNullable = p => p.LambdaNullable < 12;

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(2);
			result.First().Should().BeEquivalentTo(expectedResultLambda);
			result.Last().Should().BeEquivalentTo(expectedResultLambdaNullable);

			var resultWhere = exampleList.Where(result.First().Compile()).Where(result.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(2);
			resultWhere.First().Beta.Should().Be(2);
			resultWhere.Last().Beta.Should().Be(3);
		}

		[TestMethod]
		public void BuildQueryExpressionsDecimalPropertyTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					My = 4m,
					MyNullableOne = 10m
				},
				new Alpha
				{
					Beta = 2,
					My = 4.26m,
					MyNullableOne = 10.49m
				},
				new Alpha
				{
					Beta = 3,
					My = 6.25m,
					MyNullableOne = 8.1m
				},
				new Alpha
				{
					Beta = 4,
					My = 5m,
					MyNullableOne = 11m
				}
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.GreaterThan,
						PropertyName = nameof(Alpha.My),
						SearchTerm = "4.25"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.LessThan,
						PropertyName = nameof(Alpha.MyNullableOne),
						SearchTerm = "10.5"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultMy = p => p.My > 4.25m;
			Expression<Func<Alpha, bool>> expectedResultMyNullableOne = p => p.MyNullableOne < 10.5m;

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(2);
			result.First().Should().BeEquivalentTo(expectedResultMy);
			result.Last().Should().BeEquivalentTo(expectedResultMyNullableOne);

			var resultWhere = exampleList.Where(result.First().Compile()).Where(result.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(2);
			resultWhere.First().Beta.Should().Be(2);
			resultWhere.Last().Beta.Should().Be(3);
		}


		[TestMethod]
		public void BuildQueryExpressionsFloatPropertyTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Ny = 4f,
					NyNullable = 10f
				},
				new Alpha
				{
					Beta = 2,
					Ny = 4.25f,
					NyNullable = 10.5f
				},
				new Alpha
				{
					Beta = 3,
					Ny = 6f,
					NyNullable = 8f
				},
				new Alpha
				{
					Beta = 4,
					Ny = 5f,
					NyNullable = 11f
				}
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.GreaterThanOrEqual,
						PropertyName = nameof(Alpha.Ny),
						SearchTerm = "4.25"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.LessThanOrEqual,
						PropertyName = nameof(Alpha.NyNullable),
						SearchTerm = "10.5"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultNy = p => p.Ny >= 4.25f;
			Expression<Func<Alpha, bool>> expectedResultNyNullable = p => p.NyNullable <= 10.5f;

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(2);
			result.First().Should().BeEquivalentTo(expectedResultNy);
			result.Last().Should().BeEquivalentTo(expectedResultNyNullable);

			var resultWhere = exampleList.Where(result.First().Compile()).Where(result.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(2);
			resultWhere.First().Beta.Should().Be(2);
			resultWhere.Last().Beta.Should().Be(3);
		}

		[TestMethod]
		public void BuildQueryExpressionsDoublePropertyTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Xi = 0,
					XiNullable = 0
				},
				new Alpha
				{
					Beta = 2,
					Xi = 4.25,
					XiNullable = 4.25
				},
				new Alpha
				{
					Beta = 3,
					Xi = 4.25,
					XiNullable = 10.5
				}
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.Xi),
						SearchTerm = "4.25"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.NotEqual,
						PropertyName = nameof(Alpha.XiNullable),
						SearchTerm = "10.5"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultXi = p => p.Xi == 4.25;
			Expression<Func<Alpha, bool>> expectedResultXiNullable = p => p.XiNullable != 10.5;

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(2);
			result.First().Should().BeEquivalentTo(expectedResultXi);
			result.Last().Should().BeEquivalentTo(expectedResultXiNullable);

			var resultWhere = exampleList.Where(result.First().Compile()).Where(result.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsDateTimePropertyTest()
		{
			// Arrange
			var dateTimePsi = DateTime.Today.Add(new TimeSpan(12, 30, 15));
			var dateTimeOmega = DateTime.Today.Add(new TimeSpan(9, 45, 30));
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Psi = null,
					OmegaDateTime = dateTimeOmega
				},
				new Alpha
				{
					Beta = 2,
					Psi = dateTimeOmega,
					OmegaDateTime = dateTimePsi
				},
				new Alpha
				{
					Beta = 3,
					Psi = dateTimePsi,
					OmegaDateTime = dateTimePsi
				},
				new Alpha
				{
					Beta = 4,
					Psi = DateTime.Today.AddDays(1),
					OmegaDateTime = dateTimePsi
				}
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.LessThanOrEqual,
						PropertyName = nameof(Alpha.Psi),
						SearchTerm = dateTimePsi.ToString("yyyy-MM-ddTHH:mm:ssZ")
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.NotEqual,
						PropertyName = nameof(Alpha.OmegaDateTime),
						SearchTerm = dateTimeOmega.ToString("yyyy-MM-ddTHH:mm:ssZ")
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultPsi = p => p.Psi <= dateTimePsi;
			Expression<Func<Alpha, bool>> expectedResultOmegaDateTime = p => p.OmegaDateTime != dateTimeOmega;

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(2);
			result.First().Should().BeEquivalentTo(expectedResultPsi);
			result.Last().Should().BeEquivalentTo(expectedResultOmegaDateTime);

			var resultWhere = exampleList.Where(result.First().Compile()).Where(result.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(2);
			resultWhere.First().Beta.Should().Be(2);
			resultWhere.Last().Beta.Should().Be(3);
		}

		[TestMethod]
		public void BuildQueryExpressionsWrongDataTypesTest()
		{
			// Arrange
			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.GreaterThan,
						PropertyName = nameof(Alpha.Gamma),
						SearchTerm =  ""
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.GreaterThan,
						PropertyName = nameof(Alpha.Lambda),
						SearchTerm =  "Darkwing Duck"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.GreaterThanOrEqual,
						PropertyName = nameof(Alpha.My),
						SearchTerm = "Darkwing Duck"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.GreaterThanOrEqual,
						PropertyName = nameof(Alpha.Ny),
						SearchTerm = "Darkwing Duck"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.Xi),
						SearchTerm = "Darkwing Duck"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.Psi),
						SearchTerm = "Darkwing Duck"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.StigmaOne),
						SearchTerm =  ""
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.Chi),
						SearchTerm = "Darkwing Duck"
					}
				}
			};

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(0);
		}

		[TestMethod]
		public void BuildQueryExpressionsBooleanPropertyTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					StigmaOne = true,
					StigmaTwo = null
				},
				new Alpha
				{
					Beta = 2,
					StigmaOne = true,
					StigmaTwo = false
				},
				new Alpha
				{
					Beta = 3,
					StigmaOne = true,
					StigmaTwo = true
				},
				new Alpha
				{
					Beta = 4,
					StigmaOne = false,
					StigmaTwo = false
				}
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.StigmaOne),
						SearchTerm = "1"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.StigmaTwo),
						SearchTerm = "false"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultStigmaOne = p => p.StigmaOne == true;
			Expression<Func<Alpha, bool>> expectedResultStigmaTwo = p => p.StigmaTwo == false;

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(2);
			result.First().Should().BeEquivalentTo(expectedResultStigmaOne);
			result.Last().Should().BeEquivalentTo(expectedResultStigmaTwo);

			var resultWhere = exampleList.Where(result.First().Compile()).Where(result.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(2);
		}

		[TestMethod]
		public void BuildQueryExpressionsBooleanNullablePropertyTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					StigmaTwo = null
				},
				new Alpha
				{
					Beta = 2,
					StigmaTwo = false
				},
				new Alpha
				{
					Beta = 3,
					StigmaTwo = true
				}
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.NotEqual,
						PropertyName = nameof(Alpha.StigmaTwo),
						SearchTerm = "true"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.NotEqual,
						PropertyName = nameof(Alpha.StigmaTwo),
						SearchTerm = "false"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultStigmaOne = p => p.StigmaTwo != true;
			Expression<Func<Alpha, bool>> expectedResultStigmaTwo = p => p.StigmaTwo != false;

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(2);
			result.First().Should().BeEquivalentTo(expectedResultStigmaOne);
			result.Last().Should().BeEquivalentTo(expectedResultStigmaTwo);

			var resultWhere = exampleList.Where(result.First().Compile()).Where(result.Last().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(1);
		}

		[TestMethod]
		public void BuildQueryExpressionsGuidPropertyTest()
		{
			// Arrange
			var guidToMatch = Guid.Parse("87428c4c-26c5-4208-861f-6875606e356f");

			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Chi = null
				},
				new Alpha
				{
					Beta = 2,
					Chi = Guid.Empty
				},
				new Alpha
				{
					Beta = 3,
					Chi = guidToMatch
				}
				,
				new Alpha
				{
					Beta = 4,
					Chi = Guid.Parse("3f93f67c-1345-48c4-bb71-547378e71c6c")
				}
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.Chi),
						SearchTerm = guidToMatch.ToString()
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultStigmaOne = p => p.Chi == guidToMatch;

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(1);
			result.First().Should().BeEquivalentTo(expectedResultStigmaOne);

			var resultWhere = exampleList.Where(result.First().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(3);
		}

		[TestMethod]
		public void BuildQueryExpressionsPropertyMappingTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Kappa = new Omega
					{
						Psi = ""
					}
				},
				new Alpha
				{
					Beta = 2,
					Kappa = new Omega
					{
						Psi = null
					}
				},
				new Alpha
				{
					Beta = 3,
					Kappa = new Omega
					{
						Psi = "Darkwing Duck"
					}
				},
				new Alpha
				{
					Beta = 4,
					Kappa = new Omega
					{
						Psi = "QuackFu"
					}
				}
			};

			var mappings = new Dictionary<string, List<Expression<Func<Alpha, object>>>>
			{
				{ nameof(Alpha.Chi), new List<Expression<Func<Alpha, object>>> { p => p.Kappa.Psi } }
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.Chi),
						SearchTerm = "Darkwing Duck"
					},
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Contains,
						PropertyName = nameof(Alpha.Chi),
						SearchTerm = "QuackFu"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultEqual = p => p.Kappa.Psi == "Darkwing Duck";
			Expression<Func<Alpha, bool>> expectedResultContains = p => p.Kappa.Psi != null && p.Kappa.Psi.Contains("QuackFu");

			// Act
			var result = _classUnderTest.BuildQueryExpressions(queryParameter, mappings);

			// Assert
			result.Should().HaveCount(2);
			result.First().Should().BeEquivalentTo(expectedResultEqual);
			result.Last().Should().BeEquivalentTo(expectedResultContains);

			exampleList.Where(result.First().Compile()).Single().Beta.Should().Be(3);
			exampleList.Where(result.Last().Compile()).Single().Beta.Should().Be(4);
		}

		[TestMethod]
		public void BuildQueryExpressionsPropertyMappingMultipleTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Kappa = new Omega
					{
						Psi = "",
						Chi = ""
					}
				},
				new Alpha
				{
					Beta = 2,
					Kappa = new Omega
					{
						Psi = null,
						Chi = null
					}
				},
				new Alpha
				{
					Beta = 3,
					Kappa = new Omega
					{
						Psi = "Darkwing Duck",
						Chi = ""
					}
				},
				new Alpha
				{
					Beta = 4,
					Kappa = new Omega
					{
						Psi = "",
						Chi = "Darkwing Duck"
					}
				}
			};

			var mappings = new Dictionary<string, List<Expression<Func<Alpha, object>>>>
			{
				{ nameof(Alpha.Chi), new List<Expression<Func<Alpha, object>>> { p => p.Kappa.Psi, p => p.Kappa.Chi, p => p.Kappa.Sigma } }
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Equal,
						PropertyName = nameof(Alpha.Chi),
						SearchTerm = "Darkwing Duck"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultEqual = p => p.Kappa.Psi == "Darkwing Duck" || p.Kappa.Chi == "Darkwing Duck";

			// Act
			var result = _classUnderTest.BuildQueryExpressions(queryParameter, mappings);

			// Assert
			result.Should().HaveCount(1);
			result.First().Should().BeEquivalentTo(expectedResultEqual);

			var resultWhere = exampleList.Where(result.First().Compile()).ToList();
			resultWhere.Should().HaveCount(2);
			resultWhere.First().Beta.Should().Be(3);
			resultWhere.Last().Beta.Should().Be(4);
		}

		[TestMethod]
		public void BuildQueryExpressionsPropertyMappingEnumarableTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Sigma = null
				},
				new Alpha
				{
					Beta = 2,
					Sigma = new List<int>()
				},
				new Alpha
				{
					Beta = 3,
					Sigma = new List<int> { 7, 5, 3 }
				},
				new Alpha
				{
					Beta = 4,
					Sigma = new List<int> { 1, 2, 3, }
				}
			};

			var mappings = new Dictionary<string, List<Expression<Func<Alpha, object>>>>
			{
				{ nameof(Alpha.Chi), new List<Expression<Func<Alpha, object>>> { p => p.Sigma } }
			};

			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Contains,
						PropertyName = nameof(Alpha.Chi),
						SearchTerm = "7"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultEqual = p => p.Sigma != null && p.Sigma.Contains(7);

			// Act
			var result = _classUnderTest.BuildQueryExpressions(queryParameter, mappings);

			// Assert
			result.Should().HaveCount(1);
			result.First().Should().BeEquivalentTo(expectedResultEqual);

			var resultWhere = exampleList.Where(result.First().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(3);
		}

		[TestMethod]
		public void BuildQueryExpressionsPropertyEnumarableTest()
		{
			// Arrange
			var exampleList = new List<Alpha>
			{
				new Alpha
				{
					Beta = 1,
					Sigma = null
				},
				new Alpha
				{
					Beta = 2,
					Sigma = new List<int>()
				},
				new Alpha
				{
					Beta = 3,
					Sigma = new List<int> { 7, 5, 3 }
				},
				new Alpha
				{
					Beta = 4,
					Sigma = new List<int> { 1, 2, 3, }
				}
			};
			
			var queryParameter = new QueryParameters
			{
				WhereQueryProperties = new List<WhereQueryProperty>
				{
					new WhereQueryProperty
					{
						Operator =  CompareOperator.Contains,
						PropertyName = nameof(Alpha.Sigma),
						SearchTerm = "7"
					}
				}
			};

			Expression<Func<Alpha, bool>> expectedResultEqual = p => p.Sigma != null && p.Sigma.Contains(7);

			// Act
			var result = _classUnderTest.BuildQueryExpressions<Alpha>(queryParameter);

			// Assert
			result.Should().HaveCount(1);
			result.First().Should().BeEquivalentTo(expectedResultEqual);

			var resultWhere = exampleList.Where(result.First().Compile()).ToList();
			resultWhere.Should().HaveCount(1);
			resultWhere.First().Beta.Should().Be(3);
		}
	}
}