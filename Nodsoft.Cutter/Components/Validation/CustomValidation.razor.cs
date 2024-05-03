using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Nodsoft.Cutter.Components.Validation;

/// <summary>
/// Provides a way to display custom validation messages in a Blazor form.
/// </summary>
public class CustomValidation : ComponentBase
{
    private ValidationMessageStore? messageStore;

    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    protected override void OnInitialized()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CustomValidation)} requires a cascading parameter of type {nameof(EditContext)}.");
        }

        messageStore = new(CurrentEditContext);

        CurrentEditContext.OnValidationRequested += (s, e) => messageStore?.Clear();
        CurrentEditContext.OnFieldChanged += (s, e) => messageStore?.Clear(e.FieldIdentifier);
    }

    public void DisplayErrors(Dictionary<string, List<string>> errors)
    {
        if (CurrentEditContext is null)
        {
            return;
        }

        foreach ((string? key, var value) in errors)
        {
            messageStore?.Add(CurrentEditContext.Field(key), value);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    public void ClearErrors()
    {
        messageStore?.Clear();
        CurrentEditContext?.NotifyValidationStateChanged();
    }
}