using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Threading.Tasks;

namespace MjIot.EventsHandler.ValueModifiers
{
    class CustomCalculationExecutor
    {
        public async Task<string> CalculateAsync(ValueInfo inputValue, string calculation)
        {
            object input = null;
            if (inputValue.IsNumeric)
                input = new CustomCalculationInput<double>(inputValue.NumericValue.Value);
            else if (inputValue.IsBoolean)
                input = new CustomCalculationInput<bool>(inputValue.BooleanValue.Value);
            else
                input = new CustomCalculationInput<string>(inputValue.StringValue);

            return await GetResult(input, calculation);
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