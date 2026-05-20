using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Threading.Tasks;

namespace TableTennisHistoric
{
    public class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult != ValueProviderResult.None)
            {
                string? enteredValue = valueProviderResult.FirstValue;

                if (!string.IsNullOrEmpty(enteredValue))
                {
                    // Accepte point ou virgule
                    enteredValue = enteredValue.Replace(".", ",");

                    if (decimal.TryParse(enteredValue, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out decimal parsedValue))
                    {
                        bindingContext.Result = ModelBindingResult.Success(parsedValue);
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Valeur invalide.");
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
