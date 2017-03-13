using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Реализует основные методы юнита
    /// </summary>
    public interface UnitInterface
    {
        /// <summary>
        /// Начальный метод
        /// </summary>
        void startMethod();

        /// <summary>
        /// Стартовый метод
        /// </summary>
        void Start();

        /// <summary>
        /// Проверить, жив ли объект
        /// </summary>
        /// <returns></returns>
        bool getAliveCondition();

        /// <summary>
        /// Проверить готовность к битве
        /// </summary>
        /// <returns></returns>
        bool getReadyToFightCondition();

        /// <summary>
        /// Увеличить число атакующих
        /// </summary>
        /// <param name="_obj"></param>
        void increaseCountOfTurrelFighters(GameObject _obj);

        /// <summary>
        /// Уменьшить число атакующих
        /// </summary>
        /// <param name="_obj"></param>
        void decreaseCountOfTurrelFighters(GameObject _obj);

        /// <summary>
        /// Смерть
        /// </summary>
        void CmdDead();

        void RpcClientDeath();

        /// <summary>
        /// Атака
        /// </summary>
        void attack();

        /// <summary>
        /// Контроль за анимацией
        /// </summary>
        void mover();

        /// <summary>
        /// Менять атакующую позицию
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        void changeValues(bool a, bool b, bool c, bool d);

        /// <summary>
        /// Анимация атаки
        /// </summary>
        void attackAnim();

        /// <summary>
        /// Обнулить основные переменные объекта
        /// </summary>
        void nullAttackedObject();

        /// <summary>
        /// Очищает позицию атакующего и снизить количество атакующих
        /// </summary>
        void decreaser();

        /// <summary>
        /// Случайная скорость удара
        /// </summary>
        void randomHit();

        /// <summary>
        /// рассчитать атакующую позицию
        /// </summary>
        /// <param name="p"></param>
        void calculatePoint(byte p);

        /// <summary>
        /// Сменить атакующую позицию
        /// </summary>
        /// <returns></returns>
        byte switchPoint();

        /// <summary>
        /// Очистить атакующую позицию
        /// </summary>
        /// <param name="b"></param>
        void clearPoint(byte b);

        /// <summary>
        /// Получить объект-цель
        /// </summary>
        /// <returns></returns>
        GameObject getAttackedObject();
    }

    public interface GetDamageAnimation
    {
        /// <summary>
        /// Меняет цвет, при получении урона
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> damageAnimation();

        /// <summary>
        /// Сменить цвет, при ударе
        /// </summary>
        /// <param name="color"></param>
        void changeColor(Color color);
    }
}
