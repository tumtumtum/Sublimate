﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dryice.Expressions
{
	public static class DryExpression
	{
		public static CommentExpression Comment(string comment)
		{
			return new CommentExpression(comment);
		}

		public static BlockExpression Block(params Expression[] expressions)
		{
			return DryExpression.Block(null, expressions);
		}

		public static BlockExpression Block(IEnumerable<ParameterExpression> variables, params Expression[] expressions)
		{
			var newExpressions = expressions.ToGroupedExpression(GroupedExpressionsExpressionStyle.Wide);

			if (variables == null)
			{
				return Expression.Block(newExpressions);
			}
			else
			{
				return Expression.Block(variables, newExpressions);
			}
		}

		public static ParameterExpression Variable(string type, string name = null)
		{
			return Expression.Variable(new DryType(type), name);
		}

		public static ParameterExpression Variable(Type type, string name = null)
		{
			return Expression.Variable(type, name);
		}

		public static ParameterExpression Parameter(string type, string name = null)
		{
			return Expression.Parameter(new DryType(type), name);
		}

		public static ParameterExpression Parameter(Type type, string name = null)
		{
			return Expression.Parameter(type, name);
		}

		private static bool IsAnonymousType(this Type type)
		{
			return type.IsGenericType
				   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic
				   && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
				   && (type.Name.Contains("AnonymousType") || type.Name.Contains("AnonType"))
				   && Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false);
		}

		public static MethodCallExpression MakeStaticMethodCall(Type type, string methodName, object arguments)
		{
			return MakeMethodCall(null, type, typeof(void), methodName, arguments, true);
		}

		public static MethodCallExpression MakeStaticMethodCall(string type, string methodName, object arguments)
		{
			return MakeMethodCall(null, new DryType(type), typeof(void), methodName, arguments, true);
		}

		public static MethodCallExpression MakeStaticMethodCall(Type type, Type returnType, string methodName, object arguments)
		{
			return MakeMethodCall(null, type, returnType, methodName, arguments, true);
		}

		public static MethodCallExpression MakeStaticMethodCall(string type, string returnType, string methodName, object arguments)
		{
			return MakeMethodCall(null, new DryType(type), new DryType(returnType), methodName, arguments, true);
		}

		public static MethodCallExpression MakeMethodCall(Expression instance, string methodName, object arguments)
		{
			return MakeMethodCall(instance, instance.Type, typeof(void), methodName, arguments, false);
		}

		public static MethodCallExpression MakeMethodCall(Expression instance, string returnType, string methodName, object arguments)
		{
			return MakeMethodCall(instance, instance.Type, new DryType(returnType), methodName, arguments, false);
		}

		public static MethodCallExpression MakeMethodCall(Expression instance, Type returnType, string methodName, object arguments)
		{
			return MakeMethodCall(instance, instance.Type, returnType, methodName, arguments, false);
		}

		private static Tuple<ParameterInfo[], Expression[]> GetParametersAndArguments(object arguments)
		{
			Expression[] argumentExpressions;
			DryParameterInfo[] parameterInfos;

			if (arguments == null)
			{
				parameterInfos = new DryParameterInfo[0];

				argumentExpressions = new Expression[0];
			}
			else if (arguments.GetType().IsAnonymousType())
			{
				var properties = arguments.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				var i = 0;
				argumentExpressions = new Expression[properties.Length];
				parameterInfos = new DryParameterInfo[properties.Length];

				foreach (var propertyInfo in properties)
				{
					Type parameterType;
					var parameterName = propertyInfo.Name;
					var value = propertyInfo.GetValue(arguments);

					if (value is Expression)
					{
						parameterType = ((Expression)value).Type;
						argumentExpressions[i] = (Expression)value;
					}
					else
					{
						parameterType = propertyInfo.PropertyType;
						argumentExpressions[i] = Expression.Constant(value);
					}

					parameterInfos[i] = new DryParameterInfo(parameterType, parameterName);


					i++;
				}
			}
			else if (arguments is Expression)
			{
				parameterInfos = new DryParameterInfo[]
				{
					new DryParameterInfo(((Expression)arguments).Type, "arg1")
				};

				argumentExpressions = new Expression[]
				{
					(Expression)arguments
				};
			}
			else
			{
				parameterInfos = new DryParameterInfo[]
				{
					new DryParameterInfo(arguments.GetType(), "arg1")
				};

				argumentExpressions = new Expression[]
				{
					Expression.Constant(arguments)
				};
			}

			return new Tuple<ParameterInfo[], Expression[]>(parameterInfos, argumentExpressions);
		}

		private static MethodCallExpression MakeMethodCall(Expression instance, Type type, Type returnType, string methodName, object arguments, bool isStatic = false)
		{
			var result = GetParametersAndArguments(arguments);

			var types = result.Item1.Select(c => c.ParameterType).ToArray();

			var methodInfo = type.GetMethod(methodName, types);

			if (methodInfo == null || methodInfo is DryMethodInfo)
			{
				methodInfo = new DryMethodInfo(type, returnType, methodName, result.Item1, isStatic);
			}

			return Expression.Call(instance, methodInfo, result.Item2);
		}

		public static MemberExpression Property(Expression instance, Type propertyType, string propertyName)
		{
			var propertyInfo = instance.Type.GetProperty(propertyName);

			if (propertyInfo == null || propertyInfo is DryPropertyInfo)
			{
				propertyInfo = new DryPropertyInfo(instance.Type, propertyType, propertyName);
			}

			return Expression.Property(instance, propertyInfo);
		}

		public static NewExpression New(string type, string constructorName, object arguments)
		{
			return DryExpression.New(new DryType(type), constructorName, arguments);
		}

		public static Expression Convert(Expression value, Type type)
		{
			return Expression.Convert(value, type);
		}

		public static Expression Convert(Expression value, string type)
		{
			return Expression.Convert(value, new DryType(type));
		}

		public static NewExpression New(Type type, string constructorName, object arguments)
		{
			var result = GetParametersAndArguments(arguments);

			var constructorInfo = type.GetConstructor(result.Item1.Select(c => c.ParameterType).ToArray());

			if (constructorInfo == null || constructorInfo is DryConstructorInfo)
			{
				constructorInfo = new DryConstructorInfo(type, constructorName, result.Item1);
			}

			return Expression.New(constructorInfo, result.Item2);
		}

		public static GroupedExpressionsExpression Grouped(params Expression[] expressions)
		{
			return new GroupedExpressionsExpression(expressions);
		}

		public static GroupedExpressionsExpression GroupedWode(params Expression[] expressions)
		{
			return new GroupedExpressionsExpression(expressions, GroupedExpressionsExpressionStyle.Wide);
		}

		public static GroupedExpressionsExpression Grouped(IEnumerable<Expression> expressions)
		{
			return new GroupedExpressionsExpression(expressions);
		}

		public static IncludeStatementExpression IncludeStatement(string fileName)
		{
			return new IncludeStatementExpression(fileName);
		}

		public static StatementExpression Statement(Expression expression)
		{
			return new StatementExpression(expression);
		}

		public static StatementExpression ToStatement(this Expression expression)
		{
			return new StatementExpression(expression);
		}

		public static BlockExpression ToBlock(this Expression expression)
		{
			return Expression.Block(expression);
		}

		private static IEnumerable<Expression> ToStatementsNormalized(this IEnumerable<Expression> expressions)
		{
			return expressions.Select(c => (c is  BinaryExpression 
				|| c is MemberExpression 
				|| c is GotoExpression 
				|| c is MethodCallExpression) ? c.ToStatement() : c);
		}

		public static GroupedExpressionsExpression ToGroupedExpression(this IEnumerable<Expression> expressions, GroupedExpressionsExpressionStyle style = GroupedExpressionsExpressionStyle.Narrow)
		{
			return new GroupedExpressionsExpression(expressions.ToStatementsNormalized(), style);
		}

		public static ForEachExpression ForEach(ParameterExpression variableExpression, Expression target, Expression body)
		{
			return new ForEachExpression(variableExpression, target, body);
		}
	}
}