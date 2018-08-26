using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Threading.Tasks;

namespace MjIot.EventsHandler.ValueModifiers
{
    class CustomCalculationExecutor
    {
        public async Task<string> CalculateAsync(ValueInfo inputValue, string calculation)
        {
            //object input = null;
            if (inputValue.IsNumeric)
                calculation = calculation.Replace("x", inputValue.NumericValue.Value.ToString());
            else if (inputValue.IsBoolean)
                calculation = calculation.Replace("x", inputValue.BooleanValue.Value.ToString().ToLower());
            else
                calculation = calculation.Replace("x", $@"""{inputValue.StringValue}""");

            return await GetResult2(calculation);
        }

        async Task<string> GetResult2(string calculation)
        {
            var task = CSharpScript.EvaluateAsync(
                calculation,
                options: ScriptOptions.Default.WithImports("System")
            );

            await task;

            var result = task.Result.ToString();
            if (result == "False")
                return "false";
            if (result == "True")
                return "true";

            return task.Result.ToString();
        }
        async Task<string> GetResult(object input, string calculation)
        {
            var task = CSharpScript.EvaluateAsync(
                calculation,
                globals: input,
                options: ScriptOptions.Default.WithImports("System")
            );

            await task;

            var result = task.Result.ToString();
            if (result == "False")
                return "false";
            if (result == "True")
                return "true";

            return task.Result.ToString();
        }
    }
}