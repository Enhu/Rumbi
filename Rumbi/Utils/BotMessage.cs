using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rumbi.Utils
{
    public class BotMessage
    {
        // General Messages
        public static string InternalError { get; } =
            $"❌ An error ocurred, please contact enhu on discord.";

        // FAQ Module
        public static string QuestionsEmpty { get; } = "⚠️ No questions found, please add some.";

        public static string QuestionNotFound(string identifier)
        {
            return $"⚠️ Question with identifier '{identifier}' not found";
        }

        public static string QuestionAlreadyExists(string identifier)
        {
            return $"⚠️ Question with identifier '{identifier}' already exists";
        }

        public static string QuestionEdited { get; } = "✅ Question Edited";
        public static string QuestionAdded { get; } = "✅ Question added";
        public static string QuestionDeleted { get; } = "✅ Question deleted";
        public static string QuestionSelect { get; } = "Select a question...";
        public static string QuestionModalAdd { get; } = "Add strat question";
        public static string QuestionModalEdit { get; } = "Edit strat question";
        public static string QuestionWelcomeMessage { get; } =
            "These are the most frequenlty asked questions about strats. \n"
            + "If you don't understand something in here, or your specific doubt/question"
            + "isn't covered please don't hesitate to ask! \n";
    }
}
