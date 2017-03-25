using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    public class NetworkEnemy
        : NetworkBehaviour
    {
        public override void OnStartLocalPlayer()
        {
            GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        }

        public override void PreStartClient()
        {
            GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        }
    }
}
