using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace CastIt.Shared.Models;

public class ToLog
{
    private const string Separator = "_";

    private static readonly string[] ReservedSuffixes =
    {
        "HostedService"
    };

    private static readonly Type[] LoggerTypes =
    {
        typeof(ILoggerFactory),
        typeof(ILogger),
        typeof(ILogger<>),
    };


    public string? AssemblyFullName { get; }
    public string LogFileName { get; }
    public bool Filtered { get; }

    public ToLog(Type type)
        : this(type, GenerateFilename(type.Name))
    {
    }

    public ToLog(Type type, string filename)
        : this(filename)
    {
        AssemblyFullName = type.FullName;
        Filtered = true;
    }

    public ToLog(string filename)
    {
        LogFileName = filename;

        if (!Path.HasExtension(LogFileName))
        {
            LogFileName += "_.txt";
        }
    }

    public static List<ToLog> From<TClass>()
    {
        Type type = typeof(TClass);
        Assembly assembly = type.Assembly;
        return From(type.Namespace!, assembly);
    }

    public static List<ToLog> From(Type type)
    {
        Assembly assembly = type.Assembly;
        return From(type.Namespace!, assembly);
    }

    public static List<ToLog> From(string @namespace, Assembly assembly)
    {
        return assembly.GetTypes().Where(t => IsMatch(@namespace, t))
            .Select(type => new ToLog(type))
            .ToList();
    }

    public static bool IsMatch(string @namespace, Type type)
    {
        bool baseMatch = type is { IsClass: true, IsAbstract: false } &&
                         Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute)) == null &&
                         !string.IsNullOrWhiteSpace(type.Namespace) &&
                         type.Namespace.Contains(@namespace);

        if (!baseMatch)
        {
            return false;
        }

        List<Type> parameterTypes = type.GetConstructors()
            .SelectMany(ctor => ctor.GetParameters())
            .Select(p => p.ParameterType)
            .ToList();

        foreach (Type t in parameterTypes)
        {
            if (LoggerTypes.Contains(t))
            {
                return true;
            }

            if (LoggerTypes.Any(lt => lt.IsAssignableFrom(t)))
            {
                return true;
            }
        }

        return false;
    }

    public static string GenerateFilename(string typeName)
    {
        foreach (string reserved in ReservedSuffixes)
        {
            if (!typeName.Contains(reserved, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            string val = string.Concat(
                reserved[0].ToString().ToUpperInvariant(),
                reserved.ToLowerInvariant().AsSpan(1));
            typeName = typeName.Replace(reserved, val, StringComparison.OrdinalIgnoreCase);
        }

        string[] parts = Regex.Split(typeName, @"(?<!^)(?=[A-Z])");
        if (parts.Length < 2)
        {
            throw new ArgumentOutOfRangeException(typeName, "The type name cannot be split");
        }

        string suffix = parts.Last().ToLowerInvariant();
        string prefix = string.Join(Separator, parts.Take(parts.Length - 1)).ToLowerInvariant();
        return string.Join(Separator, suffix, prefix);
    }
}