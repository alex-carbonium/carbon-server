using System;

namespace Carbon.Framework.Util
{
	public static class Guard
	{
		public static void AssertArgument(bool argumentCondition, string message)
		{
			if (!argumentCondition)
			{
				throw new ArgumentException(message);
			}
		}
		public static void ArgumentNotNull(object obj, string message)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(message);
			}
		}

		public static void Assert(bool condition, string message)
		{
			if (!condition)
			{
				throw new InvalidOperationException(message);
			}
		}
		public static void AssertNull(object obj, string message)
		{
			if (obj != null)
			{
				throw new InvalidOperationException(message);
			}
		}
		public static void AssertNotNull(object obj, string message)
		{
			if (obj == null)
			{
				throw new InvalidOperationException(message);
			}
		}

		public static void Fail(string message)
		{
			throw new InvalidOperationException(message);
		}
		public static void FailSwitchDefault(object value)
		{
			Fail(string.Format("The value '{0}' in the switch operator is not supported", value));
		}
	}
}
