using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TheOrigin.Framework.AOP
{
    public class Utils
    {
        public static string GetMapPropertyName<T>(Expression<Func<T, object>> MapPropertyExpression)
        {
            Expression expression = MapPropertyExpression.Body;

            if (expression.NodeType == ExpressionType.Constant)
            {
                if (!(expression.Type == typeof(string)))
                    throw new InvalidOperationException("ERROR_MAPPING_NAME_MUST_BE_STRING");
                else
                    return (expression as ConstantExpression).Value.ToString();
            }

            if (expression.NodeType == ExpressionType.Convert)
                expression = ((UnaryExpression)expression).Operand;

            if (expression is MemberExpression)
                return (expression as MemberExpression).Member.Name;

            return string.Empty;
        }
    }
}
