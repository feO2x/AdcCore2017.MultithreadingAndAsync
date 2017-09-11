// This example was created based on the contents of Jon Skeet's excellent Pluralsight course "Asynchronous C# 5.0":
// https://app.pluralsight.com/library/courses/skeet-async/table-of-contents

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable RedundantAssignment
#pragma warning disable 169

// ReSharper disable MemberCanBePrivate.Local

namespace AsyncDecompiled
{
    public static class Program
    {
        public static async Task Main()
        {
            var readers = InitializeReaders();
            var numberOfWords = await Async.CalculateTotalNumberOfWordsAsync(readers);
            readers.DisposeAll();
            Console.WriteLine($"The {readers.Length} streams contain a total number of {numberOfWords} words.");
            Console.ReadLine();
        }

        private static TextReader[] InitializeReaders()
        {
            return new TextReader[]
                   {
                       InitializeStreamReader("TextInFile.txt"),
                       InitializeStreamReader("SomeHierarchicalText.json")
                   };
        }

        private static StreamReader InitializeStreamReader(string path)
        {
            return new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous));
            //return new StreamReader(path);
        }

        public static async Task<int> CalculateTotalNumberOfWordsAsync(IEnumerable<TextReader> readers)
        {
            var totalNumberOfWords = 0;
            foreach (var reader in readers)
            {
                var content = await reader.ReadToEndAsync();
                totalNumberOfWords += content.CalculateNumberOfWords();
            }

            return totalNumberOfWords;
        }
    }

    public static class Async
    {
        //[DebuggerStepThrough]
        [AsyncStateMachine(typeof(DemoStateMachine))]
        public static Task<int> CalculateTotalNumberOfWordsAsync(IEnumerable<TextReader> readers) // Starting point
        {
            var machine = new DemoStateMachine
                          {
                              Readers = readers, // Copy all method parameter values to the state machine
                              Builder = AsyncTaskMethodBuilder<int>.Create(), // Create an AsyncTaskMethodBuilder, the reusable part of each state machine
                              State = -1 // Initial state
                          };
            machine.Builder.Start(ref machine); // Start the state machine via the builder, this call might complete the task
            return machine.Builder.Task; // Return the associated task
        }

        [CompilerGenerated]
        private struct DemoStateMachine : IAsyncStateMachine
        {
            // Parameters and Variables
            public IEnumerable<TextReader> Readers;

            public int TotalNumberOfWords;
            public IEnumerator<TextReader> Enumerator;
            public TextReader Reader;
            public string Content;

            // Async Infrastructure
            public AsyncTaskMethodBuilder<int> Builder;

            public int State; // -2 = done (successful or exception caught), -1 = running, other states for different await statements
            public TaskAwaiter<string> TaskAwaiter;
            public object Stack; // for expressions like (await a) + (await b)

            void IAsyncStateMachine.MoveNext()
            {
                var result = default(int);
                try
                {
                    var executeFinallyBlock = true;
                    switch (State)
                    {
                        case 0:
                            goto FakeAwaitContinuation;
                    }

                    // Initially, State is -1
                    TotalNumberOfWords = 0;
                    Enumerator = Readers.GetEnumerator();
                    FakeAwaitContinuation:
                    try
                    {
                        if (State == 0) goto RealAwaitContinuation;
                        goto LoopCondition;

                        LoopBody:
                        Reader = Enumerator.Current;
                        var localTaskAwaiter = Reader.ReadToEndAsync().GetAwaiter();
                        if (localTaskAwaiter.IsCompleted)
                            goto AwaitCompletion;

                        State = 0;
                        TaskAwaiter = localTaskAwaiter;
                        Builder.AwaitOnCompleted(ref localTaskAwaiter, ref this);
                        executeFinallyBlock = false;
                        return;

                        RealAwaitContinuation:
                        localTaskAwaiter = TaskAwaiter;
                        TaskAwaiter = default;
                        State = -1;

                        AwaitCompletion:
                        Content = localTaskAwaiter.GetResult();
                        localTaskAwaiter = default;
                        TotalNumberOfWords += Content.CalculateNumberOfWords();
                        result = TotalNumberOfWords;

                        LoopCondition:
                        if (Enumerator.MoveNext())
                            goto LoopBody;
                    }
                    finally
                    {
                        if (executeFinallyBlock)
                            Enumerator?.Dispose();
                    }
                }
                catch (Exception exception)
                {
                    State = -2;
                    Builder.SetException(exception);
                    return;
                }
                State = -2;
                Builder.SetResult(result);
            }

            [DebuggerHidden]
            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine) // stateMachine is the boxed version of the DemoStateMachine instance
            {
                Builder.SetStateMachine(stateMachine);
            }
        }
    }
}