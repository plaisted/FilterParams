using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FilterParams
{
    public class FilterBinder<T> : FilterBinder<T, T> { }
    public class FilterBinder<IT, T> : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            var query = bindingContext.HttpContext.Request.QueryString.Value;

            return Task.CompletedTask;

            // ?(property=eq:test+OR+property=eq:one)&propert2=gt:3&_order=+property


        }
    }
}
