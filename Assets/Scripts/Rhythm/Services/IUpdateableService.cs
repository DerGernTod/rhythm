﻿namespace Rhythm.Services {
    public interface IUpdateableService : IService {
        void Update(float deltaTime);
        void FixedUpdate();
    }
}