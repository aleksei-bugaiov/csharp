using System;
using System.Diagnostics;

namespace CSharp.Boxing
{
    internal static class Program
    {
        private static void Main()
        {
            const int iterations = 100000000;
            Test(iterations, Executor.ExecutorType.NonGeneric);
            Test(iterations, Executor.ExecutorType.Generic);
        }

        private static void Test(int iterations, Executor.ExecutorType executorType)
        {
            var classStopwatch = new Stopwatch();
            classStopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                var a = new A();
                switch (executorType)
                {
                    case Executor.ExecutorType.NonGeneric:
                        Executor.ExecuteNonGeneric(a);
                        break;
                    case Executor.ExecutorType.Generic:
                        Executor.ExecuteGeneric(a);
                        break;
                }
            }
            classStopwatch.Stop();

            var structStopwatch = new Stopwatch();
            structStopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                var b = new B();
                switch (executorType)
                {
                    case Executor.ExecutorType.NonGeneric:
                        Executor.ExecuteNonGeneric(b);
                        break;
                    case Executor.ExecutorType.Generic:
                        Executor.ExecuteGeneric(b);
                        break;
                }
            }
            structStopwatch.Stop();

            PrintResults(classStopwatch.ElapsedMilliseconds, structStopwatch.ElapsedMilliseconds, executorType);
        }

        private static void PrintResults(long classMilliseconds, long structMilliseconds, Executor.ExecutorType executorType)
        {
            Console.WriteLine(
                "-------------------------\n" +
                $"Test results for {executorType}\n" +
                $"Class milliseconds: {classMilliseconds}\n" +
                $"Struct milliseconds: {structMilliseconds}\n" +
                "-------------------------\n"
            );
        }

        private interface IFoo
        {
            long Bar();
        }

        private class A : IFoo
        {
            private long _a1;
            private long _a2;
            private long _a3;
            private long _a4;
            private long _a5;

            public long Bar()
            {
                _a1++;
                _a2++;
                _a3++;
                _a4++;
                _a5++;

                return _a1 + _a2 + _a3 + _a4 + _a5;
            }
        }

        private struct B : IFoo
        {
            private long _b1;
            private long _b2;
            private long _b3;
            private long _b4;
            private long _b5;

            public long Bar()
            {
                _b1++;
                _b2++;
                _b3++;
                _b4++;
                _b5++;

                return _b1 + _b2 + _b3 + _b4 + _b5;
            }
        }

        private static class Executor
        {
            public static void ExecuteGeneric<T>(T foo) where T : IFoo
            {
                var _ = foo.Bar();
            }

            public static void ExecuteNonGeneric(IFoo foo)
            {
                var _ = foo.Bar();
            }

            public enum ExecutorType
            {
                Generic,
                NonGeneric
            }
        }
    }
}
