using NUnit.Framework;
using RoslynNUnitLight;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AsyncVoidAnalyzer.Test
{
    [TestFixture]
    public class DiagnosticTests : AnalyzerTestFixture
    {
        #region Configuration

        protected override string LanguageName
        {
            get
            {
                return Microsoft.CodeAnalysis.LanguageNames.CSharp;
            }
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AsyncVoidAnalyzer();
        }

        #endregion

        [Test]
        public void NoCode_HasNoDiagnostics()
        {
            var code = @"";

            NoDiagnostic(code, AsyncVoidAnalyzer.DiagnosticId);
        }

        [Test]
        public void ReturningTask_HasNoDiagnostics()
        {
            var code = @"
    using System;
    using System.Threading.Tasks;

    namespace AsyncTest
    {
        class AsyncTestClass
        {
            public async Task ReturningTask()
            {
                await Task.Delay(0);
            }
        }
    }";

            NoDiagnostic(code, AsyncVoidAnalyzer.DiagnosticId);
        }

        [Test]
        public void Catching_HasNoDiagnostics()
        {
            var code = @"
    using System;
    using System.Threading.Tasks;

    namespace AsyncTest
    {
        class AsyncTestClass
        {
            public async void Catching()
            {
                try
                {
                    await Task.Delay(0);
                }
                catch (Exception)
                {
                }
            }
        }
    }";

            NoDiagnostic(code, AsyncVoidAnalyzer.DiagnosticId);
        }

        [Test]
        public void NotCatching_HasDiagnostics()
        {
            var code = @"
    using System;
    using System.Threading.Tasks;

    namespace AsyncTest
    {
        class AsyncTestClass
        {
            public async void NotCatching()
            {
                [|await Task.Delay(0)|];
            }
        }
    }";

            HasDiagnostic(code, AsyncVoidAnalyzer.DiagnosticId);
        }


        [Test]
        public void AsyncVoidLambdaNotCatching_HasDiagnostics()
        {
            // Note that this has a try/catch around the assignment (but not the actual await!),
            // just to try to fool the analyzer

            var code = @"
    using System;
    using System.Threading.Tasks;

    namespace AsyncTest
    {
        class AsyncTestClass
        {
            public void NotCatching()
            {
                try
                {
                    Action action = async () =>
                    {
                        [|await Task.Delay(0)|];
                    };
                }
                catch {}
            }
        }
    }";

            HasDiagnostic(code, AsyncVoidAnalyzer.DiagnosticId);
        }


        [Test]
        public void AsyncTaskLambda_HasNoDiagnostics()
        {
            var code = @"
    using System;
    using System.Threading.Tasks;

    namespace AsyncTest
    {
        class AsyncTestClass
        {
            public void AsyncTaskLambda()
            {
                Func<Task> func = async () =>
                {
                    await Task.Delay(0);
                };
            }
        }
    }";

            NoDiagnostic(code, AsyncVoidAnalyzer.DiagnosticId);
        }

        [Test]
        public void AsyncVoidLambdaCatching_HasNoDiagnostics()
        {
            var code = @"
    using System;
    using System.Threading.Tasks;

    namespace AsyncTest
    {
        class AsyncTestClass
        {
            public void Catching()
            {
                Action action = async () =>
                {
                    try
                    {
                        await Task.Delay(0);
                    }
                    catch {}
                };
            }
        }
    }";

            NoDiagnostic(code, AsyncVoidAnalyzer.DiagnosticId);
        }

        [Test]
        public void AsyncVoidDelegateNotCatching_HasDiagnostics()
        {
            var code = @"
    using System;
    using System.Threading.Tasks;

    namespace AsyncTest
    {
        class AsyncTestClass
        {
            public void NotCatching()
            {
                Action action = async delegate () { [|await Task.Delay(0)|]; };
            }
        }
    }";

            HasDiagnostic(code, AsyncVoidAnalyzer.DiagnosticId);
        }

        [Test]
        public void AsyncVoidExpressionNotCatching_HasDiagnostics()
        {
            var code = @"
    using System;
    using System.Threading.Tasks;

    namespace AsyncTest
    {
        class AsyncTestClass
        {
            public void NotCatching()
            {
                Action action = async () => [|await Task.Delay(0)|];
            }
        }
    }";

            HasDiagnostic(code, AsyncVoidAnalyzer.DiagnosticId);
        }
    }
}
