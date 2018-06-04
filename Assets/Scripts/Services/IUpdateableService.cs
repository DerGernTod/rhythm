namespace Services {
    public interface IUpdateableService : IService {
        void Update(float deltaTime);
    }
}