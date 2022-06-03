using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoWantsToBeAMillionaire.Game
{
    internal class Question
    {
        public string Title { get; set; }
        public string TrueAnswer { get; set; }
        public string FalseAnswer1 { get; set; }
        public string FalseAnswer2 { get; set; }
        public string FalseAnswer3 { get; set; }
        public int QuestionLevel { get; set; }
        public bool QuestionWasUsed { get; set; } = false;
        public String[] RandomAnswersArr { get; private set; }

        public void RandomAnswers()
        {
            var res = new String[] { TrueAnswer, FalseAnswer1, FalseAnswer2, FalseAnswer3 };
            for (int i = res.Length - 1; i >= 1; i--)
            {
                int j = Services.random.Next(i + 1);
                var temp = res[j];
                res[j] = res[i];
                res[i] = temp;
            }
            RandomAnswersArr = res;
        }

        public override string ToString()
        {
            return $"{Title}\n1. {RandomAnswersArr[0]}\n2. {RandomAnswersArr[1]}\n3. {RandomAnswersArr[2]}\n4. {RandomAnswersArr[3]}\n";
        }
    }
}
