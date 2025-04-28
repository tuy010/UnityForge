using UnityEngine;
using UnityEngine.Events;

namespace Tuy.UnityForge.Base
{
    public class EasyTrigger : MonoBehaviour
    {
        [SerializeField] private string tagName;
        [Space(10f)]
        [SerializeField] private UnityEvent onTriggerEnter;
        [SerializeField] private UnityEvent onTriggerExit;
        [SerializeField] private UnityEvent onTriggerStay;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == tagName)
            {
                onTriggerEnter.Invoke();
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if(other.tag == tagName)
            { 
                onTriggerExit.Invoke();
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if(other.tag == tagName)
            {
                onTriggerStay.Invoke();
            }
        }
    }
}

