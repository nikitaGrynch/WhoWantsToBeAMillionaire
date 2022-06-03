using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace WhoWantsToBeAMillionaire.Game
{
    /// <summary>
    /// Game menu
    /// </summary>
    enum Menu
    {
        StartGame,
        ShowRecords,
        AboutGame,
        Rules,
        RefreshQuestions,
        Exit
    }
    internal class Game
    {
        Player.Player Player { get; set; }
        private List<Question>[] Questions;                     // all questions and answers received from a file
        private string GameLanguage;                            // game language affects the language of questions, answers and interface
        private const string RecordsFileName = "records.xml";   // file name for save and get records
        private List<Record> Records;                           // all records received from a file
        private static int[] QuestionCost;                      // the money the player gets after each successful answer on question
        Regex nameTemplateReg = new Regex(
            @"^([A-ZА-Я][a-zа-я]*')?[A-ZА-Я][a-zа-я]+(\s[A-ZА-Я][a-zа-я]+([-\s][A-ZА-Я][a-zа-я]+)?)+$");  // template for right name by regular expression


        public Game()
        {
            GameLanguage = "eng";  // default language - English
            QuestionCost = new int[] { 100, 200, 300, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000, 125000, 250000, 500000, 1000000 };
            ChooseLanguage();
            GetQuestionsFromJson();
            GetRecordsFromXML();
        }

        /// <summary>
        /// Choose game langueage
        /// </summary>
        public void ChooseLanguage()
        {
            Console.WriteLine("Default language - English. Do you want to switch to Russian? (1 - yes, 2 - no)");
            var ansKey = Console.ReadKey(true);
            while (true)
            {
                if (ansKey.KeyChar == '1')
                {
                    GameLanguage = "rus";
                    break;
                }
                else if (ansKey.KeyChar == '2')
                    break;
                ansKey = Console.ReadKey(true);
            }
        }

        #region Receive data

        public void GetQuestionsFromJson()
        {
            String fileName = String.Empty;
            if (GameLanguage == "rus")
                fileName = "questionsRus.json";
            else if (GameLanguage == "eng")
                fileName = "questionsEng.json";

            List<Question> allQuestions;
            try
            {
                if (File.Exists(fileName))
                {
                    using (var reader = new StreamReader(fileName))
                    {
                        allQuestions = JsonSerializer.Deserialize<List<Question>>(reader.ReadToEnd());
                    }
                }
                else
                {
                    Questions = null;
                    throw new Exception(GameLanguage == "rus" ?
                        "Файл с вопросами не найден" :
                        "Cannot find file with questions");
                }
                if (allQuestions.Count() == 0)
                {
                    Questions = null;
                    throw new Exception(GameLanguage == "rus" ?
                        "Ошибка: Неверный или пустой файл" :
                        "Error: Invalid or empty file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Questions = new List<Question>[5];  // 5 - question levels 1 - 5
            for (int i = 0; i < 5; i++)
            {
                Questions[i] = new List<Question>();
            }
            foreach(var question in allQuestions)
            {
                Questions[question.QuestionLevel - 1].Add(question);
            }
            if (Questions[0].Count() < 3 && Questions[1].Count() < 3 && Questions[2].Count() < 3 &&
                Questions[3].Count() < 3 && Questions[4].Count() < 3)
            {
                Questions = null;
                Console.WriteLine(GameLanguage == "rus" ?
                    "Недостаточное количество вопросов (минимум 3 вопроса на каждый уровень. Количество уровней - 5)" :
                    "Not enough questions (min 3 questions per level. Count of level - 5)");
            }
            Console.WriteLine("Questions was received");
            
        }

        public void GetRecordsFromXML()
        {
            Records = new List<Record>();
            XmlSerializer xmlDeserializer = new XmlSerializer(Records.GetType());
            try
            {
                using (var recordsFile = new StreamReader(RecordsFileName))
                {
                    Records = (List<Record>)xmlDeserializer.Deserialize(recordsFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(GameLanguage == "rus" ? "Неверный файл либо файл не найден" :
                    "Invalid file or file was not found");
            }
        }

        #endregion 

        /// <summary>
        /// Get right level question from array of list with question
        /// </summary>
        /// <param name="level">Question level</param>
        /// <returns></returns>
        private Question GetQuestion(int level)
        {
            int countQuestions = Questions[level - 1].Count;
            Question res = null;
            while(true)
            {
                countQuestions = Questions[level - 1].Count;
                res = Questions[level - 1][Services.random.Next(countQuestions)];
                if (!res.QuestionWasUsed)
                {
                    res.RandomAnswers();
                    res.QuestionWasUsed = true;
                    break;
                }
            }
            return res;
        }

        public void SaveRecordsToXML()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(Records.GetType());
            using(var recordsFile = new StreamWriter(RecordsFileName))
            {
                xmlSerializer.Serialize(recordsFile, Records);
            }
        }

        #region Menu

        /// <summary>
        /// Launch when user choose Start Game in menu
        /// </summary>
        public void StartGame()
        {
            if (Questions == null)
            {
                Console.WriteLine(GameLanguage == "rus" ?
                        "Вопросы не найдены" :
                        "Questions have not found");
                return;
            }
            
            while (true)
            {
                Console.WriteLine(GameLanguage == "rus" ? "Введите ваше имя (Имя Фамилия)" : "Enter your name (Name Surname)");
                String playerName;
                do
                {
                    Console.Write(GameLanguage == "rus" ? "Ваше имя: " : "Your name: ");
                    playerName = Console.ReadLine();
                    // check input for correct name
                    if(nameTemplateReg.IsMatch(playerName))
                        break;
                    else
                    {
                        Console.WriteLine(GameLanguage == "rus" ? "Имя введено неверно" : "Incorrect name");
                    }
                }
                while (true);
                // Create new player
                Player = new Player.Player { Name = playerName };
                break;
            }
            Question question;
            ConsoleKeyInfo key;
            for (int i = 1; i <= 15; i++)
            {
                Console.WriteLine("------------");
                Console.WriteLine(i + " " + (GameLanguage == "rus"? "Вопрос" : "Question"));
                Console.WriteLine(question = GetQuestion(i % 3 == 0 ? i / 3 : i / 3 + 1)); // Choose question level
                Console.WriteLine("------------");
                int ans = 0;
                Console.Write(GameLanguage == "rus" ? "Ваш ответ: " : "Your answer: ");
                do
                {
                    key = Console.ReadKey(true);
                } while (!(key.KeyChar == '1' || key.KeyChar == '2' || key.KeyChar == '3' || key.KeyChar == '4'));
                switch (key.KeyChar)
                {
                    case '1': ans = 1; break;
                    case '2': ans = 2; break;
                    case '3': ans = 3; break;
                    case '4': ans = 4; break;
                }
                Console.WriteLine(question.RandomAnswersArr[ans - 1]);
                Thread.Sleep(300);
                // check answer
                if (question.RandomAnswersArr[ans - 1] == question.TrueAnswer)
                {
                    // Right answer
                    Console.WriteLine(GameLanguage == "rus" ? "Верно!" : "Right you are!");
                    Player.Balance = QuestionCost[i-1];
                    Console.WriteLine((GameLanguage == "rus" ? "Ваш баланс: " : "Your balance: ") + Player.Balance + "$");
                    if(i == 5 || i == 10)
                    {
                        Player.FireproofAmount = QuestionCost[i-1];
                        Console.WriteLine((GameLanguage == "rus" ? "Несгорамемая сумма: " : "Fireproof amount: ") + Player.FireproofAmount + "$!");
                    }
                }
                else
                {
                    // Wrong answer
                    Console.WriteLine(GameLanguage == "rus" ? "Неверно!" : "You are wrong!");
                    if (Player.FireproofAmount > 0)
                    {
                        // if player achive fireproof amount
                        Player.Balance = Player.FireproofAmount;
                        Console.WriteLine((GameLanguage == "rus" ? "Вы выиграли только " : "You won only ") + Player.Balance + "$");
                        Records.Add(new Record { Name = Player.Name, Money = Player.Balance });
                        Console.WriteLine("--------------------");
                        new Thread(SaveRecordsToXML).Start();
                    }
                    else
                    {
                        Console.WriteLine(GameLanguage == "rus" ? "К сожалению, вы проиграли " : "Unfortunately you lost ");
                        Console.WriteLine("--------------------");
                    }
                    return;
                }
                Console.WriteLine("--------------------");

            }
            // if player win
            Console.WriteLine(GameLanguage == "rus" ? "Поздравляем! Вы выиграли один миллион долларов!" : "Congratulations! You won a million dollars!");
            Records.Add(new Record { Name = Player.Name, Money = Player.Balance });
            new Thread(SaveRecordsToXML).Start();
            Console.WriteLine("--------------------");
        }

        /// <summary>
        /// Launch when user choose Show Records in menu
        /// </summary>
        public void ShowRecords()
        {
            Console.WriteLine(GameLanguage == "rus" ? "Список рекордов" : "Record list");
            Console.WriteLine("--------------------");
            if (Records == null || Records.Count == 0)
            {
                Console.WriteLine(GameLanguage == "rus" ? "Список рекордов пуст" : "Record list is emty");
                return;
            }
            foreach (var record in Records)
                Console.WriteLine(record);
            Console.WriteLine("--------------------");
        }

        /// <summary>
        /// Launch when user choose About Game in menu
        /// </summary>
        public void AboutGame()
        {
            Console.WriteLine(GameLanguage == "rus" ? "Количество вопросов" : "Count of questions");
            Console.WriteLine("--------------------");
            
            if(Questions == null)
            {
                Console.WriteLine(GameLanguage == "rus" ? "Список вопросов пуст" : "List of questions is empty");
                return;
            }
            else
            {
                var allQuestions = () =>
                {
                    int res = 0;
                    foreach (var question in Questions)
                        res += question.Count();
                    return res;
                };
                Console.WriteLine($"{(GameLanguage == "rus" ? "Всего вопросов: " : "All questions: ")} {allQuestions()}");
                for (int i = 0; i < Questions.Length; i++)
                {
                    Console.WriteLine(GameLanguage == "rus" ? $"Вопросов {i + 1} уровня: {Questions[i].Count}" :
                        $"Questions of {i + 1} level: {Questions[i].Count}");
                }
            }

        }

        /// <summary>
        /// Launch when user choose Rules in menu
        /// </summary>
        public void Rules()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://ru.wwbm.com/rules", UseShellExecute = true });
                return;
            }catch (Exception ex)
            {
                Console.WriteLine(GameLanguage == "rus" ? "Ошибка открытия браузера" : "Browser openning error" + ex.Message);
            }
            /*
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    Console.WriteLine(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Console.WriteLine(other.Message);
            }
            */
            
        }

        #endregion

        public Menu MainMenu()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine(GameLanguage == "rus" ? "Главное меню" : "Main menu");
            Console.WriteLine("--------------------");
            Console.WriteLine($"A. {(GameLanguage == "rus" ? "Начать игру" : "Start game")}");
            Console.WriteLine($"B. {(GameLanguage == "rus" ? "Список рекордов" : "Record list")}");
            Console.WriteLine($"C. {(GameLanguage == "rus" ? "Об игре" : "About game")}");
            Console.WriteLine($"D. {(GameLanguage == "rus" ? "Правила" : "Rules")}");
            Console.WriteLine($"E. {(GameLanguage == "rus" ? "Обновить вопросы" : "Refresh questions")}");
            Console.WriteLine($"F. {(GameLanguage == "rus" ? "Выход" : "Exit")}");
            Console.WriteLine((GameLanguage == "rus" ? "Для выбора нажмите соответствующую клавишу на клавиатуре" :
                "To select, press the corresponding key on the keyboard"));
            Console.WriteLine("--------------------");
            Menu menu;
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.A) { menu = Menu.StartGame; break; }
                else if (key.Key == ConsoleKey.B) { menu = Menu.ShowRecords; break; }
                else if (key.Key == ConsoleKey.C) { menu = Menu.AboutGame; break; }
                else if (key.Key == ConsoleKey.D) { menu = Menu.Rules; break; }
                else if (key.Key == ConsoleKey.E) { menu = Menu.RefreshQuestions; break; }
                else if (key.Key == ConsoleKey.F) { menu = Menu.Exit; break; }

            }
            return menu;

        }


    }
}
