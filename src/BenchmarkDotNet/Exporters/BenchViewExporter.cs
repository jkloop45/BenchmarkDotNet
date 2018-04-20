﻿using System;
using System.Collections;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Extensions;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Running;

namespace BenchmarkDotNet.Exporters
{
    public class BenchViewExporter
    {
        public static string GetBenchmarkName(Benchmark benchmark)
        {
            var type = benchmark.Target.Type;
            var method = benchmark.Target.Method;

            // we can't just use type.FullName because we need sth different for generics (it reports SimpleGeneric`1[[System.Int32, mscorlib, Version=4.0.0.0)
            var name = new StringBuilder();

            if (!string.IsNullOrEmpty(type.Namespace))
                name.Append(type.Namespace).Append('.');

            name.Append(GetNestedTypes(type));

            name.Append(type.Name).Append('.');

            name.Append(method.Name);

            if (benchmark.HasArguments)
                name.Append(GetMethodArguments(method, benchmark.Parameters));

            return name.ToString();
        }

        private static string GetNestedTypes(Type type)
        {
            var nestedTypes = "";
            Type child = type, parent = type.DeclaringType;
            while (child.IsNested && parent != null)
            {
                nestedTypes = parent.Name + "+" + nestedTypes;

                child = parent;
                parent = parent.DeclaringType;
            }

            return nestedTypes;
        }

        private static string GetMethodArguments(MethodInfo method, ParameterInstances benchmarkParameters)
        {
            var methodParameters = method.GetParameters();
            var arguments = new StringBuilder(methodParameters.Length * 20).Append('(');

            for (int i = 0; i < methodParameters.Length; i++)
            {
                if (i > 0)
                    arguments.Append(", ");

                arguments.Append(methodParameters[i].Name).Append(':').Append(' ');
                arguments.Append(GetArgument(benchmarkParameters.GetArgument(methodParameters[i].Name).Value, methodParameters[i].ParameterType));
            }

            return arguments.Append(')').ToString();
        }

        private static string GetArgument(object argumentValue, Type argumentType)
        {
            if (argumentValue == null)
                return "null";

            if (argumentValue is string text)
                return $"\"{text}\"";
            if (argumentValue is char character)
                return $"'{character}'";
            if (argumentValue is DateTime time)
                return time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");

            if (argumentType != null && argumentType.IsArray)
                return $"[{((IEnumerable) argumentValue).Join(value => GetArgument(value, null), ", ", string.Empty)}]";

            return argumentValue.ToString();
        }
    }
}