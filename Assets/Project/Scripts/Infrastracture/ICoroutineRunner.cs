using System.Collections;
using UnityEngine;

namespace Project.Scripts.Infrastracture
{
    public interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
    }
}