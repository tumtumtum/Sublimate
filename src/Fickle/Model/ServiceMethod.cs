﻿using System.Collections.Generic;

namespace Fickle.Model
{
	public class ServiceMethod
	{
		public string Name { get; set; }
		
		[ServiceAnnotation] 
		public string Path { get; set; }

		[ServiceAnnotation] 
		public string ReturnFormat { get; set; }

		[ServiceAnnotation]
		public string ContentFormat { get; set; }

		[ServiceAnnotation]
		public string RequestFormat { get; set; }

		[ServiceAnnotation] 
		public bool Secure { get; set; }

		[ServiceAnnotation] 
		public bool Authenticated { get; set; }

		[ServiceAnnotation] 
		public string Method { get; set; }

		[ServiceAnnotation] 
		public string Returns { get; set; }

		[ServiceAnnotation]
		public string Content { get; set; }

		public ServiceParameter ContentServiceParameter { get; set; }

		public List<ServiceParameter> Parameters { get; set; }
	}
}
