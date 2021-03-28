using System;

namespace projektlabor.noah.planmeldung.datahandling.entities
{
    class TimespentEntity
    {

        /// The unique id of the timespent entity
        public int Id { get; set; }

        /// When the user started to work
        public DateTime Start { get; set; }

        /// When the user ended his work
        /// Nullable
        public DateTime Stop { get; set; }

        /// If the day-end stopped the work
        public bool Enddisconnect { get; set; }

        /// The user's id this timespententity belongs to
        public int UserId { get; set; }
    }
}
