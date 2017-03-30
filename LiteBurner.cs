using MovementEffects;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Описывает поведение поджигателя
    /// Наследует PlayerAbstract
    /// </summary>
    public class LiteBurner 
        : LiteArcher
    {
        protected Vector3 _burningPosition;

        /// <summary>
        /// Выстрел
        /// </summary>
        public override void Bursting()
        {
            if (_cleverShooting) CleverShoot();

            _instantier.transform.LookAt(_attackedObject.transform.position
                + _plusPos);

            Debug.DrawLine(_instantier.transform.position, _attackedObject.transform.position + _plusPos, Color.red,1);
            CmdInstantiate(_bullet);
            _countOfAmmo--;
        }

        [Command]
        protected override void CmdInstantiate(GameObject clone)
        {
            RpcInstantiate(clone);
        }

        [Client]
        protected override void RpcInstantiate(GameObject _bullet)
        {
            GameObject clone = GameObject.Instantiate(_bullet);
            clone.transform.position = _instantier.transform.position;
            clone.transform.localEulerAngles = _instantier.transform.localEulerAngles;
            clone.GetComponent<Molotov>().setPosition(_attackedObject.transform.position + _plusPos);
            clone.GetComponent<Molotov>().setInstantedPlayer(gameObject.GetComponent<PlayerAbstract>());
            NetworkServer.Spawn(clone);
        }

        [ClientRpc]
        protected override void RpcPlayAudio(byte condition)
        {
            switch (condition)
            {
                case 0:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsCloseUnit((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsCloseUnit()));
                    _audioSource.Play();
                    break;
                case 1:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFarUnit((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsFarUnit()));
                    _audioSource.Play();
                    break;
                case 2:
                    _audioSource.pitch = (float)randomer.NextDouble() + 1f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFire((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsFire()));
                    _audioSource.Play();
                    break;
                case 3:
                    _audioSource.pitch = (float)randomer.NextDouble()  + 1f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioThrowes((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioAudioThrowes()));
                    _audioSource.Play();
                    break;
                case 4:
                    _audioSource.pitch = (float)randomer.NextDouble() + 2f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioDeathsUnit((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioDeathsUnit()));
                    _audioSource.Play();
                    break;
            }
        }
    }
}
