using System;

namespace WhoWantsToBeAMillionaire
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Game.Game game = new Game.Game();

            while (true)
            {
                // Menu
                switch (game.MainMenu())
                {
                    case Game.Menu.StartGame: game.StartGame(); break;
                    case Game.Menu.ShowRecords: game.ShowRecords(); break;
                    case Game.Menu.AboutGame: game.AboutGame(); break;
                    case Game.Menu.Rules: game.Rules(); break;
                    case Game.Menu.RefreshQuestions: game.GetQuestionsFromJson(); break;
                    case Game.Menu.Exit: return;
                }
            }
        }
    }
}
