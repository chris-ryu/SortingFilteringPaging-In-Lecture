using System.Linq.Expressions;
using System.Reflection;

namespace SortingFilteringPaging
{
    public static class ExpressionBuilder
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
        private static MethodInfo startsWithMethod =
        typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        private static MethodInfo endsWithMethod =
        typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

        public static Expression<Func<T, bool>> GetExpression<T>(IList<Filter> filters)
        {

            if (filters.Count == 0)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(T), "t");
            Expression exp = null;

            if (filters.Count == 1)
                exp = GetExpression<T>(param, filters[0]);
            else if (filters.Count == 2)
                exp = GetExpression<T>(param, filters[0], filters[1]);
            else
            {
                while (filters.Count > 0)
                {
                    var f1 = filters[0];
                    var f2 = filters[1];

                    if (exp == null)
                        exp = GetExpression<T>(param, filters[0], filters[1]);
                    else
                        exp = Expression.AndAlso(exp, GetExpression<T>(param, filters[0], filters[1]));

                    filters.Remove(f1);
                    filters.Remove(f2);

                    if (filters.Count == 1)
                    {
                        exp = Expression.AndAlso(exp, GetExpression<T>(param, filters[0]));
                        filters.RemoveAt(0);
                    }
                }
            }

            return Expression.Lambda<Func<T, bool>>(exp, param);
        }
        static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        private static Expression GetExpression<T>(ParameterExpression param, Filter filter)
        {
            MemberExpression member = Expression.Property(param, filter.PropertyName);
            ConstantExpression constant = Expression.Constant(filter.Value);
            Expression theFilter;
            if (filter.Value?.GetType() != member.Type)
            {
                theFilter = Expression.Constant(Convert.ChangeType(filter.Value, member.Type));
            }
            else
            {
                theFilter = constant;
            }

            switch (filter.Operation)
            {
                case Op.IsNull:
                    return Expression.Equal(member, Expression.Constant(null));
                // Add null checker in front of the condition using &&
                //    return Expression.AndAlso(nullCheck, theFilter);
                case Op.IsNotNull:
                    return Expression.NotEqual(member, Expression.Constant(null));
                case Op.Equals:
                    var exp = Expression.Equal(member, theFilter);
                    return exp;
                //   Console.WriteLine($"{exp.DebugView}");
                case Op.NotEquals:
                    return Expression.NotEqual(member, theFilter);

                case Op.GreaterThan:
                    return Expression.GreaterThan(member, theFilter);

                case Op.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, theFilter);

                case Op.LessThan:
                    return Expression.LessThan(member, theFilter);

                case Op.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, theFilter);

                case Op.Contains:
                    var nullCheck = Expression.NotEqual(member, Expression.Constant(null, typeof(object)));
                    theFilter = Expression.Call(member, containsMethod, constant);
                    return Expression.AndAlso(nullCheck, theFilter);

                case Op.StartsWith:
                    return Expression.Call(member, startsWithMethod, constant);

                case Op.EndsWith:
                    return Expression.Call(member, endsWithMethod, constant);
            }

            return null;
        }

        private static BinaryExpression GetExpression<T>
        (ParameterExpression param, Filter filter1, Filter filter2)
        {
            Expression bin1 = GetExpression<T>(param, filter1);
            Expression bin2 = GetExpression<T>(param, filter2);

            return Expression.AndAlso(bin1, bin2);
        }

        //order by
        private static readonly MethodInfo OrderByMethod =
        typeof(Queryable).GetMethods().Single(method =>
        method.Name == "OrderBy" && method.GetParameters().Length == 2);

        private static readonly MethodInfo OrderByDescendingMethod =
            typeof(Queryable).GetMethods().Single(method =>
            method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

        public static bool PropertyExists<T>(this IQueryable<T> source, string propertyName)
        {
            return typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                BindingFlags.Public | BindingFlags.Instance) != null;
        }

        public static IQueryable<T> OrderByProperty<T>(
           this IQueryable<T> source, string propertyName)
        {
            if (typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return null;
            }
            ParameterExpression paramterExpression = Expression.Parameter(typeof(T));
            Expression orderByProperty = Expression.Property(paramterExpression, propertyName);
            LambdaExpression lambda = Expression.Lambda(orderByProperty, paramterExpression);
            MethodInfo genericMethod =
              OrderByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);
            object ret = genericMethod.Invoke(null, new object[] { source, lambda });
            return (IQueryable<T>)ret;
        }

        public static IQueryable<T> OrderByPropertyDescending<T>(
            this IQueryable<T> source, string propertyName)
        {
            if (typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return null;
            }
            ParameterExpression paramterExpression = Expression.Parameter(typeof(T));
            Expression orderByProperty = Expression.Property(paramterExpression, propertyName);
            LambdaExpression lambda = Expression.Lambda(orderByProperty, paramterExpression);
            MethodInfo genericMethod = OrderByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);
            object ret = genericMethod.Invoke(null, new object[] { source, lambda });
            return (IQueryable<T>)ret;
        }
    }


    public enum Op
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        StartsWith,
        EndsWith,
        Asc,
        Desc,
        IsNull,
        IsNotNull,
    }
    public class Filter
    {
        public string PropertyName { get; set; }
        public Op Operation { get; set; }
        public object Value { get; set; }
    }
}