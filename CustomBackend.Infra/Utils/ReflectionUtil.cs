using CustomBackend.Infra.Dtos.Result;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace CustomBackend.Infra.Utils
{
    public static class ReflectionUtil
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool apply, Expression<Func<T, bool>> predicate)
        {
            if (apply)
                return source.Where(predicate);

            return source;
        }

        public static IQueryable<T> WhereIfNotNull<T>(this IQueryable<T> source, object value, Expression<Func<T, bool>> predicate) =>
            source.WhereIf(
                value != null
                && value.ToString().IsValidString()
                && (!value.IsArray() || value.IsArrayWithItems())
            , predicate);



        public static string GetDisplayValue<T>(this T value) => ((DisplayAttribute)value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DisplayAttribute), true)?.FirstOrDefault())?.Name ?? value.ToString();



        public static bool IsValidGuid(this Guid? value) => value.HasValue && value != Guid.Empty;

        public static bool IsValidGuid(this Guid value) => value != Guid.Empty;

        public static Guid ToGuid(this object value)
        {
            if (value == null)
                return Guid.Empty;

            try
            {
                var stringValue = value?.ToString();

                if (!string.IsNullOrWhiteSpace(stringValue))
                    return Guid.Parse(stringValue);
            }
            catch
            {
            }

            return Guid.Empty;
        }



        public static int ToInt32(this object value, int defaultValue = 0)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal ToDecimal(this object value, decimal defaultValue = 0, bool fromBr = true)
        {
            if (value == null)
                return defaultValue;

            try
            {
                if (fromBr)
                    return Convert.ToDecimal(value, StringUtil.brCulture);

                return Convert.ToDecimal(value, StringUtil.usCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal? TryGetDecimal(this object value)
        {
            if (value == null)
                return null;

            try
            {
                var stringValue = value?.ToString();//.Replace(".", string.Empty).Replace(",", ".");
                return Convert.ToDecimal(stringValue, new CultureInfo("pt-BR"));
            }
            catch
            {
                return null;
            }
        }



        public static bool IsValidEnum<TEnum>(this TEnum value) where TEnum : struct => Enum.IsDefined(typeof(TEnum), value);

        public static string[] ListEnums<TEnum>() where TEnum : struct => Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(e => e.ToString()).ToArray();

        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            T result;
            return Enum.TryParse(value, true, out result) ? result : defaultValue;
        }

        public static CodeDescriptionResult[] ListEnumsAsCodigoDescricao<TEnum>() where TEnum : struct => Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(e => new CodeDescriptionResult { Code = e.ToString(), Description = e.GetDisplayValue() }).ToArray();

        public static CodeDescriptionResult[] BuildItemsList(
            this IEnumerable<CodeDescriptionResult> items,
            bool addFirstItem = true,
            string firstItemCode = null,
            string firstItemDescription = null
        )
        {
            var result = items.ToList();

            if (addFirstItem)
                result.Insert(0, new CodeDescriptionResult { Code = firstItemCode ?? string.Empty, Description = firstItemDescription ?? string.Empty });

            return result.ToArray();
        }

        public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source) => new PagedResult<T>(1, 1, 1, source.Count(), source.Count(), source);

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> e) => e == null || !e.Any();

        public static bool IsArray(this object value)
        {
            if (value == null)
                return false;

            if (value is IEnumerable<char>)
                return false;

            var result = value.GetType().GetInterface("IEnumerable`1") != null;
            return result;
        }

        public static bool IsArrayWithItems(this object value)
        {
            if (value.IsArray())
            {
                var result = ((IEnumerable)value).GetEnumerator().MoveNext();
                return result;
            }

            return false;
        }



        public static IQueryable<T> DynamicOrderBy<T>(this IQueryable<T> source, string orderColumn, bool orderAscending) =>
            ApplyOrder(source, orderColumn, orderAscending ? "OrderBy" : "OrderByDescending");

        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            var props = property.Split('.');
            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");

            Expression expr = arg;

            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }

            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });

            return (IOrderedQueryable<T>)result;
        }



        public static List<Type> ListTypesOf<TBase>(Assembly assembly = null)
        {
            var baseType = typeof(TBase);

            if (assembly == null)
                assembly = Assembly.GetAssembly(baseType);

            var result = assembly
                .GetTypes()
                .Where(type =>
                    type.BaseType != null
                    && (
                        (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == baseType)) // -> Generics, ex: CrudRepository<>
                        || (baseType.IsAssignableFrom(type) && !type.IsAbstract) // -> Non generics, ex: Repository
                    )
                .ToList();

            return result;
        }

        public static string GetNameDateVersionatedName(this Assembly assembly)
        {
            var name = assembly.GetName().Name;
            var date = assembly.GetAssemblyDate();
            var result = $"{name}-{date.ToString("dd-MM-yyyy-HH-mm-ss")}";
            return result;
        }

        private static DateTime GetAssemblyDate(this Assembly assembly) => File.GetLastWriteTime(assembly.Location);
    }
}
