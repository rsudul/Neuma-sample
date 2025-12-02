using System;

namespace Neuma.Core.DataLoading
{
    public interface IDataLoader<T>
    {
        T Load(string resourceId);
    }
}

