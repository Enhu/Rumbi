using Discord;
using Discord.Interactions;

namespace Rumbi.Modals
{
    public class QuestionModal : IModal
    {
        public int QuestionId { get; set; } = 0;

        [InputLabel("Question")]
        [ModalTextInput("strat_label", TextInputStyle.Short, "What's a sdj?", maxLength: 50)]
        public string Label { get; set; }

        [InputLabel("Short Unique Idenfitier")]
        [ModalTextInput("strat_value", TextInputStyle.Short, "sdj", maxLength: 15)]
        public string Identifier { get; set; } = string.Empty;

        [InputLabel("Description")]
        [RequiredInput(false)]
        [ModalTextInput(
            "strat_desc",
            TextInputStyle.Short,
            "A short desc. (optional)",
            maxLength: 15
        )]
        public string? Description { get; set; } = null;

        [InputLabel("Answer")]
        [ModalTextInput(
            "strat_content",
            TextInputStyle.Paragraph,
            maxLength: 3000,
            placeholder: "A **sprint double jump!** :thumbs_up:"
        )]
        public string Content { get; set; }

        public string Title => "Strats";
    }
}