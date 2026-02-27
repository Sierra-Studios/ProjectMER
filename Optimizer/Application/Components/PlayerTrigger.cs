using LabApi.Features.Wrappers;
using UnityEngine;

namespace MEROptimizer.Application.Components
{
    public class PlayerTrigger : MonoBehaviour
    {
        public Player player { get; set; }

        private Vector3 offset { get; set; }

        void Start()
        {
            offset = new Vector3(0, 2000, 0);
        }
        
        public void Update()
        {
            if (player == null || !player.ReferenceHub || !player.ReferenceHub.transform)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = player.ReferenceHub.transform.position + offset;
        }
    }
}