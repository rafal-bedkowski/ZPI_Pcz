using Microsoft.AspNetCore.Mvc.Formatters;
using WebStore.ViewModels.VM;

namespace WebStore.Web.Infrastructure;

// Pozwala przyjmować dane logowania także jako formularz (urlencoded i multipart), obok JSON obsługiwanego domyślnie.
public class FormLoginInputFormatter : InputFormatter
{
    public FormLoginInputFormatter()
    {
        SupportedMediaTypes.Add("application/x-www-form-urlencoded");
        SupportedMediaTypes.Add("multipart/form-data");
    }

    protected override bool CanReadType(Type type) => type == typeof(LoginUserVm);

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        var form = await context.HttpContext.Request.ReadFormAsync(context.HttpContext.RequestAborted);
        var model = new LoginUserVm();
        (model.Login, model.Password) = (
            form[nameof(LoginUserVm.Login)].ToString(),
            form[nameof(LoginUserVm.Password)].ToString());
        return await InputFormatterResult.SuccessAsync(model);
    }
}
