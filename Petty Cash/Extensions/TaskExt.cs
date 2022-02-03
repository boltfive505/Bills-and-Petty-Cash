using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash
{
    public static class TaskExt
    {
        public static T GetResult<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
