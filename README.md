# RoslynWeave
An AOP code generator

# Initiative
As of now when this project is created, there is no way to intercept Roslyn compilation process, what most people wanted is simply to have a point where we can plug in some pre-compile logic to re-write the code in order to enable AOP, it took too long for .Net to respond so that I have to make another approach to trick it.

# Usage
- Take the code and compile (I'll possibly make a nuget later if I'm able to abstract it well)
- Execute `RoslynWeaveCli` and give it an argument as the path to a solution file, it will scan all the cs files and put the generated file into `AopManaged` folder of each project, for example [ExampleClass](RoslynWeaveTest/ExampleClass.cs) -> [AopManaged/ExampleClass](RoslynWeaveTest/AopManaged/ExampleClass.cs)
- The above process will skip `program.cs` files
- In `program.cs` change the using statement of the affected namespace and append `_AopWrapped` to it to use AOP, it will not take any changes if the namespace is unchanged, so that you can flip the use of it.
- Implement a class inheriting [DefaultAopContext](RoslynWeave/RoslynWeave/DefaultAopContext.cs), override the 6 intercept points to handle your AOP
* EnteringMethodAsync
* EnteringMethod
* ExitingMethodAsync
* ExitingMethod
* TryHandleExceptionAsync
* TryHandleException
- Give this class to [AopContextLocator](RoslynWeave/RoslynWeave/AopContextLocator.cs) with a factory method
- In the context class, the `CurrentFrame` object will provide metadata of current method, including the MehtodBase, as well as the parameters passed in
- Weavers and Aspects and Advice will come to help later
```

    public class MyContext: DefaultAopContext
    {
        public MyContext()
        {
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
- It re-writes all the methods to wrap in predefined template (essentially try/catch with error handling), and point the interception points to [IAopContext](RoslynWeave/IAopContext.cs) interface provided to the method by [AopContextLocator](RoslynWeave/AopContextLocator.cs) with an `AsyncLocal`
- In order to let the new code get compiled without modifying and tracking the original code, it's a trick to use the code rewriter to write a duplication of each `cs` file, with changing the namespace to avoid conflicts.
- for instance, if you have class defined in a file with namespace `ExampleNamespace`, with the default config it will generate another class with the same name but in namespace `ExampleNamespace_AopWrapped`, and also checks all the `using`s in all files that it processes, change to `using ExampleNamespace_AopWrapped;` if it sees `using ExampleNamespace;`, the new generate files are put into `AopManaged` folder with the same structure

Therefore in order to use it, use the `_AopWrapped` namespace in the program

# How to customize it
- [program.cs of the Cli](RoslynWeaveCli/program.cs) has a very basic main to iterate through files,  take the file make your own cli to find the files and store the output in your way
- Use the default wrapper to manage the code then derive from [DefaultAopContext](RoslynWeave/DefaultAopContext.cs) to make interception

- Or, Derive from [WrapperTemplate.cs](RoslynWeave/CodeRewriter/WrapperTemplate.cs), give the source code as string to [TemplateExtractor.cs](RoslynWeave/CodeRewriter/TemplateExtractor.cs) with a factory method.
- then the code rewriter will use your way to wrap the methods,
- it will replace `Body();` statement with original method body,
- it will replace `Default();` with a default value return statement, this is used when exception happened
- you can plug multiple copies of the original body, just be aware of the scopes so that the variable names don't get conflicted.
- [CodeRewriterConfig.cs](RoslynWeave/CodeRewriter/CodeRewriterConfig.cs) contains a very basic configuration, will extend it soon.

## Limitations
It's built as an intermediary solution while I'm waiting for .Net to release an intercept feature of the compiler. so it's expected to have limitations, I can't find a perfect solution except IL weaving, but, IL is too difficult to write and debug for many people. is impossible to me..
- It enlarges the codebase to more than twice, as it duplicates all files, and wraps all method bodies with try/catch
- It only deals with the typical coding style. for example if you prefer to use class full name in method instead of `using` at the begging, at least the first (current) version is unable to find and point to  
- At this (initial) stage of the project, I haven't implemented any weaving logic, nor any `Advice`s, `Aspect`s.
