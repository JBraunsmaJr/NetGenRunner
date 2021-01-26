using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace NetGenRunner
{
    public class Floor
    {
        public string Programs { get; set; }
        public Floor Parent { get; set; }
        public List<Floor> Children { get; set; } = new List<Floor>();
        public int LeftStart { get; set; }
    }

    public class NetGenRunner
    {
        #region RUNNER CONFIG
        private const int BOX_WIDTH = 15;
        private const int BOX_HEIGHT = 6;
        private const int SPACE_BETWEEN_LEVELS = 3;
        private const int SPACE_BETWEEN_BOXES = 2;
        #endregion

        #region PARTS
        private const string ROOT = " *Root*";
        private const string PIPE = "|";
        private const string LEFT_WALL = PIPE;
        private const string RIGHT_WALL = PIPE;
        private const char LEFT_LINK = '<';
        private const char RIGHT_LINK = '>';
        private const char TOP_BORDER = '-';
        private const string TOP_LEFT = ".";
        private const string TOP_RIGHT = ".";
        private const char BOTTOM_BORDER = '-';
        private const string BOTTOM_LEFT = "'";
        private const string BOTTOM_RIGHT = "'";
        #endregion

        private const string FontFamilyName = "Courier New";

        /// <summary>
        /// Name in which floor configs shall be saved to
        /// </summary>
        private const string FloorConfigFileName = "FloorConfig.json";

        /// <summary>
        /// Name in which lobby floor configs shall be saved to
        /// </summary>
        private const string LobbyFloorConfigFileName = "LobbyFloor.json";

        private string BaseDirectory => Directory.GetCurrentDirectory();

        /// <summary>
        /// Default lobby floors
        /// </summary>
        public string[] DefaultLobbyFloor => new[]
        {
                "File DV6",
                "Password DV6",
                "Password DV8",
                "Skunk",
                "Wisp",
                "Killer"
        };

        /// <summary>
        /// Default floor configs
        /// </summary>
        public List<FloorConfig> DefaultFloorConfigs => new List<FloorConfig>()
        {
                new FloorConfig { FloorLevel = 3 , Programs = new [] {"Hellhound","Hellhound x2","Kraken","Hellhound x3" }},
                new FloorConfig { FloorLevel = 4 , Programs = new [] {"Sabertooth","Hellhound, Killer","Hellhound, Scorpion","Asp x2" }},
                new FloorConfig { FloorLevel = 5 , Programs = new [] {"Raven x2","Skunk x2","Hellhound, Killer","Hellhound, Liche" }},
                new FloorConfig { FloorLevel = 6 , Programs = new [] {"Hellhound","Sabertooth","Raven x2","Wisp x3" }},
                new FloorConfig { FloorLevel = 7 , Programs = new [] {"Wisp","Scorpion","Sabertooth","Hellhound, Sabertooth" }},
                new FloorConfig { FloorLevel = 8 , Programs = new [] {"Raven","Hellhound","Hellhound","Kraken" }},
                new FloorConfig { FloorLevel = 9 , Programs = new [] {"Password DV6","Password DV8","Password DV10","Password DV12" }},
                new FloorConfig { FloorLevel = 10, Programs = new [] {"File DV6","File DV8","File DV10","File DV12" }},
                new FloorConfig { FloorLevel = 11, Programs = new [] {"Control Node DV6","Control Node DV8","Control Node DV10","Control Node DV12" }},
                new FloorConfig { FloorLevel = 12, Programs = new [] {"Password DV6","Password DV8","Password DV10","Password DV12" }},
                new FloorConfig { FloorLevel = 13, Programs = new [] {"Skunk","Asp","Killer","Giant" }},
                new FloorConfig { FloorLevel = 14, Programs = new [] {"Asp","Killer","Liche","Dragon" }},
                new FloorConfig { FloorLevel = 15, Programs = new [] {"Scorpion","Liche","Dragon","Killer, Scorpion" }},
                new FloorConfig { FloorLevel = 16, Programs = new [] {"Killer, Skunk","Asp","Asp, Raven","Kraken" }},
                new FloorConfig { FloorLevel = 17, Programs = new [] {"Wisp x3","Raven x3","Dragon, Wisp","Raven, Wisp, Hellhound" }},
                new FloorConfig { FloorLevel = 18, Programs = new [] {"Liche","Liche, Raven","Giant","Dragon x2" }}
        };

        private string[] lobbyFloor;
        private Dictionary<int, string[]> remainingFloors = new Dictionary<int, string[]>();

        public bool ContinueRunning = true;

        public NetGenRunner()
        {
            Initialize();
        }

        public int RollDice(int count, int size)
        {
            return Enumerable.Range(1, size).OrderBy(x => Guid.NewGuid()).First()
                + (count > 1 ? RollDice(--count, size) : 0);
        }

        private void Initialize()
        {
            if(File.Exists(Path.Combine(BaseDirectory, FloorConfigFileName)))
            {
                string text = File.ReadAllText(Path.Combine(BaseDirectory, FloorConfigFileName));
                this.remainingFloors = JsonConvert.DeserializeObject<List<FloorConfig>>(text).ToDictionary(x=>x.FloorLevel, x=>x.Programs);
            }
            else
            {
                File.WriteAllText(Path.Combine(BaseDirectory, FloorConfigFileName), JsonConvert.SerializeObject(DefaultFloorConfigs, Formatting.Indented));
                this.remainingFloors = DefaultFloorConfigs.ToDictionary(x=>x.FloorLevel, x=>x.Programs);
            }

            if(File.Exists(Path.Combine(BaseDirectory, LobbyFloorConfigFileName)))
            {
                string text = File.ReadAllText(Path.Combine(BaseDirectory, LobbyFloorConfigFileName));
                this.lobbyFloor = JsonConvert.DeserializeObject<string[]>(text);
            }
            else
            {
                File.WriteAllText(Path.Combine(BaseDirectory, LobbyFloorConfigFileName), JsonConvert.SerializeObject(DefaultLobbyFloor, Formatting.Indented));
                this.lobbyFloor = DefaultLobbyFloor;
            }

        }

        public void Run(string[] args)
        {
            
            while(ContinueRunning)
            {
                Console.Clear();

                double difficulty = Util.GetInput("Difficulty in decimals (0-3).\n\t1.5 would be half level 1 half level 2 difficulty",0, 3);
                
                int min = remainingFloors.Keys.Min();
                int max = remainingFloors.Keys.Max();
                int numberOfFloors = Util.GetInput($"Number of floors? {min} - {max}", min, max);

                numberOfFloors = numberOfFloors > 2 ? numberOfFloors : RollDice(3, 6);


                var fileName = $"NetRun_{difficulty}_{numberOfFloors}_";
                var lobby = new Floor { Programs = lobbyFloor.OrderBy(x => Guid.NewGuid()).First() };
                numberOfFloors--;
                fileName += lobby.Programs[0];
                var lobby2 = new Floor { Programs = lobbyFloor.OrderBy(x => Guid.NewGuid()).First(), Parent = lobby };
                lobby.Children = new List<Floor> { lobby2 };
                numberOfFloors--;
                fileName += lobby2.Programs[0];
                var currentFloors = new List<Floor> { lobby2 };

                Random rnd = new Random();

                var maxFloorsWide = 1;

                while (numberOfFloors > 1)
                {
                    var nextFloors = new List<Floor>();
                    foreach (var floor in currentFloors)
                    {
                        var roll = RollDice(3, 6);
                        var childFloor = new Floor { Programs = remainingFloors[roll][(int)Math.Floor(difficulty + rnd.NextDouble())], Parent = floor };
                        floor.Children.Add(childFloor);
                        nextFloors.Add(childFloor);
                        numberOfFloors--;
                        fileName += childFloor.Programs[0];
                        if (numberOfFloors > 1 && RollDice(1, 10) >= 9)
                        {
                            roll = RollDice(3, 6);
                            childFloor = new Floor { Programs = remainingFloors[roll][(int)Math.Floor(difficulty + rnd.NextDouble())], Parent = floor };
                            floor.Children.Add(childFloor);
                            nextFloors.Add(childFloor);
                            numberOfFloors--;
                            fileName += childFloor.Programs[0];
                        }
                    }
                    currentFloors = nextFloors;
                    maxFloorsWide = Math.Max(maxFloorsWide, currentFloors.Count);
                }
                {
                    var floor = currentFloors.OrderBy(x => Guid.NewGuid()).First();
                    var roll = RollDice(3, 6);
                    var childFloor = new Floor { Programs = remainingFloors[roll][(int)Math.Floor(difficulty + rnd.NextDouble())] + ROOT, Parent = floor };
                    floor.Children.Add(childFloor);
                    fileName += childFloor.Programs[0];

                    fileName += "_" + maxFloorsWide;
                }



                int maxWidth = BOX_WIDTH * maxFloorsWide + SPACE_BETWEEN_BOXES * (maxFloorsWide + 1);
                currentFloors = new List<Floor> { lobby };

                var allLines = "";
                while (currentFloors.Any())
                {
                    var currentspaceBetween = (maxWidth - (BOX_WIDTH * currentFloors.Count())) / (currentFloors.Count() + 1);

                    var formattedLevel = new List<string>();
                    Enumerable.Range(0, BOX_HEIGHT).ToList().ForEach(l =>
                         formattedLevel.Add(""));

                    var formattedArrows = new List<string>();
                    Enumerable.Range(0, SPACE_BETWEEN_LEVELS).ToList().ForEach(l =>
                         formattedArrows.Add(""));


                    var nextFloors = new List<Floor>();
                    for (var i = 0; i < currentFloors.Count; i++)
                    {
                        Enumerable.Range(0, BOX_HEIGHT).ToList().ForEach(l =>
                             formattedLevel[l] += new string(' ', currentspaceBetween));

                        currentFloors[i].LeftStart = formattedLevel.Last().Length;

                        if (currentFloors[i].Parent != null)
                        {
                            var parent = currentFloors[i].Parent;

                            Enumerable.Range(0, SPACE_BETWEEN_LEVELS).ToList().ForEach(l =>
                            {
                                if (parent.LeftStart - formattedArrows[l].Length + (BOX_WIDTH / 2) >= 0)
                                    formattedArrows[l] += new string(' ', parent.LeftStart - formattedArrows[l].Length + (BOX_WIDTH / 2)) + (l <= SPACE_BETWEEN_LEVELS / 2 ? PIPE : "");
                                else
                                    formattedArrows[l] = formattedArrows[l].Substring(0, parent.LeftStart + (BOX_WIDTH / 2)) + (l <= SPACE_BETWEEN_LEVELS / 2 ? PIPE : "");
                            });

                            Enumerable.Range(0, SPACE_BETWEEN_LEVELS).ToList().ForEach(l =>
                            {
                                if (currentFloors[i].LeftStart - formattedArrows[l].Length + (BOX_WIDTH / 2) >= 0)
                                    formattedArrows[l] += l == SPACE_BETWEEN_LEVELS / 2 ?
                                                    new string(RIGHT_LINK, currentFloors[i].LeftStart - formattedArrows[l].Length + (BOX_WIDTH / 2) + 1)
                                                      : l > SPACE_BETWEEN_LEVELS / 2 ? new string(' ', currentFloors[i].LeftStart - formattedArrows[l].Length + (BOX_WIDTH / 2)) + PIPE
                                                      : "";
                                else
                                    formattedArrows[l] = l == SPACE_BETWEEN_LEVELS / 2 ?
                                                      formattedArrows[l].Substring(0, currentFloors[i].LeftStart + (BOX_WIDTH / 2)) + (currentFloors[i].LeftStart != parent.LeftStart ? new string(LEFT_LINK, parent.LeftStart - currentFloors[i].LeftStart) : PIPE)
                                                      : l > SPACE_BETWEEN_LEVELS / 2 ? formattedArrows[l].Substring(0, currentFloors[i].LeftStart + (BOX_WIDTH / 2)) + PIPE
                                                      : formattedArrows[l];
                            });
                        }

                        nextFloors.AddRange(currentFloors[i].Children);

                        var programWords = new Stack<string>(currentFloors[i].Programs.Split(' ').Reverse());
                        var formattedPrograms = new List<string> { programWords.Pop() };
                        //split
                        while (programWords.Any())
                        {
                            if ((formattedPrograms.Last() + " " + programWords.Peek()).Length < BOX_WIDTH - 4)
                            {
                                formattedPrograms[formattedPrograms.Count - 1] = formattedPrograms.Last() + " " + programWords.Pop();
                            }
                            else
                            {
                                formattedPrograms.Add(programWords.Pop());
                            }
                        }
                        //center text, add border
                        for (var line = 0; line < formattedPrograms.Count; line++)
                        {
                            formattedPrograms[line] = LEFT_WALL + new string(' ', (BOX_WIDTH - 2 - formattedPrograms[line].Length) / 2) + formattedPrograms[line];
                            formattedPrograms[line] += new string(' ', BOX_WIDTH - 1 - formattedPrograms[line].Length) + RIGHT_WALL;
                        }

                        int insertBlankLines = (BOX_HEIGHT - formattedPrograms.Count) / 2;
                        for (var line = 0; line < insertBlankLines; line++)
                        {
                            formattedPrograms.Insert(0, line == insertBlankLines - 1 ? (TOP_LEFT + new string(TOP_BORDER, BOX_WIDTH - 2) + TOP_RIGHT) : LEFT_WALL + new string(' ', BOX_WIDTH - 2) + RIGHT_WALL);
                        }
                        int appendBlankLines = BOX_HEIGHT - formattedPrograms.Count;
                        for (var line = 0; line < appendBlankLines; line++)
                        {
                            formattedPrograms.Add(line == appendBlankLines - 1
                            ? (BOTTOM_LEFT + new string(line == appendBlankLines - 1 ? BOTTOM_BORDER : ' ', BOX_WIDTH - 2) + BOTTOM_RIGHT)
                            : (LEFT_WALL + new string(line == appendBlankLines - 1 ? BOTTOM_BORDER : ' ', BOX_WIDTH - 2) + RIGHT_WALL));
                        }

                        Enumerable.Range(0, BOX_HEIGHT).ToList().ForEach(l =>
                             formattedLevel[l] += formattedPrograms[l]);


                    }
                    foreach (var line in formattedArrows)
                    {
                        allLines += "\r\n" + line;
                        Console.WriteLine(line);
                    }

                    foreach (var line in formattedLevel)
                    {
                        allLines += "\r\n" + line;
                        Console.WriteLine(line);
                    }
                    currentFloors = nextFloors;

                }
                DrawText(allLines, new Font(FontFamilyName, 10), Color.Lime, Color.Black, fileName + ".jpeg");

                ContinueRunning = Util.GetBool("Do you want to continue? [yes|no]");
            }
        }
        
        public Image DrawText(string text, Font font, Color textColor, Color backColor, string fileName)
        {
            // Create dummy bitmap
            Image image = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(image);

            // Measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            // free up the dummy image
            image.Dispose();
            drawing.Dispose();

            image = new Bitmap((int)textSize.Width, (int)textSize.Height);
            drawing = Graphics.FromImage(image);

            // paint background
            drawing.Clear(backColor);

            // Create a brush for the text
            Brush textBrush = new SolidBrush(textColor);
            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            Console.WriteLine(path);
            image.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            return image;
        }
    }
}
