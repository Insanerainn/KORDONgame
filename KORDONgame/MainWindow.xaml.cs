using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfGame
{
    public partial class MainWindow : Window
    {
        private void CheckVictory()
        {
            bool player1Wins = true;
            bool player2Wins = true;

            foreach (Region region in regions)
            {
                if (region.Color != Colors.Blue)
                {
                    player1Wins = false;
                }
                if (region.Color != Colors.Red)
                {
                    player2Wins = false;
                }
            }
            if (player1Wins || player2Wins)
            {
                string winner = player1Wins ? "Гравець 1" : "Гравець 2";
                MessageBox.Show($"{winner} переміг! Гра закінчена.", "Гра закінчена");
                Close();
            }
        }
        private Dictionary<Region, List<Army>> armiesInRegions = new Dictionary<Region, List<Army>>();
        private List<Army> armies = new List<Army>();
        private List<Region> regions = new List<Region>();
        private int currentPlayer = 0;
        private int currentArmyIndex = 0;
        public MainWindow()
        {
            InitializeComponent();

            int numberOfRegions = GetNumberOfRegions();
            int armiesInEachRegion = GetArmiesInEachRegion();

            InitializeGame(numberOfRegions, armiesInEachRegion);
        }
        private void InitializeGame(int numberOfRegions, int armiesInEachRegion)
        {
            for (int player = 0; player < 2; player++)
            {
                Color playerArmyColor = player == 0 ? Colors.Blue : Colors.Red;

                for (int i = 0; i < numberOfRegions; i++)
                {
                    Color regionColor = playerArmyColor;
                    Point regionPosition = GetRandomPositionForRegion(player);
                    Region region = new Region(gameCanvas, regionColor, regionPosition);
                    regions.Add(region);
                    armiesInRegions.Add(region, new List<Army>());

                    for (int j = 0; j < armiesInEachRegion; j++)
                    {
                        Color armyColor = playerArmyColor;
                        Point initialPosition = regionPosition;
                        Army army = new Army(gameCanvas, armyColor, initialPosition);

                        if (j == 0)
                        {
                            army.IsMovable = false;
                            army.IsSelectable = false;
                        }

                        armies.Add(army);
                    }
                }
            }
            KeyDown += MainWindow_KeyDown;
        }
        private int GetArmiesInEachRegion()
        {
            int maxArmiesInRegion = 5;
            int armiesInEachRegion;

            do
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox($"Введіть кількість армій в регіонах (Не більше {maxArmiesInRegion}):", "Armies in each region", "1");
                if (int.TryParse(input, out armiesInEachRegion) && armiesInEachRegion > 0 && armiesInEachRegion <= maxArmiesInRegion)
                {
                    return armiesInEachRegion;
                }
                else
                {
                    MessageBox.Show($"Помилка, некоректне число. Введіть число від 2 до {maxArmiesInRegion}.", "Error");
                }
            } while (true);
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R)
            {
                currentPlayer = (currentPlayer + 1) % 2;
                currentArmyIndex = 1;
                Console.WriteLine($"Switched to Player {currentPlayer + 1}");
            }
            if (e.Key == Key.Tab)
            {
                currentArmyIndex = (currentArmyIndex + 1) % (armies.Count / 2);
                while (!armies[currentPlayer * armies.Count / 2 + currentArmyIndex].IsSelectable)
                {
                    currentArmyIndex = (currentArmyIndex + 1) % (armies.Count / 2);
                }
                Console.WriteLine($"Switched to Army {currentArmyIndex + 1} of Player {currentPlayer + 1}");
            }

            if (e.Key == Key.W || e.Key == Key.S || e.Key == Key.A || e.Key == Key.D)
            {
                MoveArmy(armies[currentPlayer * armies.Count / 2 + currentArmyIndex], e);
            }
        }
        private void MoveArmy(Army army, KeyEventArgs e)
        {
            if (!army.IsMovable)
            {
                Console.WriteLine("This army is not movable.");
                return;
            }
            switch (e.Key)
            {
                case Key.W:
                    army.Move(0, -5);
                    break;
                case Key.S:
                    army.Move(0, 5);
                    break;
                case Key.A:
                    army.Move(-5, 0);
                    break;
                case Key.D:
                    army.Move(5, 0);
                    break;
            }
            foreach (Region region in regions)
            {
                if (region.Contains(army) && IsArmyAllowedInRegion(army, region))
                {
                    Console.WriteLine($"Армія потрапила в регіон кольору {region.Color}");
                    army.CurrentRegion = region;
                    region.ChangeColor(army.GetColor());
                    Console.WriteLine($"Player {currentPlayer + 1} remains the active player");
                    break;
                }
                else if (region.Contains(army) && !IsArmyAllowedInRegion(army, region))
                {
                    // If army is in another player's region, check if it's movable in that region
                    if (army.IsMovable && army.IsSelectable)
                    {
                        region.ChangeColor(army.GetColor());
                    }
                    else
                    {
                        // If not movable, revert the army's position
                        army.Move(-5, 0);
                    }
                }
            }
            CheckVictory();
        }
        private bool IsArmyAllowedInRegion(Army army, Region region)
        {
            return army.GetColor() == region.Color;
        }
        private Point GetRandomPositionForRegion(int player)
        {
            Random random = new Random();
            double x, y;
            double screenWidth = gameCanvas.ActualWidth + 1165;
            double halfScreenWidth = screenWidth / 2;
            if (player == 0)
            {
                x = random.Next((int)halfScreenWidth);
            }
            else
            {
                x = random.Next((int)halfScreenWidth, (int)screenWidth);
            }
            y = random.Next((int)gameCanvas.ActualHeight + 650);

            while (IsOverlapWithExistingRegions(x, y))
            {
                // If overlap occurs, generate a new position
                x = player == 0 ? random.Next((int)halfScreenWidth) : random.Next((int)halfScreenWidth, (int)screenWidth);
                y = random.Next((int)gameCanvas.ActualHeight + 650);
            }
            return new Point(x, y);
        }
        private bool IsOverlapWithExistingRegions(double x, double y)
        {
            foreach (Region existingRegion in regions)
            {
                Rect existingRegionBounds = new Rect(Canvas.GetLeft(existingRegion.GetRegionRectangle()), Canvas.GetTop(existingRegion.GetRegionRectangle()), existingRegion.GetRegionRectangle().Width, existingRegion.GetRegionRectangle().Height);
                Rect newRegionBounds = new Rect(x, y, 70, 70);

                if (existingRegionBounds.IntersectsWith(newRegionBounds))
                {
                    return true;
                }
            }
            return false;
        }
        private int GetNumberOfRegions()
        {
            int maxRegions = 10;
            int numberOfRegions;
            do
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox($"Введіть кількість регіонів (Не більше {maxRegions}):", "Кількість регіонів", "2");
                if (int.TryParse(input, out numberOfRegions) && numberOfRegions > 0 && numberOfRegions <= maxRegions)
                {
                    return numberOfRegions;
                }
                else
                {
                    MessageBox.Show($"Введено некоректне значення. Введіть ціле число від 1 до {maxRegions}.", "Помилка");
                }
            } while (true);
        }
    }
    public class Army
    {
        private Ellipse armyEllipse;
        private Canvas canvas;
        public bool IsMovable { get; set; } = true;
        public bool IsSelectable { get; set; } = true;
        public Color GetColor()
        {
            return ((SolidColorBrush)armyEllipse.Fill).Color;
        }
        public Army(Canvas gameCanvas, Color color, Point initialPosition)
        {
            armyEllipse = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = new SolidColorBrush(color)
            };
            canvas = gameCanvas;
            canvas.Children.Add(armyEllipse);
            SetPosition(initialPosition);
        }
        public Region CurrentRegion { get; set; }

        public void Move(double deltaX, double deltaY)
        {
            double newLeft = Canvas.GetLeft(armyEllipse) + deltaX;
            double newTop = Canvas.GetTop(armyEllipse) + deltaY;
            if (newLeft >= 0 && newLeft + armyEllipse.Width <= canvas.ActualWidth &&
                newTop >= 0 && newTop + armyEllipse.Height <= canvas.ActualHeight)
            {
                Canvas.SetLeft(armyEllipse, newLeft);
                Canvas.SetTop(armyEllipse, newTop);
            }
        }
        public Point GetPosition()
        {
            return new Point(Canvas.GetLeft(armyEllipse), Canvas.GetTop(armyEllipse));
        }
        private void SetPosition(Point position)
        {
            Canvas.SetLeft(armyEllipse, position.X);
            Canvas.SetTop(armyEllipse, position.Y);
        }
    }
    public class Region
    {
        public int OwnerPlayer { get; set; } = -1;
        private int armyCount;
        private Rectangle regionRectangle;
        private Canvas canvas;
        public Region(Canvas gameCanvas, Color color, Point initialPosition)
        {
            armyCount = 0;
            regionRectangle = new Rectangle
            {
                Width = 70,
                Height = 70,
                Fill = new SolidColorBrush(color),
                Opacity = 0.5
            };
            canvas = gameCanvas;
            canvas.Children.Add(regionRectangle);
            SetPosition(initialPosition);
        }
        public void ChangeColor(Color newColor)
        {
            Color = newColor;
            regionRectangle.Fill = new SolidColorBrush(Color);
        }
        public Color Color { get; set; }

        public Rectangle GetRegionRectangle()
        {
            return regionRectangle;
        }
        public bool Contains(Army army)
        {
            Rect regionBounds = new Rect(Canvas.GetLeft(regionRectangle), Canvas.GetTop(regionRectangle), regionRectangle.Width, regionRectangle.Height);
            Rect armyBounds = new Rect(army.GetPosition().X, army.GetPosition().Y, 30, 30);
            return regionBounds.IntersectsWith(armyBounds);
        }
        private void SetPosition(Point position)
        {
            Canvas.SetLeft(regionRectangle, position.X);
            Canvas.SetTop(regionRectangle, position.Y);
        }
    }
}