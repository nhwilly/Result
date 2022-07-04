using System;
using System.Linq;

namespace Ardalis.Result;
public static class GenericTypeHelpers
{
  public static string GetInnerMostGenericTypeName(Type type)
  {
    if (!type.IsGenericType) return type.Name;
    var genericType = type.GetGenericArguments().FirstOrDefault();
    if (genericType is null) return type.Name;
    return GetInnerMostGenericTypeName(genericType);
  }
}
