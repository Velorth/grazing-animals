using System;
using UnityEngine;

namespace GrazingAnimals
{
    public class Food : MonoBehaviour
    {
        [SerializeField] private GameObject _fxPrefab;
        
        public event Action Collected;
        
        public void Collect()
        {
            Instantiate(_fxPrefab, transform.localPosition, transform.localRotation);
            
            Collected?.Invoke();
        }
    }
}