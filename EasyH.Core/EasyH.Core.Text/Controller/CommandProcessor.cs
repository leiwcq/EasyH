using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyH.Core.Text.Controller
{
    public class CommandProcessor 
	{
		private object[] Controllers { get; set; }

		private readonly Dictionary<string, object> contextMap;

		public CommandProcessor(object[] controllers)
		{
            Controllers = controllers;

            contextMap = new Dictionary<string, object>();
            foreach (var x in controllers.ToList())
            {
                contextMap[x.GetType().Name] = x;
            }
        }

		public void Invoke(string commandUri)
		{
			var actionParts = commandUri.Split(new[] { "://" }, StringSplitOptions.None);

			var controllerName = actionParts[0];

			var pathInfo = PathInfo.Parse(actionParts[1]);

		    if (!contextMap.TryGetValue(controllerName, out var context))
		        throw new Exception("UnknownContext: " + controllerName);

            var methodName = pathInfo.ActionName;

            var method = context.GetType().GetMethods().First(
                c => c.Name == methodName && c.GetParameters().Length == pathInfo.Arguments.Count);

			var methodParamTypes = method.GetParameters().Select(x => x.ParameterType);

			var methodArgs = ConvertValuesToTypes(pathInfo.Arguments, methodParamTypes.ToList());

			try
			{
				method.Invoke(context, methodArgs);
			}
			catch (Exception ex)
			{
				throw new Exception("InvalidCommand", ex);
			}
		}

		private static object[] ConvertValuesToTypes(IList<string> values, IList<Type> types)
		{
			var convertedValues = new object[types.Count];
			for (var i = 0; i < types.Count; i++)
			{
				var propertyValueType = types[i];
				var propertyValueString = values[i];
				var argValue = TypeSerializer.DeserializeFromString(propertyValueString, propertyValueType);
				convertedValues[i] = argValue;
			}
			return convertedValues;
		}
	}
}