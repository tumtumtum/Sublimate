﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Description;
using Dryice.Model;
using System.Web.Http;
using Platform;

namespace Dryice.Reflectors.WebApiRuntime
{
	public class WebApiRuntimeServiceModelReflector
		: ServiceModelReflector
	{
		public HttpConfiguration Configuration { get; private set; }
		public ServiceModelReflectionOptions Options { get; private set; }

		public WebApiRuntimeServiceModelReflector(ServiceModelReflectionOptions options, HttpConfiguration configuration)
		{
			this.Options = options;
			this.Configuration = configuration;
		}

		private static bool IsBaseType(Type type)
		{
			return type == null || type == typeof(object) || type == typeof(Enum) || type == typeof(ValueType);
		}

		private static void AddType(ISet<Type> set, Type type)
		{
			if (IsBaseType(type))
			{
				return;
			}

			 while (!IsBaseType(type))
			{
				set.Add(type);

				type = type.BaseType;
			}
		}

		private static IEnumerable<Type> GetReferencedTypes(IEnumerable<ApiDescription> descriptions)
		{
			var types = new HashSet<Type>();

			foreach (var description in descriptions)
			{
				AddType(types, description.ActionDescriptor.ReturnType);

				foreach (var type in description.ParameterDescriptions.Select(c => c.ParameterDescriptor.ParameterType))
				{
					AddType(types, type);
				}
			}

			return types.Where(c => c != null);
		}

		private static string GetTypeName(Type type)
		{
			if (type == null)
			{
				return null;
			}

			if (TypeSystem.IsPrimitiveType(type))
			{
				return TypeSystem.GetPrimitiveName(type).ToLower();
			}

			if (type == typeof(object))
			{
				return null;
			}

			if (typeof(IEnumerable<>).IsAssignableFromIgnoreGenericParameters(type))
			{
				return GetTypeName(type.GetGenericArguments()[0]) + "[]";
			}

			return type.Name;
		}

		public override ServiceModel Reflect()
		{
			var descriptions = Configuration.Services.GetApiExplorer().ApiDescriptions;

			var enums = new List<ServiceEnum>();
			var classes = new List<ServiceClass>();
			var gateways = new List<ServiceGateway>();

			var referencedTypes = GetReferencedTypes(descriptions).ToList();

			foreach (var enumType in referencedTypes.Where(c => c.BaseType == typeof(Enum)))
			{
				var serviceEnum = new ServiceEnum
				{
					Name = GetTypeName(enumType),
					Values = ((int[])enumType.GetEnumValues()).Select(c => new ServiceEnumValue { Name = enumType.GetEnumName(c), Value = c}).ToList()
				};

				enums.Add(serviceEnum);
			}

			foreach (var type in referencedTypes
				.Where(TypeSystem.IsNotPrimitiveType)
				.Where(c => c.BaseType != typeof(Enum))
				.Where(c => !c.IsInterface)
				.Where(c => !typeof(IList<>).IsAssignableFromIgnoreGenericParameters(c)))
			{
				var baseTypeName = GetTypeName(type.BaseType);
				var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Select(c => new ServiceProperty
					{
						Name = c.Name,
						TypeName = GetTypeName(c.PropertyType)
					}).ToList();

				var serviceClass = new ServiceClass
				{
					Name = GetTypeName(type),
					BaseTypeName = baseTypeName,
 					Properties = properties
				};

				classes.Add(serviceClass);
			}
			
			return new ServiceModel(enums, classes, gateways);
		}
	}
}
