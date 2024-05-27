using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceNeural.Utils
{
    //https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors
    //https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource-1
    //La classe TaskCompletionSourceManager serve a ospitare un campo di tipo TaskCompletionSource che va utilizzato in combinazione con i metodi:
    //ConfigureContinuousIntentPatternMatchingWithMicrophoneAsync 
    //ContinuousIntentPatternMatchingWithMicrophoneAsync
    //la classe TaskCompletionSourceManager implementa un Primary Constructor (C# ver. 12 e succ. .NET SDK 8.0 e succ.)
    public class TaskCompletionSourceManager<T>(TaskCompletionSource<T> taskCompletionSource)
    {
        TaskCompletionSource<T> _taskCompletionSource = taskCompletionSource;
        public TaskCompletionSourceManager() : this(new TaskCompletionSource<T>()) { }
        public TaskCompletionSource<T> TaskCompletionSource { get => _taskCompletionSource; set => _taskCompletionSource = value; }
    }
}
