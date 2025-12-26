using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class BootEntry : MonoBehaviour
    {
        private void Awake()
        {
            
        }
    }

    public class SceneLoader
    {
        public async UniTask LoadSceneAsync(string sceneName)
        {
        }

        public async UniTask LoadSceneAsync(int sceneId)
        {
            
        }
    }

    public interface IService
    { }
    
    public interface ITickable
    {
        public void Tick();
    }

    public interface IFixedTickable
    {
        public void FixedTick();
    }
    
    public class Services
    {
        private Dictionary<Type, IService> _services;

        public struct BindArgs
        {
        }
        
        public void Bind<T>(T target) where T : IService
        {
            _services[typeof(T)] = target;
        }

        public IService Resolve<T>() where T : IService
        {
            if (_services.TryGetValue(typeof(T), out IService service))
                return service;
            
            throw new ArgumentException($"Service of type {typeof(T)} not found");
        }

        public IService Resolve(Type type)
        {
            if (_services.TryGetValue(type, out IService service))
                return service;
            
            throw new ArgumentException($"Service of type {type} not found");
        }
    }

    public class TickableController
    {
        private (LinkedList<ITickable> collection, int count) _tickables = new();
        private (LinkedList<IFixedTickable> collection, int count) _fixedTickables = new();

        public void AddTickable(ITickable tickable)
        {
            _tickables.collection.AddLast(tickable);
            _tickables.count++;
        }

        public void AddFixedTickable(IFixedTickable fixedTickable)
        {
            _fixedTickables.collection.AddLast(fixedTickable);
            _fixedTickables.count++;
        }

        public void RemoveTickable(ITickable tickable)
        {
            _tickables.collection.Remove(tickable);
            _tickables.count--;
        }

        public void RemoveFixedTickable(IFixedTickable fixedTickable)
        {
            _fixedTickables.collection.Remove(fixedTickable);
            _fixedTickables.count--;
        }
        
        private void OnUpdate()
        {
            LinkedListNode<ITickable> target =  _tickables.collection.First;
            
            for (int i = 0; i < _tickables.count; i++)
            {
                target.Value.Tick();
                target = target.Next;
            }
        }

        private void OnFixedUpdate()
        {
            LinkedListNode<IFixedTickable> target = _fixedTickables.collection.First;

            for (int i = 0; i < _tickables.count; i++)
            {
                target.Value.FixedTick();
                target = target.Next;
            }
        }
    }
}
