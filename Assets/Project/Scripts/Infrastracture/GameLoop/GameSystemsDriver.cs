using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Infrastracture.GameLoop
{
    public class GameSystemsDriver : MonoBehaviour
    {
        [SerializeField] private List<MonoBehaviour> _systems = new List<MonoBehaviour>();

        private readonly List<IGameStartable> _startables = new List<IGameStartable>();
        private readonly List<IGameUpdatable> _updatables = new List<IGameUpdatable>();

        private void Awake()
        {
            _startables.Clear();
            _updatables.Clear();

            foreach (MonoBehaviour system in _systems)
            {
                if (system is IGameStartable startable)
                    _startables.Add(startable);
                if (system is IGameUpdatable updatable)
                    _updatables.Add(updatable);
            }
        }

        private void Start()
        {
            for (int i = 0; i < _startables.Count; i++)
                _startables[i].GameStart();
        }

        private void Update()
        {
            for (int i = 0; i < _updatables.Count; i++)
                _updatables[i].GameUpdate();
        }
    }
}
