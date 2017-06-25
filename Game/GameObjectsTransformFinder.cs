using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Класс, реализующий поиск противника, вместо использования физики
    /// </summary>
    public class GameObjectsTransformFinder
        : NetworkBehaviour
    {
        [SerializeField,Tooltip("Позиции врагов")]
        private static List<Transform> enemyTransformList;
        [SerializeField, Tooltip("Позиции персов игрока")]
        private static List<Transform> playerTransformList;
        private static Vector3 positionOfTower;
        private static System.Random rnd = new System.Random();

        /// <summary>
        /// Инициализация
        /// </summary>
        public override void OnStartServer()
        {
            enemyTransformList = new List<Transform>();
            playerTransformList = new List<Transform>();
            positionOfTower = 
                GameObject.Find("Tower").gameObject.transform.position;
        }

        /// <summary>
        /// Удалить из листа врагов
        /// </summary>
        /// <param name="transEnemy"></param>
        public static void RemoveFromEnemyTransformList(Transform transEnemy)
        {
            enemyTransformList.Remove(transEnemy);
        }

        /// <summary>
        /// Добавить в лист врагов
        /// </summary>
        /// <param name="transEnemy"></param>
        public static void AddToEnemyTransformList(Transform transEnemy)
        {
            enemyTransformList.Add(transEnemy);
        }

        /// <summary>
        /// Удалить из листа врагов
        /// </summary>
        /// <param name="transEnemy"></param>
        public static void RemoveFromPlayerTransformList(Transform transPlayer)
        {
            playerTransformList.Remove(transPlayer);
            //Debug.Log(enemyTransformList.Count);
        }

        /// <summary>
        /// Добавить в лист врагов
        /// </summary>
        /// <param name="transEnemy"></param>
        public static void AddToPlayerTransformList(Transform transPlayer)
        {
            playerTransformList.Add(transPlayer);
        }

        /// <summary>
        /// Получаем врага со сцены для атаки по нему
        /// , удовлетворяющего критерию поиска, 
        /// на основе вычисления дистанции, между векторами.
        /// 
        /// Является оптимизированной заменой физики.
        /// </summary>
        /// <param name="playerTransform"></param>
        /// <param name="attackRadius"></param>
        /// <param name="typeEnemyChoice"></param>
        /// <returns></returns>
        public static GameObject GetEnemyUnit(Transform playerTransform, 
            float radius,TypeEnemyChoice typeEnemyChoice,bool isTurrelTarget=false)
        {
            GameObject enemy = null;
            switch (typeEnemyChoice)
            {
                case TypeEnemyChoice.First:
                    float distance = 1000000;
                    float tempDistance = 1000000;
                    foreach (Transform tEnemy in enemyTransformList)
                    {
                        tempDistance = Vector3.Distance(tEnemy.position, playerTransform.position);
                        if (tempDistance <= radius && tempDistance < distance)
                        {
                            if (tEnemy.GetComponent<EnemyAbstract>().IsAlive)
                            {
                                if (isTurrelTarget)
                                {
                                    distance = tempDistance;
                                    enemy = tEnemy.gameObject;
                                }
                                else
                                {
                                    if (tEnemy.GetComponent<EnemyAbstract>().GetReadyToFightCondition())
                                    {
                                        distance = tempDistance;
                                        enemy = tEnemy.gameObject;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case TypeEnemyChoice.Standart:
                    foreach (Transform tEnemy in enemyTransformList)
                    {
                        if (Vector3.Distance(tEnemy.position, playerTransform.position) <= radius)
                        {
                            if (tEnemy.GetComponent<EnemyAbstract>().IsAlive)
                            {
                                if (isTurrelTarget)
                                {
                                    enemy = tEnemy.gameObject;
                                    return enemy;
                                }
                                else
                                {
                                    if (tEnemy.GetComponent<EnemyAbstract>().GetReadyToFightCondition())
                                    {
                                        enemy = tEnemy.gameObject;
                                        return enemy;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case TypeEnemyChoice.Fast:
                    float speed = -1;
                    float tempSpeed = -1;
                    foreach (Transform tEnemy in enemyTransformList)
                    {
                        if (Vector3.Distance(tEnemy.position, playerTransform.position) <= radius)
                        {
                            tempSpeed = tEnemy.GetComponent<EnemyAbstract>().WalkSpeed;
                            if (tempSpeed > speed)
                            {
                                if (isTurrelTarget)
                                {
                                    enemy = tEnemy.gameObject;
                                    speed = tempSpeed;
                                }
                                else
                                {
                                    if (tEnemy.GetComponent<EnemyAbstract>().GetReadyToFightCondition())
                                    {
                                        enemy = tEnemy.gameObject;
                                        speed = tempSpeed;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case TypeEnemyChoice.Power:
                    float power = -1;
                    float tempPower = -1;
                    foreach (Transform tEnemy in enemyTransformList)
                    {
                        if (Vector3.Distance(tEnemy.position, playerTransform.position) <= radius)
                        {
                            tempPower = tEnemy.GetComponent<EnemyAbstract>().GetPower();
                            if (tempPower > power)
                            {
                                if (isTurrelTarget)
                                {
                                    enemy = tEnemy.gameObject;
                                    power = tempPower;
                                }
                                else
                                {
                                    if (tEnemy.GetComponent<EnemyAbstract>().GetReadyToFightCondition())
                                    {
                                        enemy = tEnemy.gameObject;
                                        power = tempPower;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            return enemy;
        }

        /// <summary>
        /// Получаем игрока со сцены для атаки по нему
        /// </summary>
        /// <param name="enemyTransform"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static GameObject GetPlayerUnit(Transform enemyTransform,float radius) 
        {
            GameObject playerUnit = null;
            foreach (Transform playerT in playerTransformList)
            {
                if (Vector3.Distance(playerT.position, enemyTransform.position) <= radius)
                {
                    if (playerT.GetComponent<PlayerAbstract>().IsAlive
                        && playerT.GetComponent<PlayerAbstract>().GetReadyToFightCondition())
                    {
                        playerUnit = playerT.gameObject;
                        return playerUnit;
                    }
                }
            }
            return playerUnit;
        }

        public static GameObject GetPlayerUnit(Transform enemyTransform
            , float radius, TypeEnemyChoice type)
        {
            GameObject playerUnit = null;
            switch (type)
            {
                case TypeEnemyChoice.First:
                    float distance = 1000000;
                    float tempDistance = 1000000;
                    foreach (Transform playerT in playerTransformList)
                    {
                        tempDistance = Vector3.Distance(playerT.position, enemyTransform.position);
                        if (tempDistance <= radius && tempDistance < distance)
                        {
                            if (playerT.GetComponent<PlayerAbstract>().IsAlive)
                            {
                                if (playerT.GetComponent<PlayerAbstract>().GetReadyToFightCondition())
                                {
                                    distance = tempDistance;
                                    playerUnit = playerT.gameObject;
                                }
                            }
                        }
                    }
                    break;
                case TypeEnemyChoice.Standart:
                    foreach (Transform playerT in playerTransformList)
                    {
                        if (Vector3.Distance(playerT.position, enemyTransform.position) <= radius)
                        {
                            if (playerT.GetComponent<PlayerAbstract>().IsAlive
                                && playerT.GetComponent<PlayerAbstract>().GetReadyToFightCondition())
                            {
                                playerUnit = playerT.gameObject;
                                return playerUnit;
                            }
                        }
                    }
                    break;
                case TypeEnemyChoice.Fast:
                    float speed = -1;
                    float tempSpeed = -1;
                    foreach (Transform playerT in playerTransformList)
                    {
                        if (Vector3.Distance(playerT.position, enemyTransform.position) <= radius)
                        {
                            tempSpeed = playerT.GetComponent<PlayerAbstract>().MoveSpeed;
                            if (tempSpeed > speed)
                            {
                                if (playerT.GetComponent<PlayerAbstract>().GetReadyToFightCondition())
                                {
                                    playerUnit = playerT.gameObject;
                                    speed = tempSpeed;
                                }
                            }
                        }
                    }
                    break;
                case TypeEnemyChoice.Power:
                    float power = -1;
                    float tempPower = -1;
                    foreach (Transform playerT in playerTransformList)
                    {
                        if (Vector3.Distance(playerT.position, enemyTransform.position) <= radius)
                        {
                            tempPower = playerT.GetComponent<PlayerAbstract>().TotalPlayerUnitPower;
                            if (tempPower > power)
                            {
                                if (playerT.GetComponent<PlayerAbstract>().GetReadyToFightCondition())
                                {
                                    playerUnit = playerT.gameObject;
                                    power = tempPower;
                                }
                            }
                        }
                    }
                    break;
            }
            return playerUnit;
        }

        /// <summary>
        /// Какой враг добрался до башни?
        /// </summary>
        /// <returns></returns>
        public static GameObject IsEnemyIntoTarget()
        {
            foreach (Transform tEnemy in enemyTransformList)
                if (Vector3.Distance(positionOfTower, tEnemy.position) <= 1f)
                    return tEnemy.gameObject;
            return null;
        }

        /// <summary>
        /// Какой враг добрался до цели?
        /// </summary>
        /// <returns></returns>
        public static GameObject IsEnemyIntoTarget(Transform bulletTransform)
        {
            foreach (Transform tEnemy in enemyTransformList)
                if (Vector3.Distance(bulletTransform.position, tEnemy.position) <= 0.5f)
                    return tEnemy.gameObject;
            return null;
        }

        public static TypeEnemyChoice SetRandomTypeOfEnemyChoiceForPlayerUnit()
        {
            int rndNumber = rnd.Next(0,4);
            switch (rndNumber)
            {
                case 0:
                    return TypeEnemyChoice.First;
                case 1:
                    return TypeEnemyChoice.Standart;
                case 2:
                    return TypeEnemyChoice.Fast;
                case 3:
                    return TypeEnemyChoice.Power;
            }
            return TypeEnemyChoice.Standart;
        }
    }

    /// <summary>
    /// Перечисление. Выбор противника.
    /// Самый ближний;
    /// самый первый, что найден;
    /// самый быстрый;
    /// самый сильный
    /// </summary>
    public enum TypeEnemyChoice
    {
        First, Standart, Fast, Power
    }
}
