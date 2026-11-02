using System;
using System.Linq;
using System.Reflection;

namespace LinkGrab.Tests
{
    public class TestFailure : Exception
    {
        public TestFailure(string message) : base(message) { }
    }

    public static class Check
    {
        public static void Equal(object expected, object actual)
        {
            if (!Equals(expected, actual)) throw new TestFailure(string.Format("esperaba [{0}], obtuve [{1}]", expected, actual));
        }

        public static void True(bool value, string what)
        {
            if (!value) throw new TestFailure(what);
        }
    }

    // Runs every public static method of every class whose name ends in "Tests".
    public static class TestRunner
    {
        static int Main()
        {
            var tests = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Name.EndsWith("Tests"))
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

            int passed = 0;
            int failed = 0;
            foreach (MethodInfo test in tests)
            {
                try
                {
                    test.Invoke(null, null);
                    passed++;
                }
                catch (TargetInvocationException e)
                {
                    failed++;
                    Console.WriteLine("FALLA {0}.{1}: {2}", test.DeclaringType.Name, test.Name, e.InnerException.Message);
                }
            }

            Console.WriteLine("Tests: {0} correctos, {1} fallidos", passed, failed);
            return failed == 0 ? 0 : 1;
        }
    }
}
