//RoslynWeave auto generated code.
//RoslynWeave auto generated code.
//RoslynWeave auto generated code.
//RoslynWeave auto generated code.
using System;
using Xunit;
using RoslynWeave;
using System.Diagnostics;
using Xunit.Abstractions;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RoslynWeaveTests_AopWrapped_AopWrapped_AopWrapped_AopWrapped
{
    public class MyTestClass
    {
        private readonly ITestOutputHelper _output;
        public MyTestClass(ITestOutputHelper output)
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod(), ("output", (object)output ?? typeof(ITestOutputHelper)));
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            try
            {
                var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod(), ("output", (object)output ?? typeof(ITestOutputHelper)));
                AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
                try
                {
                    var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod(), ("output", (object)output ?? typeof(ITestOutputHelper)));
                    AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
                    try
                    {
                        var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod(), ("output", (object)output ?? typeof(ITestOutputHelper)));
                        AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
                        try
                        {
                            _output = output;
                        }
                        catch (Exception exByAop)
                        {
                            if (AopContextLocator.AopContext.TryHandleException(exByAop))
                            {
                                return;
                            }

                            throw;
                        }
                        finally
                        {
                            AopContextLocator.AopContext.ExitFrame();
                        }
                    }
                    catch (Exception exByAop)
                    {
                        if (AopContextLocator.AopContext.TryHandleException(exByAop))
                        {
                            return;
                        }

                        throw;
                    }
                    finally
                    {
                        AopContextLocator.AopContext.ExitFrame();
                    }
                }
                catch (Exception exByAop)
                {
                    if (AopContextLocator.AopContext.TryHandleException(exByAop))
                    {
                        return;
                    }

                    throw;
                }
                finally
                {
                    AopContextLocator.AopContext.ExitFrame();
                }
            }
            catch (Exception exByAop)
            {
                if (AopContextLocator.AopContext.TryHandleException(exByAop))
                {
                    return;
                }

                throw;
            }
            finally
            {
                AopContextLocator.AopContext.ExitFrame();
            }
        }

        [Fact]
        public void Test1()
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod());
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            try
            {
                var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod());
                AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
                try
                {
                    var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod());
                    AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
                    try
                    {
                        var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod());
                        AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
                        try
                        {
                            var path = @"../../../ExampleClass.cs";
                            string code = null;
                            using (var sr = new StreamReader(path))
                                code = sr.ReadToEnd();
                            var codeRewriter = new CodeRewriter(new CodeRewriterConfig(), new TemplateExtractor());
                            var str = codeRewriter.Wrap(code);
                            _output.WriteLine(str);
                            using (var sw = new StreamWriter(path.Replace(".cs", ".out.cs")))
                                sw.Write(str);
                            Assert.True(false);
                        }
                        catch (Exception exByAop)
                        {
                            if (AopContextLocator.AopContext.TryHandleException(exByAop))
                            {
                                return;
                            }

                            throw;
                        }
                        finally
                        {
                            AopContextLocator.AopContext.ExitFrame();
                        }
                    }
                    catch (Exception exByAop)
                    {
                        if (AopContextLocator.AopContext.TryHandleException(exByAop))
                        {
                            return;
                        }

                        throw;
                    }
                    finally
                    {
                        AopContextLocator.AopContext.ExitFrame();
                    }
                }
                catch (Exception exByAop)
                {
                    if (AopContextLocator.AopContext.TryHandleException(exByAop))
                    {
                        return;
                    }

                    throw;
                }
                finally
                {
                    AopContextLocator.AopContext.ExitFrame();
                }
            }
            catch (Exception exByAop)
            {
                if (AopContextLocator.AopContext.TryHandleException(exByAop))
                {
                    return;
                }

                throw;
            }
            finally
            {
                AopContextLocator.AopContext.ExitFrame();
            }
        }
    }
}