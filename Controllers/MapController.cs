using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sapper.Controllers
{
    public static class MapController
    {
        public static int mapSize = GetMapSize();   // Размер карты клетки х клетки
        public const int cellSize = 50; // Размер клетки
        
        private static Random r = new Random();
        private static int bombCount = Convert.ToInt32(mapSize);   // Количество бомб

        private static int currentPictureToSet = 0;     // Для перебора спрайтов
        private static Point previousPressedButtonLocation;

        public static int[,] map;  // Массив идентификаторов для кнопок (-1 - бомба, 0 - пустая клетка, 1- одна бомба рядом, 2 - две бомбы и т.д)
        public static Button[,] buttons;   // Массив кнопок на поле

        private static Image spriteSet;     // Спрпайт сет из которого берем спрайты для кнопок (лежит в /Sprites/tiles.png)

        private static bool isFirstStep;     // Определяет первый ли это клик пользователя
        private static Point firstCoord;     // Определяет координаты первой нажатой кнопки

        public static Form form;    // Для работы с основной формой

        public static MenuStrip menu = new MenuStrip(); // Объявление нового объекта "меню" на котором будет находится кнопка настроек
        public static ToolStripMenuItem buttonSettings = new ToolStripMenuItem("Настройки"); // Кнопка настроек

        // Инициализация игры
        public static void Init(Form current)     
        {
            form = current;
            currentPictureToSet = 0;
            previousPressedButtonLocation = new Point(0, 0);
            isFirstStep = true;
            spriteSet = new Bitmap(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName.ToString(), "Sprites\\tiles.png"));
                                                                                                                                           
            InitMap();
            InitButtons(current);   
            ConfigureMapSize(current);

            current.Controls.Add(menu);
            menu.Items.Add(buttonSettings);
        }

        // Берем размер карты из настроек
        public static int GetMapSize()      
        {
            mapSize = FormSettings.newMapSize;

            map = new int[mapSize, mapSize];
            buttons = new Button[mapSize, mapSize];

            return mapSize;
        }

        // Изменение размера формы от размера клеток и размера карты
        private static void ConfigureMapSize(Form current)     
        {
            current.Width = mapSize * cellSize + 20;
            current.Height = mapSize * cellSize + 65;
        }

        // Инициализация карты
        private static void InitMap()       
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    map[i, j] = 0;
                }
            }
        }

        // Добавление на форму кнопок
        private static void InitButtons(Form current)      
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    Button button = new Button();
                    button.Location = new Point(j * cellSize, i * cellSize + 25);
                    button.Size = new Size(cellSize, cellSize);
                    button.Image = FindNeededImage(0, 0);   // Определяет какой спрайт будет у кнопки
                    button.MouseUp += new MouseEventHandler(OnButtonPressedMouse);
                    current.Controls.Add(button);   // Добавляем кнопку на форму
                    buttons[i, j] = button;     // Помещаем кнопку в массив кнопок
                }
            }
        }

        // При нажатии какой либо кнопки мыши
        private static void OnButtonPressedMouse(object sender, MouseEventArgs e)   
        {
            Button pressedButton = sender as Button;        // Помещаем в переменную объект кнопку
            switch (e.Button.ToString())
            {
                case "Right":
                    OnRightButtonPressed(pressedButton);
                    break;
                case "Left":
                    OnLeftButtonPressed(pressedButton);
                    break;
            }
            previousPressedButtonLocation = pressedButton.Location;     // Запоминаем позицию предыдущей нажатой кнопки для корректного
                                                                        // отображения флажков
        }

        // При левом клике
        private static void OnLeftButtonPressed(Button pressedButton)     
        {
            pressedButton.Text = " ";       // Чтобы нажатая кнопка не теряла свой цвет, вместо деактивации кнопки,
                                            // я добавляю в ее текст пробел и по нему задаю условия
            int iButton = pressedButton.Location.Y / cellSize;      // Переводим координаты в пикселях в более удобную форму
            int jButton = pressedButton.Location.X / cellSize;      // также как спрайты

            // По первому клику генерирует карту
            if (isFirstStep)        
            {
                firstCoord = new Point(jButton, iButton);
                SeedMap();
                CountCellBomb();
                isFirstStep = false;
            }
            OpenCells(iButton, jButton);    // Открываем клетки

            // Если пользователь нажал на бомбу - поражение
            if (map[iButton,jButton] == -1) 
            {
                ShowAllBombs(iButton, jButton);
                MessageBox.Show("Ты проиграл ¯\\_(ツ)_/¯");
                form.Controls.Clear();
                Init(form);
            }

            // Считаем деактивированные кнопки
            int disabledButtons = 0;
            for (int k = 0; k < mapSize; k++)
            {
                for (int l = 0; l < mapSize; l++)
                {
                    if (buttons[k, l].Text == " ")
                    {
                        disabledButtons++;
                    }
                }
            }

            // Если все кнопки кроме бомб деактивированы - победа
            if (disabledButtons == mapSize * mapSize - bombCount)   
            {
                ShowAllBombs(iButton, jButton);
                MessageBox.Show("Ты выиграл! (◕‿◕)");
                form.Controls.Clear();
                Init(form);
            }
        }

        // При правом клике
        private static void OnRightButtonPressed(Button pressedButton)      
        {
            if (pressedButton.Text != " ")
            {
                currentPictureToSet++;      // При каждом нажатии меняем спрайт и не позволяем ему уйти за рамки двух положений
                int posX = 0;
                int posY = 0;

                if (previousPressedButtonLocation != pressedButton.Location)    // Если нажата не та же самая кнопка ставим флажок
                {
                    currentPictureToSet = 1;
                }
                currentPictureToSet %= 2;

                switch (currentPictureToSet)        // Определяет какой спрайт будет при нажатии правой кнопки (2 положения)
                {
                    case 0: // Пустая клетка
                        posX = 0;
                        posY = 0;
                        break;
                    case 1: // Флажок
                        posX = 0;
                        posY = 2;
                        break;
                }
                pressedButton.Image = FindNeededImage(posX, posY);
            }
        }
        private static void ShowAllBombs(int iBomb, int jBomb)
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (i == iBomb || j == jBomb)
                        continue;
                    if (map[i,j] == -1)
                    {
                        buttons[i, j].Image = FindNeededImage(3, 2);
                    }
                }
            }
        }

        // Вырезает из спрайтсета нужный спрайт
        public static Image FindNeededImage(int xPos, int yPos)     
        {
            Image image = new Bitmap(cellSize,cellSize);
            Graphics g = Graphics.FromImage(image);
            g.DrawImage(spriteSet, new Rectangle(new Point(0, 0), new Size(cellSize, cellSize)), 0 + 32 * xPos, 0 + 32 * yPos, 33, 33, GraphicsUnit.Pixel);     // Разделяет спрайт сет на отдельные спрайты, доступные по координатам по типу (0,0) (0,1)
            
            return image;
        }

        // Генерация карты (расстановка бомб)
        private static void SeedMap()   
        {
            for (int i = 0; i < bombCount; i++)
            {
                int posI = r.Next(0, mapSize - 1);
                int posJ = r.Next(0, mapSize - 1);

                while (map[posI,posJ] == -1 || (Math.Abs(posI-firstCoord.Y)<=1 && Math.Abs(posJ - firstCoord.X) <= 1))
                {
                    posI = r.Next(0, mapSize - 1);
                    posJ = r.Next(0, mapSize - 1);
                }

                map[posI, posJ] = -1;   // -1 - значит бомба
            }
        }

        // Считаем бомбы вокруг клеток
        private static void CountCellBomb()     
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (map[i,j] == -1)
                    {
                        for (int k = i - 1; k < i + 2; k++)
                        {
                            for (int l = j - 1; l < j + 2; l++)
                            {
                                if (!IsInBorder(k,l) || map[k,l] == -1)
                                    continue;
                                map[k, l] = map[k, l] + 1;
                            }
                        }
                    }
                }
            }
        }

        // Проверка границ поля, чтобы не уйти за пределы
        private static bool IsInBorder(int i, int j)   
        {
            if (i < 0 || j < 0 || i > mapSize - 1 || j > mapSize - 1) 
            {
                return false;
            }
            return true;
        }

        // Назначаем каждой неактивной кнопке свой спрайт
        private static void OpenCell(int i, int j)   
        {
            buttons[i, j].Text = " ";

            switch (map[i, j])
            {
                case 1:     // Спрайт: одна бомба рядом
                    buttons[i, j].Image = FindNeededImage(1, 0);
                    break;
                case 2:     // Спрайт: две бомбы рядом
                    buttons[i, j].Image = FindNeededImage(2, 0);
                    break;
                case 3:     // Спрайт: три бомбы рядом
                    buttons[i, j].Image = FindNeededImage(3, 0);
                    break;
                case 4:     // Спрайт: четыре бомбы рядом
                    buttons[i, j].Image = FindNeededImage(4, 0);
                    break;
                case 5:     // Спрайт: пять бомб рядом
                    buttons[i, j].Image = FindNeededImage(0, 1);
                    break;
                case 6:     // Спрайт: шесть бомб рядом
                    buttons[i, j].Image = FindNeededImage(1, 1);
                    break;
                case 7:     // Спрайт: семь бомб рядом
                    buttons[i, j].Image = FindNeededImage(2, 1);
                    break;
                case 8:     // Спрайт: восемь бомб рядом
                    buttons[i, j].Image = FindNeededImage(3, 1);
                    break;
                case -1:    // Спрайт: бомба
                    buttons[i, j].Image = FindNeededImage(1, 2);
                    break;
                case 0:     // Спрайт: неактивная клетка
                    buttons[i, j].Image = FindNeededImage(4, 2);
                    break;
            }
        }

        // При нажатии на ячейку открываем соседние ячейки не-бомбы
        private static void OpenCells(int i, int j)     
        {
            OpenCell(i, j);

            if (map[i, j] > 0)
                return;

            for (int k = i - 1; k < i + 2; k++)
            {
                for (int l = j - 1; l < j + 2; l++)
                {
                    if (!IsInBorder(k, l) || map[k, l] == -1)
                        continue;
                    if (buttons[k, l].Text == " ")
                        continue;
                    if (map[k, l] == 0)
                        OpenCells(k, l);
                    else if (map[k, l] > 0)
                        OpenCell(k, l);
                }
            }
        }
        
    }
}

