using System;

namespace Discord
{
    /// <summary>
    ///     A Unicode emoji.
    /// </summary>
    public class Emoji : IEmote, IEquatable<Emoji>
    {
        // TODO: need to constrain this to Unicode-only emojis somehow

        /// <inheritdoc />
        public string Name { get; }
        /// <summary>
        ///     Gets the Unicode representation of this emote.
        /// </summary>
        /// <returns>
        ///     A string that resolves to <see cref="Emoji.Name"/>.
        /// </returns>
        public override string ToString() => Name;

        /// <summary>
        ///     Initializes a new <see cref="Emoji"/> class with the provided Unicode.
        /// </summary>
        /// <param name="unicode">The pure UTF-8 encoding of an emoji.</param>
        public Emoji(string unicode)
        {
            Name = unicode;
        }

        public bool Equals(Emoji other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is not Emoji other)
                return false;

            return Equals(other);
        }

        public override int GetHashCode() => Name != null ? Name.GetHashCode() : 0;

        public static bool operator ==(Emoji left, Emoji right) => Equals(left, right);

        public static bool operator !=(Emoji left, Emoji right) => !Equals(left, right);
    }
}
