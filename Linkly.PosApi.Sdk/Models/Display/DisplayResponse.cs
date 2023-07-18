using System.Collections.ObjectModel;
using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.Models.Display
{
    /// <summary>
    /// This message is returned asynchronously when a display event occurs on the PIN pad. To handle
    /// display events refer to <see cref="IPosApiEventListener.Display" />.
    /// </summary>
    public class DisplayResponse : PosApiResponse
    {
        /// <summary>Number of lines to display.</summary>
        public int NumberOfLines { get; set; }

        /// <summary>Number of character per display line.</summary>
        public int LineLength { get; set; }

        /// <summary>Text to be displayed. Each display line is concatenated.</summary>
        /// <example>[&quot;     SWIPE CARD     &quot;,&quot;                    &quot;]</example>
        public Collection<string> DisplayText { get; set; } = new Collection<string>();

        /// <summary>Indicates whether the Cancel button is to be displayed.</summary>
        public bool CancelKeyFlag { get; set; }

        /// <summary>Indicates whether the Accept/Yes button is to be displayed.</summary>
        public bool AcceptYesKeyFlag { get; set; }

        /// <summary>Indicates whether the Decline/No button is to be displayed.</summary>
        public bool DeclineNoKeyFlag { get; set; }

        /// <summary>Indicates whether the Authorise button is to be displayed.</summary>
        public bool AuthoriseKeyFlag { get; set; }

        /// <summary>Indicates whether the OK button is to be displayed.</summary>
        public bool OkKeyFlag { get; set; }

        public InputType InputType { get; set; }

        public GraphicCode GraphicCode { get; set; }
    }
}