using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterParams
{
    public class FilterBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (IsFilter(context.Metadata.ModelType))
            {
                return new BinderTypeModelBinder(context.Metadata.ModelType);
            }

            return null;
        }
        private bool IsFilter(Type type)
        {
            if (!type.IsConstructedGenericType)
            {
                return false;
            } else
            {
                if (type.GetGenericTypeDefinition() == typeof(Filter<>))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
