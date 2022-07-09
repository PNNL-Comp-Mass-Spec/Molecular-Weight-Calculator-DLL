namespace MolecularWeightCalculator.Tools
{
    public static class StringTools
    {
        /// <summary>
        /// Adds spaces to <paramref name="work"/> until the length is <paramref name="length"/>
        /// </summary>
        /// <param name="work"></param>
        /// <param name="length"></param>
        public static string SpacePad(string work, short length)
        {
            if (work.Length < length)
            {
                work += new string(' ', length - work.Length);
            }

            while (work.Length < length)
            {
                work += " ";
            }

            return work;
        }

        public static string SpacePadFront(string work, short length)
        {
            if (work.Length < length)
            {
                work = new string(' ', length - work.Length) + work;
            }

            while (work.Length < length)
            {
                work = " " + work;
            }

            return work;
        }
    }
}
