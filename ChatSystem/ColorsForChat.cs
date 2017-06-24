using UnityEngine;

namespace ChatSystem
{
    /// <summary>
    /// Инициализируем массив цветов
    /// </summary>
    public class ColorsForChat
        : MonoBehaviour
    {
        private static Color[] colorsArray
            = new Color[] { Color.red,Color.magenta
            ,Color.yellow,Color.green,Color.black
            ,Color.blue };

        /// <summary>
        /// Берем случайный цвет из массива
        /// </summary>
        /// <returns></returns>
        public static Color GetRandomColor()
        {
            return colorsArray[
                new System.Random()
                .Next(0, colorsArray.Length)];
        }
    }
}
