using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TestEFCore
{
    public static class QueryableExtensions
    {
        private static readonly MethodInfo genericOrderByMethodInfo =
            typeof(Queryable).GetMethods()
                .Where(x => x.Name == "OrderBy" && x.GetParameters().Length == 2 && x.GetParameters()[1].ParameterType.Name.Contains("Expression"))!
                .First();

        public static IQueryable<T> ParametricOrderBy<T>(this IQueryable<T> q, string orderByPropertyName)
        {
            Type fieldType = typeof(T).GetProperty(orderByPropertyName)!.PropertyType;

            var par = Expression.Parameter(typeof(T), "p");
            object expr = Expression.Lambda(Expression.Property(par, orderByPropertyName), par);

            var met = genericOrderByMethodInfo.MakeGenericMethod(typeof(T), fieldType);

            q = (IQueryable<T>)met.Invoke(q, new object[] { q, expr })!;

            return q;
        }
    }
}
