# RoslynWeave
An AOP code generator

# Initiative
This AOP framework sets up a goal to inject C# code during compile time, by weaving with original C# code instead of IL it provides a much higher reliability and ease of use

# The code generating approach
- This libary leverages https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/ to intercept the roslyn compiler so that to generate AOP managed code base on the existing barebone user code
- HOWEVER, The Source Generator in fact is designed to only support code analysing and generating, it [doesn't support code-rewritting](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.cookbook.md#code-rewriting)
- Therefore, this libary cannot generate new code with AOP enabled to replace your code, instead, it copies all code that's being compiled, apply the AOP wrapping, then generate them into a new separate namespace, by default it names the new namespace with a `_AopManaged` suffix, which means, the AOP and non-AOP code co-exists, to flip between, simply change the using derives from the entry point
- The using derives inside of generated are updated to reach to the generated namespace

# The AOP Approach
- In [CodeFramework](RoslynWeave/CodeFramework) it defines a set of AOP related classes, called by method at entering or leaving or on-error, 
- The method are rewritten by the intercepter to get the method body wrapped accordingly to the [WrapperTemplate.cs](RoslynWeave/CodeRewriter/WrapperTemplate.cs), the actual method body will be placed into where line `Body();` is, which is inside of a try catch statemtment with standard AOP code inplace as well
- The WrapperTemplate can be customized, it is only required to have `SyncMethod` and `AsyncMethod` implemented with `Body();` line
- It will soon be made able to accept a configured external template in the project
- Using `AopIgnore` attribute will exclude the code rewritting on class or individual method

# It needs .NET5.0 compiler to work !!

# Example
- Clone this project and build it with `dotnet` version 5, the RoslynWeaveTests project will intercepted and added with new namespaces
- The test (inprogress) shows calling to generated code

# Next Step
- the debugging might still be tricking at the beggining, will look at the options to either try to maintain the line numbers to the pdbs (not sure if possible), or wait for later visualstudio adds support for debugging this, as mentioned in the news 

# Usage 
- Take the code and compile RoslynWeave (I'll make a nuget later)
- Use it as both Analyzer item and project reference : 
```
 <ItemGroup>
    <ProjectReference Include="..\RoslynWeave\RoslynWeave.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
    <ProjectReference Include="..\RoslynWeave\RoslynWeave.csproj" />
  </ItemGroup>
```
- Implement a class deriving [AopContextFrame](RoslynWeave/CodeFramework/AopContextFrame.cs)
- Implement a class inheriting [DefaultAopContext](RoslynWeave/CodeFramework/DefaultAopContext.cs), override the 8 intercept points to handle your AOP
* EnteringMethodAsync
* EnteringMethod
* ExitingMethodAsync
* ExitingMethod
* TryHandleExceptionAsync
* TryHandleException
* NeedsProfileAsync
* NeedsProfile
- Give this class to [AopContextLocator](RoslynWeave/AopContextLocator.cs) with a factory method
- In the context class, the `CurrentFrame` object will provide metadata of current method, including the MehtodBase, as well as the parameters passed in
- `GetCurrentStackTrace()` returns the stack trace based on all frames it went through.
- Weavers and Aspects and Advice may come to help later
```cs

    public class MyContext: DefaultAopContext
    {
        public MyContext()
        {
            //A very basic example to describle how to let the AopContextLoctor resolve the Context.
            //You can add some more logics such as letting your IOC container to resolve.
            //AopContextLocator resolves once per aync context so that the the AopContext is scoped.
            //Will possibly implement more complex logic for resolving to handle more scenarios
            AopContextLocator.Initialize(() => this);
        }

        protected override Task EnteringMethodAsync(MethodMetadata method)
        {
            return base.EnteringMethodAsync(method);
        }

        protected override void EnteringMethod(MethodMetadata method)
        {
            base.EnteringMethod(method);
        }

        protected override void ExitingMethod(MethodMetadata method, double timeSpent)
        {
            base.ExitingMethod(method, timeSpent);
        }

        protected override Task ExitingMethodAsync(MethodMetadata method, double timeSpent)
        {
            return base.ExitingMethodAsync(method, timeSpent);
        }

        public override bool TryHandleException(Exception data)
        {
            return base.TryHandleException(data);
        }

        public override Task<bool> TryHandleExceptionAsync(Exception data)
        {
            return base.TryHandleExceptionAsync(data);
        }
    }
```

## How it works
- Instead of weaving to specific method with specific logic at compile time, it weaves "Management" to all the method so that inteception can be handled at run time.
- It re-writes all the methods to wrap in a predefined template (essentially try/catch with error handling), and point the interception points to [IAopContext](RoslynWeave/CodeFramework/IAopContext.cs) interface provided to the method by [AopContextLocator](RoslynWeave/CodeFramework/AopContextLocator.cs) with an `AsyncLocal`
- In order to let the new code get compiled without modifying and tracking the original code, it's a trick to use the code rewriter to write a duplication of each `cs` file, with changing the namespace to avoid conflicts.
- for instance, if you have class defined in a file with namespace `ExampleNamespace`, with the default config it will generate another class with the same name but in namespace `ExampleNamespace_AopWrapped`, and also checks all the `using`s in all files that it processes, change to `using ExampleNamespace_AopWrapped;` if it sees `using ExampleNamespace;`, the new generate files are put into `AopManaged` folder with the same structure

Therefore in order to use it, use the `_AopWrapped` namespace in the program

# How to customize it
- Use the default wrapper to manage the code then derive from [DefaultAopContext](RoslynWeave/CodeFramework/DefaultAopContext.cs) to make interception

- Or, Derive from [WrapperTemplate.cs](RoslynWeave/CodeRewriter/WrapperTemplate.cs), give the source code as string to [TemplateExtractor.cs](RoslynWeave/CodeRewriter/TemplateExtractor.cs) with a factory method.
- then the code rewriter will use your way to wrap the methods,
- it will replace `Body();` statement with original method body,
- it will replace `Default();` with a default value return statement, this is used when exception happened
- you can plug multiple copies of the original body, just be aware of the scopes so that the variable names don't get conflicted.
- Multiple default value return (`Default();`)is also allowed and will not likely to cause conflict
- [CodeRewriterConfig.cs](RoslynWeave/CodeRewriter/CodeRewriterConfig.cs) contains a very basic configuration, will extend it soon.

## Limitations
It's built as an intermediary solution while I'm waiting for .Net to release an intercept feature of the compiler. so it's expected to have limitations, I can't find a perfect solution except IL weaving, but, IL is too difficult to write and debug for many people. is impossible to me.
- It enlarges the codebase to more than twice, as it duplicates all files, and wraps all method bodies with try/catch
- It only deals with the typical coding style. for example if you prefer to use class full name in method instead of `using` at the begging, at least the first (current) version is unable to find and point to  
- At this (initial) stage of the project, I haven't implemented any weaving logic, nor any `Advice`s, `Aspect`s. This can be added on top of this basic framework

## Something good about this trick
- `DynamicProxy` does a similar work to intecept class member invokations at run time, it is easier to use but it can't intercept method calls within a class
- It does a similar thing as other IL weaving libraries on post-build, only it is pre-build, what's generated is C# and you can see what's generated.
- It doesn't write a lot of fixed-mangic code to rewrite your source code, instead, it reads from a template that can be customized, in this source code project, the template is in the same project with what the generated code will be calling, therefore, compilation of this libary will ensure the template is written properly.
- In the `AopContext` object of yours, you can call/inject anything else that you are used to such as loggings and metrics interfaces.
- It has a big cost of duplicating all source code, although, you can also customize the cli main to to have the output go to other another isolated solution path as a rewritten-target separated from original source.
- In a nutshell, it's only the code rewriter that can help you re-write the code, anything else is designed to be customized.
