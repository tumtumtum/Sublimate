﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sublimate.Generators.Objective
{
	public class ObjectiveLanguage
	{
		public static readonly SublimateType NSZoneType = new SublimateType("NSZone");
		public static readonly SublimateType NSMutableArray = new SublimateType("NSMutableArray");

		public static ConstructorInfo MakeConstructorInfo(Type declaringType, string initMethodName, params object[] args)
		{
			var parameterInfos = new List<ParameterInfo>();

			for (var i = 0; i < args.Length; i += 2)
			{
				parameterInfos.Add(new SublimateParameterInfo((Type)args[i], (string)args[i + 1]));
			}

			return new SublimateConstructorInfo(declaringType, initMethodName, parameterInfos.ToArray());
		}
	}
}
