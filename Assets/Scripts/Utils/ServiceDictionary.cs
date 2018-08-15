using System;
using Services;

namespace Utils {
    [Serializable]
    public class ServiceDictionary: SerializableDictionary<Type, IService> {}
}