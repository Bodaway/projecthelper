using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projecthelper.Result;

public static class TaskResultExtensions
{
    /// <summary>
    /// force extraction of sucess data, throw an exception if not
    /// </summary>
    /// <param name="result">the result</param>
    /// <returns>the data inside Ok result</returns>
    public static async Task<T> ExtractOkData<T>(this Task<Result<T>> result)
    {
        var awaited = await result;
        return awaited.ExtractOkData(); 
    }
}
