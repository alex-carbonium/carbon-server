using System;
using System.Linq.Expressions;

namespace Carbon.Framework.Util
{
	public static class ExpressionUtil
	{
		private static string GetPropertyNameRecursive(MemberExpression memberExpression)
		{
			if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
			{
				return GetPropertyNameRecursive((MemberExpression)memberExpression.Expression)
					+ "."
					+ memberExpression.Member.Name;
			}
			return memberExpression.Member.Name;
		}

		public static string GetPropertyName<T, TPropReturn>(Expression<Func<T, TPropReturn>> expression)
		{
			var memberExpression = expression.Body as MemberExpression;
			Guard.AssertNotNull(memberExpression, "Invalid member expression");
			return GetPropertyNameRecursive(memberExpression);
		}
	}
}
