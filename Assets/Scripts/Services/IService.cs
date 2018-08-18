namespace Services {
    public interface IService {
        void Initialize();
        void PostInitialize();
        void Destroy();
    }
}